@echo off
setlocal
rem ============================================================
rem  Levanta Firebird 3.0 en Docker y restaura el backup REAL
rem  de Contaplus (CONTACONSTANZA-20250609.FBK) a CONTAC.FDB.
rem  FB 3.0 restaura backups ODS 11.2 (FB 2.5) sin problema y el
rem  cliente .NET se autentica con Srp de forma nativa.
rem ============================================================

set FBK_DIR=%~dp0firebird_data
set FBK_FILE=CONTACONSTANZA-20250609.FBK

echo [1/4] Eliminando contenedor previo (si existe)...
docker rm -f petrolrios-firebird >nul 2>&1

echo [2/4] Levantando Firebird 3.0 (SYSDBA/masterkey, puerto 3050)...
docker run -d --name petrolrios-firebird ^
  -e ISC_PASSWORD=masterkey ^
  -e FIREBIRD_DATABASE=ignorar.fdb ^
  -p 3051:3050 ^
  -v "%FBK_DIR%:/backup:ro" ^
  jacobalberty/firebird:v3.0
if errorlevel 1 (
  echo ERROR: no se pudo iniciar el contenedor. Esta Docker Desktop corriendo?
  pause
  exit /b 1
)

echo [3/4] Esperando 20 segundos a que Firebird inicialice...
timeout /t 20 /nobreak > nul

echo [4/4] Restaurando %FBK_FILE% (364 MB, puede tardar varios minutos)...
docker exec petrolrios-firebird /usr/local/firebird/bin/gbak -c -i -v -user SYSDBA -pas masterkey "/backup/%FBK_FILE%" /firebird/data/CONTAC.FDB > "%~dp0firebird_restore.log" 2>&1

if errorlevel 1 (
  echo ADVERTENCIA: gbak reporto errores. Revisa firebird_restore.log
) else (
  echo Restauracion completada.
)

docker ps --filter name=petrolrios-firebird
echo.
echo Firebird listo en localhost:3050 — BD: /firebird/data/CONTAC.FDB
echo (la BD restaurada es la CONTACONSTANZA real de Contaplus)
pause
