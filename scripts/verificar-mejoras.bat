@echo off
setlocal
cd /d "%~dp0.."
title PetrolRios - Verificacion (build + tests + migraciones + frontend)

echo ============================================================
echo   Verificacion PetrolRios
echo   Toma unos 3-5 minutos. Veras el progreso aqui mismo,
echo   paso a paso (no se queda en negro).
echo ============================================================
echo.

echo [0/6] Deteniendo servicios en ejecucion (liberan los binarios;
echo       si no, el build falla con MSB3027)...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationMonitor.exe >nul 2>&1

echo.
echo [1/6] Restaurando dependencias .NET...
dotnet restore PetrolRios.sln || goto :fallo

echo.
echo [2/6] Compilando la solucion (Release)...
dotnet build PetrolRios.sln -c Release --no-restore || goto :fallo

echo.
echo [3/6] Ejecutando las pruebas...
dotnet test PetrolRios.sln -c Release --no-build || goto :fallo

echo.
echo [4/6] Verificando que el modelo EF no tenga cambios sin migracion...
rem Asegurar la herramienta dotnet-ef (si no esta instalada, este paso fallaba en frio).
dotnet ef --version >nul 2>&1 || dotnet tool install --global dotnet-ef >nul 2>&1
dotnet ef migrations has-pending-model-changes ^
  --project src\PetrolRios.Infrastructure\PetrolRios.Infrastructure.csproj ^
  --startup-project src\PetrolRios.Api\PetrolRios.Api.csproj ^
  --context PetrolRiosDbContext --configuration Release --no-build || goto :fallo

echo.
echo [5/6] Ejecutando lint del frontend...
pushd frontend
call npm run lint || (popd & goto :fallo)

echo.
echo [6/6] Compilando el frontend...
call npm run build || (popd & goto :fallo)
popd

echo.
echo ============================================================
echo   VERIFICACION COMPLETA: backend, migraciones, pruebas,
echo   lint y frontend OK.
echo ============================================================
pause
exit /b 0

:fallo
echo.
echo ============================================================
echo   VERIFICACION FALLIDA. Revisa el detalle que aparece arriba
echo   en esta misma ventana.
echo ============================================================
pause
exit /b 1
