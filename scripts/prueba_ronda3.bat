@echo off
setlocal
cd /d "%~dp0.."
echo [1/5] Deteniendo procesos para poder compilar...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq PetrolRios Frontend*" >nul 2>&1
timeout /t 2 > nul

echo ===== MIGRACION EF ===== > ronda3.log
echo [2/5] Generando migracion (heartbeat + reglas personalizadas)...
dotnet ef migrations add AgregarHeartbeatYReglasPersonalizadas --project src\PetrolRios.Infrastructure --startup-project src\PetrolRios.Api --output-dir Persistence\Migrations >> ronda3.log 2>&1

echo [3/5] Compilando...
echo ===== BUILD ===== >> ronda3.log
dotnet build >> ronda3.log 2>&1

echo [4/5] Tests...
echo ===== TESTS ===== >> ronda3.log
dotnet test --no-build >> ronda3.log 2>&1

echo [5/5] Frontend (tsc + vite build)...
echo ===== FRONTEND ===== >> ronda3.log
cd frontend
call npm run build >> ..\ronda3.log 2>&1
cd ..

copy /Y ronda3.log "ronda3-result-%RANDOM%.log" > nul
echo Listo
timeout /t 4 > nul
