@echo off
setlocal
cd /d "%~dp0..\src\PetrolRios.StationMonitor"
echo Monitor de problemas operativos EST-001 - Central localhost:5170
start "" http://localhost:5190
dotnet run
pause
