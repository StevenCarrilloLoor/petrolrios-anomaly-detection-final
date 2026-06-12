@echo off
setlocal
cd /d "%~dp0.."

echo ===== VERIFICACION DE MEJORAS PETROLRIOS ===== > verificacion.log
echo Inicio: %date% %time% >> verificacion.log

echo [1/5] Restaurando paquetes NuGet (puede tardar)...
echo. >> verificacion.log
echo ===== [1/5] DOTNET RESTORE ===== >> verificacion.log
dotnet restore >> verificacion.log 2>&1

echo [2/5] Generando migracion EF Core (comentarios + fecha resolucion)...
echo. >> verificacion.log
echo ===== [2/5] MIGRACION EF ===== >> verificacion.log
dotnet tool update --global dotnet-ef >> verificacion.log 2>&1
dotnet ef migrations add AgregarComentariosYFechaResolucion --project src\PetrolRios.Infrastructure --startup-project src\PetrolRios.Api --output-dir Persistence\Migrations >> verificacion.log 2>&1

echo [3/5] Compilando solucion...
echo. >> verificacion.log
echo ===== [3/5] DOTNET BUILD ===== >> verificacion.log
dotnet build >> verificacion.log 2>&1

echo [4/5] Ejecutando pruebas unitarias...
echo. >> verificacion.log
echo ===== [4/5] DOTNET TEST ===== >> verificacion.log
dotnet test --no-build >> verificacion.log 2>&1

echo [5/5] Compilando frontend (npm install + build)...
echo. >> verificacion.log
echo ===== [5/5] FRONTEND BUILD ===== >> verificacion.log
cd frontend
call npm install --no-audit --no-fund >> ..\verificacion.log 2>&1
call npm run build >> ..\verificacion.log 2>&1
cd ..

echo. >> verificacion.log
echo Fin: %date% %time% >> verificacion.log

rem Copia con nombre unico para lectura externa
copy /Y verificacion.log "verif-result-%RANDOM%.log" > nul

echo.
echo ============================================
echo  Verificacion completa. Resultados en:
echo  verificacion.log
echo  (esta ventana se cierra en 10 segundos)
echo ============================================
timeout /t 10 > nul
