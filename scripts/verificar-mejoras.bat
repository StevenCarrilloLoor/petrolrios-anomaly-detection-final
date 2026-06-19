@echo off
setlocal
cd /d "%~dp0.."
set "LOG=%CD%\verificacion.log"

call :verificar > "%LOG%" 2>&1
set "RESULTADO=%ERRORLEVEL%"
type "%LOG%"

if not "%RESULTADO%"=="0" (
  echo.
  echo VERIFICACION FALLIDA. Revisa %LOG%
  exit /b %RESULTADO%
)

echo.
echo VERIFICACION COMPLETA: backend, migraciones, pruebas, lint y frontend OK.
exit /b 0

:verificar
echo [1/6] Restaurando dependencias .NET...
dotnet restore PetrolRios.sln || exit /b 1

echo [2/6] Compilando solucion...
dotnet build PetrolRios.sln -c Release --no-restore || exit /b 1

echo [3/6] Ejecutando pruebas...
dotnet test PetrolRios.sln -c Release --no-build || exit /b 1

echo [4/6] Verificando que el modelo EF no tenga cambios sin migracion...
dotnet ef migrations has-pending-model-changes ^
  --project src\PetrolRios.Infrastructure\PetrolRios.Infrastructure.csproj ^
  --startup-project src\PetrolRios.Api\PetrolRios.Api.csproj ^
  --context PetrolRiosDbContext --configuration Release --no-build || exit /b 1

echo [5/6] Ejecutando lint del frontend...
pushd frontend
call npm run lint || (popd & exit /b 1)

echo [6/6] Compilando frontend...
call npm run build || (popd & exit /b 1)
popd
exit /b 0
