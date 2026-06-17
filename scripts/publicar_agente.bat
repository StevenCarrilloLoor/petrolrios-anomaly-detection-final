@echo off
setlocal
cd /d "%~dp0.."

rem ============================================================
rem  Publica el Agente PetrolRios para Windows, Linux y macOS.
rem  Cada destino queda autocontenido (un solo ejecutable, sin
rem  necesidad de instalar .NET en la maquina de la estacion).
rem  Se ejecuta en TU maquina Windows; dotnet compila para los
rem  demas sistemas sin problema (cross-publish).
rem ============================================================

set "PROY=src\PetrolRios.StationAgent\PetrolRios.StationAgent.csproj"

echo ============================================================
echo  Publicando el Agente PetrolRios (multiplataforma)
echo ============================================================
echo.

echo Limpiando carpeta de salida...
if exist "dist" rmdir /S /Q "dist"

echo.
echo [1/4] Windows (win-x64)...
dotnet publish "%PROY%" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o "dist\agente-windows"
if errorlevel 1 goto fin_error
copy /Y "scripts\agente-LEEME-windows.txt" "dist\agente-windows\LEEME.txt" > nul
copy /Y "ejecutables\4-PUBLICACION\instalar_agente_servicio.bat" "dist\agente-windows\instalar_agente_servicio.bat" > nul

echo.
echo [2/4] Linux (linux-x64)...
dotnet publish "%PROY%" -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o "dist\agente-linux"
if errorlevel 1 goto fin_error
copy /Y "scripts\agente-LEEME-linux.txt" "dist\agente-linux\LEEME.txt" > nul
copy /Y "ejecutables\4-PUBLICACION\instalar_agente_servicio.sh" "dist\agente-linux\instalar_agente_servicio.sh" > nul

echo.
echo [3/4] macOS Intel (osx-x64)...
dotnet publish "%PROY%" -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o "dist\agente-macos-intel"
if errorlevel 1 goto fin_error
copy /Y "scripts\agente-LEEME-macos.txt" "dist\agente-macos-intel\LEEME.txt" > nul
copy /Y "ejecutables\4-PUBLICACION\instalar_agente_servicio_macos.sh" "dist\agente-macos-intel\instalar_agente_servicio_macos.sh" > nul

echo.
echo [4/4] macOS Apple Silicon (osx-arm64)...
dotnet publish "%PROY%" -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o "dist\agente-macos-arm"
if errorlevel 1 goto fin_error
copy /Y "scripts\agente-LEEME-macos.txt" "dist\agente-macos-arm\LEEME.txt" > nul
copy /Y "ejecutables\4-PUBLICACION\instalar_agente_servicio_macos.sh" "dist\agente-macos-arm\instalar_agente_servicio_macos.sh" > nul

echo.
echo ============================================================
echo  LISTO. Carpetas listas para copiar a cada estacion:
echo    dist\agente-windows       (Windows 64-bit)
echo    dist\agente-linux         (Linux x64, p.ej. Ubuntu)
echo    dist\agente-macos-intel   (macOS Intel)
echo    dist\agente-macos-arm     (macOS Apple Silicon M1/M2/M3)
echo ============================================================
echo.
pause
exit /b 0

:fin_error
echo.
echo ERROR: la publicacion fallo. Revise el detalle arriba.
pause
exit /b 1
