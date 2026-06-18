@echo off
setlocal enabledelayedexpansion
cd /d "%~dp0.."
echo Liberando el puerto 5170 (deteniendo la API que bloquea las DLLs)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do taskkill /F /PID %%a >nul 2>&1
timeout /t 3 /nobreak >nul
echo Generando migracion UsuarioEstacionYContacto...
dotnet ef migrations add UsuarioEstacionYContacto -p src\PetrolRios.Infrastructure -s src\PetrolRios.Api -o Persistence\Migrations
echo Codigo de salida: %errorlevel%
pause
