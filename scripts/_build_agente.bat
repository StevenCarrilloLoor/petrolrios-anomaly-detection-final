@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0.."
echo Liberando el puerto 5170 (para evitar el lock de DLLs)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do taskkill /F /PID %%a >nul 2>&1
timeout /t 2 /nobreak >nul
echo Compilando el agente...
dotnet build src\PetrolRios.StationAgent -c Debug -clp:NoSummary 2>&1 | findstr /i "error CS Build succeeded"
echo --- fin ---
pause
