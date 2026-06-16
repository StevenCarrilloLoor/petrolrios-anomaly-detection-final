@echo off
setlocal
cd /d "%~dp0.."
echo [1/5] Deteniendo procesos para compilar...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq PetrolRios Frontend*" >nul 2>&1
timeout /t 2 > nul

echo ===== MIGRACION EF ===== > ronda4.log
echo [2/5] Migracion (reglas personalizadas avanzadas)...
dotnet ef migrations add ReglasPersonalizadasAvanzadas --project src\PetrolRios.Infrastructure --startup-project src\PetrolRios.Api --output-dir Persistence\Migrations >> ronda4.log 2>&1

echo [3/5] Compilando...
echo ===== BUILD ===== >> ronda4.log
dotnet build >> ronda4.log 2>&1

echo [4/5] Tests...
echo ===== TESTS ===== >> ronda4.log
dotnet test --no-build >> ronda4.log 2>&1

echo [5/5] Frontend...
echo ===== FRONTEND ===== >> ronda4.log
cd frontend
call npm run build >> ..\ronda4.log 2>&1
cd ..

copy /Y ronda4.log "ronda4-result-%RANDOM%.log" > nul
echo Listo
timeout /t 4 > nul
