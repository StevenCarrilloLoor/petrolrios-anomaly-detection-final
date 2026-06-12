@echo off
setlocal
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /PID 20200 >nul 2>&1
timeout /t 2 > nul
cd /d "%~dp0"
call 02_levantar_api.bat
