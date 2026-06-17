@echo off
setlocal
cd /d "%~dp0.."
echo Deteniendo procesos...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
timeout /t 2 >nul

echo ===== MIGRACION ===== > email.log
echo [1/4] Migracion EF (VerificacionEmail)...
dotnet ef migrations add VerificacionEmail --project src\PetrolRios.Infrastructure --startup-project src\PetrolRios.Api --output-dir Persistence\Migrations >> email.log 2>&1

echo ===== BUILD ===== >> email.log
echo [2/4] Compilando...
dotnet build >> email.log 2>&1

echo ===== TESTS ===== >> email.log
echo [3/4] Pruebas...
dotnet test --no-build >> email.log 2>&1

echo [4/4] Frontend...
cd frontend
call npm run build >> ..\email.log 2>&1
cd ..

copy /Y email.log "email-result-%RANDOM%.log" >nul
type email.log | findstr /C:"Build succeeded" /C:"Passed!" /C:"Failed!" /C:"error CS" /C:"error TS" /C:"built in" /C:"Done. To undo"
pause
