@echo off
setlocal enabledelayedexpansion
title PetrolRios - Paso: build + migracion + tests + commit
cd /d "%~dp0.."

rem ====== EDITAR ESTAS 2 LINEAS POR CADA PASO ======
set "MIGRACION=EsquemaTabla"
set "MENSAJE=El agente reporta su esquema al central + navegador de tablas con busqueda y auto-documentacion en Reglas"
rem  (Si el paso NO necesita migracion, deja MIGRACION vacio: set "MIGRACION=")
rem =================================================

echo [1/6] Liberando el puerto 5170...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do taskkill /F /PID %%a >nul 2>&1
echo.

echo [2/6] Compilando la solucion (Release)...
dotnet build PetrolRios.sln -c Release --nologo
if errorlevel 1 ( echo ERROR de compilacion. & pause & exit /b 1 )
echo.

echo [3/6] Migracion EF...
if not "%MIGRACION%"=="" (
  dir /b src\PetrolRios.Infrastructure\Persistence\Migrations\*_%MIGRACION%.cs >nul 2>&1
  if errorlevel 1 (
    dotnet ef migrations add %MIGRACION% --project src\PetrolRios.Infrastructure --startup-project src\PetrolRios.Api --output-dir Persistence\Migrations
    if errorlevel 1 ( echo ERROR creando la migracion. & pause & exit /b 1 )
    echo        Migracion %MIGRACION% creada.
  ) else ( echo        La migracion %MIGRACION% ya existe; se omite. )
) else ( echo        Este paso no requiere migracion. )
echo.

echo [4/6] Pruebas unitarias (Domain + Detectors)...
dotnet test tests\PetrolRios.Domain.Tests --nologo -c Release
if errorlevel 1 ( echo FALLARON pruebas de Domain. & pause & exit /b 1 )
dotnet test tests\PetrolRios.Detectors.Tests --nologo -c Release
if errorlevel 1 ( echo FALLARON pruebas de Detectors. & pause & exit /b 1 )
echo.

echo [5/6] Compilando el frontend...
pushd frontend
call npm run build
set FE_ERR=!errorlevel!
popd
if not "!FE_ERR!"=="0" ( echo ERROR compilando el frontend. & pause & exit /b 1 )
echo.

echo [6/6] Todo verde -> commit...
if exist ".git\index.lock" del /F /Q ".git\index.lock"
rem  Sacar del control de versiones los datos de runtime del agente (ya en .gitignore)
git rm -r --cached --ignore-unmatch src/PetrolRios.StationAgent/pending >nul 2>&1
git add -A
git commit -m "%MENSAJE%"
echo.
git log --oneline -1
echo.
echo ============================================================
echo   Paso completo. Reinicia con ejecutables\1-INICIO\INICIAR_TODO.bat
echo   (la migracion se aplica sola al arrancar la API).
echo ============================================================
pause
