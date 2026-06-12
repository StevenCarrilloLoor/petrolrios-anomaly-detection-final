@echo off
setlocal
title PetrolRios - Publicacion de ejecutables
cd /d "%~dp0..\.."
set RAIZ=%cd%
set DIST=%RAIZ%\dist

echo ============================================================
echo  Publicacion de PetrolRios (ejecutables self-contained)
echo  No requieren .NET instalado en la maquina destino.
echo ============================================================

echo [1/4] Compilando frontend (React + Vite)...
cd /d "%RAIZ%\frontend"
call npm install --no-audit --no-fund
call npm run build
if errorlevel 1 ( echo ERROR en build del frontend & pause & exit /b 1 )
cd /d "%RAIZ%"

echo [2/4] Publicando SERVIDOR (API + frontend integrado)...
dotnet publish src\PetrolRios.Api -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%DIST%\PetrolRios-Servidor"
if errorlevel 1 ( echo ERROR publicando la API & pause & exit /b 1 )

rem El frontend compilado se integra en wwwroot: la API sirve la SPA completa
xcopy /E /I /Y "%RAIZ%\frontend\dist" "%DIST%\PetrolRios-Servidor\wwwroot" > nul

echo [3/4] Publicando AGENTE de estacion (con panel local)...
dotnet publish src\PetrolRios.StationAgent -c Release -r win-x64 --self-contained true ^
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true ^
  -o "%DIST%\PetrolRios-Agente"
if errorlevel 1 ( echo ERROR publicando el agente & pause & exit /b 1 )

echo [4/4] Instaladores (Inno Setup, opcional)...
set ISCC="%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if exist %ISCC% (
  %ISCC% "%~dp0instalador_servidor.iss"
  %ISCC% "%~dp0instalador_agente.iss"
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
echo ============================================================
pause
