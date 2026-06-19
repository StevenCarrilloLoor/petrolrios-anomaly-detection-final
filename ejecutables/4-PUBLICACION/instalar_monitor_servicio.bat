@echo off
setlocal
cd /d "%~dp0"

set NOMBRE=PetrolRios Station Monitor
set EXE=%~dp0PetrolRios.StationMonitor.exe

if not exist "%EXE%" (
  echo ERROR: no se encontro PetrolRios.StationMonitor.exe en esta carpeta.
  echo Copie este .bat dentro de la carpeta del monitor publicado.
  pause
  exit /b 1
)

sc create "%NOMBRE%" binPath= "\"%EXE%\"" start= auto DisplayName= "PetrolRios Station Monitor"
if errorlevel 1 (
  echo ERROR al crear el servicio. Ejecute este archivo como Administrador.
  pause
  exit /b 1
)

sc description "%NOMBRE%" "Monitor local de problemas operativos de la estacion, en modo solo lectura."
sc start "%NOMBRE%"

echo.
echo Monitor instalado. Panel: http://localhost:5190
echo Para detenerlo: sc stop "%NOMBRE%"
echo Para eliminarlo: sc delete "%NOMBRE%"
pause
