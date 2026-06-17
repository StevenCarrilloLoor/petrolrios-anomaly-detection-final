@echo off
setlocal
cd /d "%~dp0.."
echo Deteniendo procesos para compilar...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
timeout /t 2 >nul

echo ===== BUILD ===== > ronda5.log
echo [1/3] Compilando solucion...
dotnet build >> ronda5.log 2>&1

echo ===== TESTS ===== >> ronda5.log
echo [2/3] Ejecutando pruebas...
dotnet test --no-build >> ronda5.log 2>&1

echo ===== FRONTEND ===== >> ronda5.log
echo [3/3] Compilando frontend...
cd frontend
call npm run build >> ..\ronda5.log 2>&1
cd ..

echo. >> ronda5.log
echo ===== RESUMEN ===== >> ronda5.log
findstr /C:"Build succeeded" /C:"error" /C:"Passed!" /C:"Failed!" /C:"Errors" /C:"built in" ronda5.log >> ronda5.log
copy /Y ronda5.log "ronda5-result-%RANDOM%.log" >nul
echo Listo. Revise ronda5.log
type ronda5.log | findstr /C:"Passed!" /C:"Failed!" /C:"Build succeeded" /C:"error CS" /C:"built in"
pause
