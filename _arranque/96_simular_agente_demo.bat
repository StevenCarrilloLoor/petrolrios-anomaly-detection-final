@echo off
chcp 65001 >nul
title PetrolRios - DEMO: Simular agente con nueva venta
echo ============================================================
echo  SIMULACION EN VIVO DEL STATION AGENT
echo  - Hace login con JWT como agente de EST-001
echo  - Empuja 3 transacciones recien "ocurridas" al API
echo  - Cada una dispara un detector distinto
echo  - Despues, el job de Hangfire las procesa en 30s
echo ============================================================
echo.

set "API=http://localhost:5170"
set "EMAIL=agent-est-001@petrolrios.com"
set "PWD=Agent123!"

echo [1/3] Login del agente...
curl -s -X POST "%API%/api/v1/auth/login" -H "Content-Type: application/json" -d "{\"email\":\"%EMAIL%\",\"password\":\"%PWD%\"}" > "%TEMP%\login.json"

for /f "delims=" %%T in ('powershell -NoProfile -Command "(Get-Content '%TEMP%\login.json' | ConvertFrom-Json).token"') do set "JWT=%%T"

if "%JWT%"=="" (
    echo   [ERROR] No pude obtener JWT. Revisa que el API este corriendo en :5170
    pause
    exit /b 1
)
echo   JWT: %JWT:~0,40%...
echo.

set "NOW="
for /f %%D in ('powershell -NoProfile -Command "(Get-Date).ToUniversalTime().ToString('yyyy-MM-ddTHH:mm:ss.fffZ')"') do set "NOW=%%D"

echo [2/3] Empujando lote con 3 transacciones reales recien capturadas...

> "%TEMP%\lote_demo.json" (
echo {
echo   "codigoEstacion": "EST-001",
echo   "transacciones": [
echo     {
echo       "tipoTransaccion": "Factura",
echo       "dataJson": "{\"SecuenciaDocumento\":500001,\"TipoDocumento\":\"FAC\",\"NumeroDocumento\":\"001-001-000500001\",\"FechaDocumento\":\"%NOW%\",\"CodigoCliente\":\"CLI-DEMO-A\",\"TotalNeto\":31.25,\"TotalSinIva\":27.90,\"Descuento\":0,\"Iva\":3.35,\"CodigoVendedor\":\"EMP-007\",\"CodigoPago\":\"EF\",\"Placa\":\"ZZZ999949\",\"RucCliente\":\"1700000500001\",\"NumeroTurno\":50001,\"Subtotal\":27.90,\"NumeroConsecutivo\":1,\"CodigoChofer\":\"\",\"CodigoManguera\":\"M-01\"}",
echo       "fechaOriginal": "%NOW%"
echo     },
echo     {
echo       "tipoTransaccion": "DetalleFactura",
echo       "dataJson": "{\"NumeroDespacho\":500100,\"CodigoManguera\":\"M-01\",\"FechaDespacho\":\"%NOW%\",\"VolumenTotal\":9.0,\"Cantidad\":9.0,\"ValorUnitario\":3.47,\"CodigoProducto\":\"EXTRA\",\"NombreProducto\":\"Gasolina Extra\",\"CodigoCliente\":\"CLI-DEMO-A\"}",
echo       "fechaOriginal": "%NOW%"
echo     },
echo     {
echo       "tipoTransaccion": "CierreTurno",
echo       "dataJson": "{\"NumeroTurno\":50001,\"CodigoVendedor\":\"EMP-007\",\"FechaInicio\":\"%NOW%\",\"FechaFin\":\"%NOW%\",\"SaldoInicial\":100.00,\"Ingresos\":920.00,\"Egresos\":40.00,\"SaldoFinal\":885.00,\"Faltante\":95.00,\"Sobrante\":0.00,\"Creditos\":0.00}",
echo       "fechaOriginal": "%NOW%"
echo     }
echo   ]
echo }
)

curl -s -X POST "%API%/api/v1/ingesta" -H "Content-Type: application/json" -H "Authorization: Bearer %JWT%" --data-binary "@%TEMP%\lote_demo.json"
echo.
echo.

echo [3/3] Disparando el job de Hangfire para procesar AHORA...
powershell -NoProfile -Command "$body='jobs[]=anomaly-detection'; Invoke-WebRequest -Uri '%API%/hangfire/recurring/trigger' -Method POST -Body $body -ContentType 'application/x-www-form-urlencoded' -UseBasicParsing | Out-Null" 2>nul

echo.
echo ============================================================
echo  Lote empujado. Lo que pasara en los proximos 10-30 s:
echo    1. El job de Hangfire procesara las 3 transacciones
echo    2. ComplianceViolationDetector vera placa ZZZ999949 con 9 gal -^> ALERTA Alto
echo    3. CashFraudDetector vera Faltante = $95 ^> $50 umbral    -^> ALERTA Medio
echo    4. Las alertas apareceran en el dashboard automaticamente
echo.
echo  Abre / refresca: http://localhost:5173/alertas
echo ============================================================
pause
