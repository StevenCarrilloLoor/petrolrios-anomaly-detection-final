@echo off
setlocal
cd /d "%~dp0.."
echo Deteniendo procesos...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
timeout /t 2 >nul

echo ===== MIGRACION ===== > seg3.log
echo [1/4] Migracion EF (SeguridadUsuario)...
dotnet ef migrations add SeguridadUsuario --project src\PetrolRios.Infrastructure --startup-project src\PetrolRios.Api --output-dir Persistence\Migrations >> seg3.log 2>&1

echo ===== BUILD ===== >> seg3.log
echo [2/4] Compilando...
dotnet build >> seg3.log 2>&1

echo ===== TESTS ===== >> seg3.log
echo [3/4] Pruebas...
dotnet test --no-build >> seg3.log 2>&1

echo ===== FRONTEND ===== >> seg3.log
echo [4/4] Frontend...
cd frontend
call npm run build >> ..\seg3.log 2>&1
cd ..

copy /Y seg3.log "seg3-result-%RANDOM%.log" >nul
echo Listo.
type seg3.log | findstr /C:"Passed!" /C:"Failed!" /C:"Build succeeded" /C:"error CS" /C:"error" /C:"built in" /C:"Done. To undo"
pause
