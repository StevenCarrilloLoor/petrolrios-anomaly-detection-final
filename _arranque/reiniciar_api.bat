@echo off
setlocal
rem Cierra el API en ejecucion (por nombre de imagen) y por el puerto 5170, sin PIDs fijos.
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do taskkill /F /PID %%a >nul 2>&1
timeout /t 2 > nul
cd /d "%~dp0"
call 02_levantar_api.bat
