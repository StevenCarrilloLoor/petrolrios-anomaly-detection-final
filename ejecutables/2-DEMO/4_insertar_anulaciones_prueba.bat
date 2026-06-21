@echo off
setlocal
title PetrolRios - Insertar anulaciones de prueba (E2E) en Firebird
rem ============================================================
rem  Inserta anulaciones NUEVAS (tabla ANUL) en la BD Firebird REAL.
rem  Simula al POS Contaplus anulando comprobantes en vivo. El Station
rem  Agent las tomara por watermark (FECHAANULACION) en su proximo ciclo
rem  y las enviara a la central como la fuente configurable "Anulaciones".
rem  El job de Hangfire dispara la regla "PRUEBA E2E - Exceso de
rem  anulaciones" (Anulaciones: NUMAN > 0) generando alertas de Auditoria.
rem ============================================================
cd /d "%~dp0..\..\_arranque"

echo [1/2] Copiando script SQL al contenedor Firebird...
docker cp "inserciones_anulaciones_prueba.sql" petrolrios-firebird:/tmp/ins_anul.sql
if errorlevel 1 (
  echo ERROR: el contenedor petrolrios-firebird no esta corriendo. Ejecuta 05_firebird_demo.bat
  pause
  exit /b 1
)

echo [2/2] Ejecutando inserciones contra CONTAC.FDB...
docker exec petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -pas masterkey /firebird/data/CONTAC.FDB -i /tmp/ins_anul.sql > inserciones_anul_resultado.log 2>&1

type inserciones_anul_resultado.log
echo.
echo Inserciones aplicadas: 3 anulaciones (ANUL) con NUMAN 9990001-9990003.
echo El Station Agent las tomara en su proximo ciclo (max 60 s) y el job
echo de Hangfire generara las alertas (siguiente ciclo de 5 min o
echo "Trigger now" en http://localhost:5170/hangfire).
pause
