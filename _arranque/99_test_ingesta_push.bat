@echo off
chcp 65001 >nul
title PetrolRios - Test push del Station Agent
echo ============================================================
echo  Test del modelo push (Alternativa B de tesis):
echo    1. Login como agent-est-001 (JWT)
echo    2. POST /api/v1/ingesta con un lote real
echo    3. Verificar staging en Postgres
echo    4. Disparar el job Hangfire
echo    5. Comprobar nuevas alertas
echo ============================================================
echo.

set "API=http://localhost:5170"
set "EMAIL=agent-est-001@petrolrios.com"
set "PWD=Agent123!"

REM ---- 1) LOGIN ----
echo [1/5] POST %API%/api/v1/auth/login ...
curl -s -X POST "%API%/api/v1/auth/login" ^
    -H "Content-Type: application/json" ^
    -d "{\"email\":\"%EMAIL%\",\"password\":\"%PWD%\"}" > "%TEMP%\ingesta_login.json"

if errorlevel 1 (
    echo   [ERROR] curl fallo.
    pause
    exit /b 1
)

echo   Respuesta login:
type "%TEMP%\ingesta_login.json"
echo.
echo.

REM Extraer el token (chapuza en .bat: usar findstr + for + delims)
REM Mejor: usar PowerShell embebido para parsear el JSON.
for /f "delims=" %%T in ('powershell -NoProfile -Command "(Get-Content '%TEMP%\ingesta_login.json' | ConvertFrom-Json).token"') do set "JWT=%%T"

if "%JWT%"=="" (
    echo   [ERROR] No pude extraer accessToken. Revisa la respuesta arriba.
    pause
    exit /b 1
)

echo   JWT obtenido: %JWT:~0,40%...
echo.

REM ---- 2) PREPARAR LOTE ----
echo [2/5] Preparando lote con 2 transacciones de prueba (StationAgent push)...
set "NOW="
for /f %%D in ('powershell -NoProfile -Command "(Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ')"') do set "NOW=%%D"
echo   Timestamp: %NOW%

> "%TEMP%\ingesta_payload.json" (
echo {
echo   "codigoEstacion": "EST-001",
echo   "transacciones": [
echo     {
echo       "tipoTransaccion": "Factura",
echo       "dataJson": "{\"SecuenciaDocumento\":900001,\"TipoDocumento\":\"FAC\",\"NumeroDocumento\":\"001-001-000900001\",\"FechaDocumento\":\"%NOW%\",\"CodigoCliente\":\"CLI-PUSH\",\"TotalNeto\":42.50,\"TotalSinIva\":37.95,\"Descuento\":0,\"Iva\":4.55,\"CodigoVendedor\":\"EMP-099\",\"CodigoPago\":\"EF\",\"Placa\":\"ZZZ999949\",\"RucCliente\":\"1700000099001\",\"NumeroTurno\":99001,\"Subtotal\":37.95,\"NumeroConsecutivo\":901,\"CodigoChofer\":\"\",\"CodigoManguera\":\"M-09\",\"PushTest\":\"PUSH-TEST\"}",
echo       "fechaOriginal": "%NOW%"
echo     },
echo     {
echo       "tipoTransaccion": "DetalleFactura",
echo       "dataJson": "{\"NumeroDespacho\":900100,\"CodigoManguera\":\"M-09\",\"FechaDespacho\":\"%NOW%\",\"VolumenTotal\":12.0,\"Cantidad\":12.0,\"ValorUnitario\":3.54,\"CodigoProducto\":\"EXTRA\",\"NombreProducto\":\"Gasolina Extra\",\"CodigoCliente\":\"CLI-PUSH\",\"PushTest\":\"PUSH-TEST\"}",
echo       "fechaOriginal": "%NOW%"
echo     }
echo   ]
echo }
)
echo   Lote escrito en %TEMP%\ingesta_payload.json
echo.

REM ---- 3) POST INGESTA ----
echo [3/5] POST %API%/api/v1/ingesta con JWT del agente...
curl -s -X POST "%API%/api/v1/ingesta" ^
    -H "Content-Type: application/json" ^
    -H "Authorization: Bearer %JWT%" ^
    --data-binary "@%TEMP%\ingesta_payload.json"
echo.
echo.

REM ---- 4) VERIFICAR EN POSTGRES ----
echo [4/5] Verificando inserciones en transacciones_staging...
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -c ^
    "SELECT \"Id\", \"EstacionId\", \"TipoTransaccion\", \"FechaOriginal\", \"Procesada\" FROM transacciones_staging WHERE \"DataJson\"::text LIKE '%%PUSH-TEST%%' ORDER BY \"Id\" DESC LIMIT 5;"
echo.

REM ---- 5) DISPARAR HANGFIRE Y CONTAR ALERTAS ----
echo [5/5] Conteo de alertas antes del trigger:
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -t -c ^
    "SELECT COUNT(*) FROM alertas WHERE \"EstacionId\"=1;"
echo.
echo   El job se disparara automaticamente en menos de 5 minutos.
echo   Para verlo ya mismo: abre http://localhost:5170/hangfire/recurring
echo   y haz Trigger now en 'anomaly-detection'.
echo.
echo ============================================================
echo  Test de push completado.
echo ============================================================
echo.
pause
