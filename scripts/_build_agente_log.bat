@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0.."
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do taskkill /F /PID %%a >nul 2>&1
timeout /t 2 /nobreak >nul
dotnet build src\PetrolRios.StationAgent -c Debug > scripts\agente_build.txt 2>&1
echo EXITCODE=%errorlevel% >> scripts\agente_build.txt
pause
