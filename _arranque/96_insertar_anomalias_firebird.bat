@echo off
setlocal
rem ============================================================
rem  Inserta ventas nuevas (con anomalias) en la BD Firebird REAL.
rem  Simula al POS Contaplus registrando transacciones en vivo.
rem  El Station Agent las detectara por watermark en su proximo
rem  ciclo (60 s) y las empujara a /api/v1/ingesta.
rem ============================================================
cd /d "%~dp0"

echo [1/2] Copiando script SQL al contenedor...
docker cp "inserciones_anomalias.sql" petrolrios-firebird:/tmp/ins.sql
if errorlevel 1 (
  echo ERROR: el contenedor petrolrios-firebird no esta corriendo. Ejecuta 05_firebird_demo.bat
  pause
  exit /b 1
)

echo [2/2] Ejecutando inserciones contra CONTAC.FDB...
docker exec petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -pas masterkey /firebird/data/CONTAC.FDB -i /tmp/ins.sql > inserciones_resultado.log 2>&1

type inserciones_resultado.log
echo.
echo Inserciones aplicadas: 12 facturas (DCTO) + 1 despacho (DESP) + 1 turno (TURN).
echo El Station Agent las tomara en su proximo ciclo (max 60 s)
echo y el job de Hangfire generara las alertas (siguiente ciclo de 5 min
echo o "Trigger now" en localhost:5170/hangfire).
pause
