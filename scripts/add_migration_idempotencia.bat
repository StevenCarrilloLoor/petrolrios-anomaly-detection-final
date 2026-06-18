@echo off
setlocal
cd /d "%~dp0.."
echo Generando migracion IdempotenciaStaging...
dotnet ef migrations add IdempotenciaStaging -p src\PetrolRios.Infrastructure -s src\PetrolRios.Api -o Persistence\Migrations
echo.
echo Codigo de salida: %errorlevel%
echo Listo > scripts\add_migration_idempotencia.done
pause
