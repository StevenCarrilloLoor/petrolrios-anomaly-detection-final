@echo off
setlocal
cd /d "%~dp0.."
echo Deteniendo procesos...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
timeout /t 2 >nul

echo ===== BUILD ===== > tfa.log
echo [1/4] Compilando backend...
dotnet build >> tfa.log 2>&1

echo ===== TESTS ===== >> tfa.log
echo [2/4] Pruebas...
dotnet test --no-build >> tfa.log 2>&1

echo [3/4] Instalando qrcode en frontend...
cd frontend
call npm install qrcode --save >> ..\tfa.log 2>&1
call npm install --save-dev @types/qrcode >> ..\tfa.log 2>&1

echo ===== FRONTEND ===== >> ..\tfa.log
echo [4/4] Compilando frontend...
call npm run build >> ..\tfa.log 2>&1
cd ..

copy /Y tfa.log "tfa-result-%RANDOM%.log" >nul
echo Listo.
type tfa.log | findstr /C:"Build succeeded" /C:"Passed!" /C:"Failed!" /C:"error CS" /C:"error TS" /C:"built in" /C:"added"
pause
