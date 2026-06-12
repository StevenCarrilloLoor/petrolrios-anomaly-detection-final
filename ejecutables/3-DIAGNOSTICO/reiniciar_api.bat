@echo off
title PetrolRios - Reiniciar API
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
timeout /t 2 > nul
cd /d "%~dp0..\.."
start "PetrolRios API" cmd /k "dotnet run --project src\PetrolRios.Api"
echo API relanzada en una nueva ventana.
timeout /t 4 > nul
