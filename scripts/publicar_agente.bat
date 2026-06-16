@echo off
setlocal
cd /d "%~dp0.."

set "SALIDA=dist\agente"

echo ============================================================
echo  Publicando el Agente PetrolRios (autocontenido, un solo .exe)
echo ============================================================
echo.

echo [1/3] Limpiando carpeta de salida...
if exist "%SALIDA%" rmdir /S /Q "%SALIDA%"

echo [2/3] Compilando y publicando (win-x64, self-contained, single-file)...
dotnet publish src\PetrolRios.StationAgent\PetrolRios.StationAgent.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:EnableCompressionInSingleFile=true ^
  -o "%SALIDA%"

if errorlevel 1 (
  echo.
  echo ERROR: la publicacion fallo. Revise el detalle arriba.
  pause
  exit /b 1
)

echo [3/3] Agregando instrucciones para la estacion...
copy /Y "scripts\agente-LEEME.txt" "%SALIDA%\LEEME.txt" > nul

echo.
echo ============================================================
echo  LISTO. Carpeta lista para copiar a cada estacion:
echo    %CD%\%SALIDA%
echo.
echo  Para implementar: copie esa carpeta a la computadora de la
echo  estacion y ejecute PetrolRios.StationAgent.exe
echo ============================================================
echo.
dir "%SALIDA%" /B
echo.
pause
