@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM INICIA EL MONITOR DE ESTACION
REM Levanta el panel de solo lectura de problemas operativos de la estacion en http://localhost:5190.
REM ============================================================
setlocal
title PetrolRios Monitor de Estacion
cd /d "%~dp0..\..\src\PetrolRios.StationMonitor"

echo ============================================================
echo  PetrolRios - Monitor local de problemas operativos
echo  Panel: http://localhost:5190
echo ============================================================
echo.

start "" http://localhost:5190
dotnet run
pause
