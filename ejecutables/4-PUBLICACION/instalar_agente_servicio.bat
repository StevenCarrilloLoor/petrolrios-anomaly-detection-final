@echo off
rem ============================================================
rem  Instala el Agente PetrolRios como SERVICIO de Windows
rem  (arranca automaticamente con el sistema).
rem  Ejecutar COMO ADMINISTRADOR, dentro de la carpeta del agente.
rem ============================================================
setlocal
cd /d "%~dp0"

set NOMBRE=PetrolRios Station Agent
set EXE=%~dp0PetrolRios.StationAgent.exe

if not exist "%EXE%" (
  echo ERROR: no se encontro PetrolRios.StationAgent.exe en esta carpeta.
  echo Copie este .bat DENTRO de la carpeta del agente publicado y reintente.
  pause
  exit /b 1
)

echo Instalando el servicio "%NOMBRE%"...
sc create "%NOMBRE%" binPath= "\"%EXE%\"" start= auto DisplayName= "PetrolRios Station Agent"
if errorlevel 1 (
  echo.
  echo ERROR al crear el servicio. Ejecute este .bat COMO ADMINISTRADOR.
  pause
  exit /b 1
)

sc description "%NOMBRE%" "Agente de estacion PetrolRios: extrae transacciones de Firebird y las envia al servidor central."
echo Arrancando el servicio...
sc start "%NOMBRE%"

echo.
echo ============================================================
echo  Listo. El agente quedo como servicio y arrancara con Windows.
echo  Panel de control:  http://localhost:5180
echo.
echo  Para detenerlo:     sc stop "%NOMBRE%"
echo  Para desinstalarlo: sc delete "%NOMBRE%"   (detener primero)
echo ============================================================
pause
