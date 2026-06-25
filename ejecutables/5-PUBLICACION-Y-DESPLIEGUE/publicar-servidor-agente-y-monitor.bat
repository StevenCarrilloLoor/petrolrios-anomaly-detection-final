@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM PUBLICA SERVIDOR, AGENTE Y MONITOR
REM Compila el frontend y publica los 3 ejecutables self-contained (Windows) en dist\, opcional con Inno Setup.
REM ============================================================
setlocal
title PetrolRios - Publicacion de ejecutables
cd /d "%~dp0..\.."
set RAIZ=%cd%
set DIST=%RAIZ%\dist

echo ============================================================
echo  Publicacion de PetrolRios (ejecutables self-contained)
echo  No requieren .NET instalado en la maquina destino.
echo ============================================================

echo [1/5] Compilando frontend (React + Vite)...
cd /d "%RAIZ%\frontend"
call npm install --no-audit --no-fund
call npm run build
if errorlevel 1 ( echo ERROR en build del frontend & pause & exit /b 1 )
cd /d "%RAIZ%"

echo [2/5] Publicando SERVIDOR (API + frontend integrado)...
dotnet publish src\PetrolRios.Api -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%DIST%\PetrolRios-Servidor"
if errorlevel 1 ( echo ERROR publicando la API & pause & exit /b 1 )

rem El frontend compilado se integra en wwwroot: la API sirve la SPA completa
xcopy /E /I /Y "%RAIZ%\frontend\dist" "%DIST%\PetrolRios-Servidor\wwwroot" > nul

echo [3/5] Publicando AGENTE de estacion (con panel local)...
dotnet publish src\PetrolRios.StationAgent -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%DIST%\PetrolRios-Agente"
if errorlevel 1 ( echo ERROR publicando el agente & pause & exit /b 1 )

echo [4/5] Publicando MONITOR de estacion (solo lectura)...
dotnet publish src\PetrolRios.StationMonitor -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%DIST%\PetrolRios-Monitor"
if errorlevel 1 ( echo ERROR publicando el monitor & pause & exit /b 1 )
copy /Y "%~dp0instalar-monitor-como-servicio-windows.bat" "%DIST%\PetrolRios-Monitor\" > nul

echo [5/5] Instaladores (Inno Setup, opcional)...
set ISCC="%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if exist %ISCC% (
  %ISCC% "%~dp0instalador_servidor.iss"
  %ISCC% "%~dp0instalador_agente.iss"
  %ISCC% "%~dp0instalador_monitor.iss"
  echo Instaladores generados en dist\instaladores\
) else (
  echo Inno Setup 6 no esta instalado; se omiten los setup.exe.
  echo Los ejecutables portables ya estan listos en dist\
)

echo.
echo ============================================================
echo  LISTO:
echo   dist\PetrolRios-Servidor\PetrolRios.Api.exe
echo     (sirve la aplicacion completa en http://localhost:5170;
echo      requiere PostgreSQL accesible segun appsettings.json)
echo   dist\PetrolRios-Agente\PetrolRios.StationAgent.exe
echo     (panel local en http://localhost:5180; configurar
echo      appsettings.json con el Firebird de la estacion)
echo   dist\PetrolRios-Monitor\PetrolRios.StationMonitor.exe
echo     (monitor de problemas operativos en http://localhost:5190;
echo      solo consulta la estacion asignada en el servidor central)
echo ============================================================
pause
