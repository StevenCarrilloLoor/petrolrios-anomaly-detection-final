@echo off
setlocal enabledelayedexpansion
title PetrolRios - Verificacion ronda "Fuentes centrales + memoria de envio"
cd /d "%~dp0.."

echo ============================================================
echo   Verificacion de la ronda:
echo   - Memoria de envio del agente
echo   - Reglas Operativa/Auditoria
echo   - Buscador de tablas escribible
echo   - Registro CENTRAL de fuentes (tablas extra) + migracion
echo ============================================================
echo.

echo [1/5] Liberando el puerto 5170 (para no bloquear el build)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do (
  taskkill /F /PID %%a >nul 2>&1
)
echo        Puerto libre.
echo.

echo [2/5] Compilando la solucion (Release)...
dotnet build PetrolRios.sln -c Release --nologo
if errorlevel 1 (
  echo.
  echo        ERROR de compilacion. Revisa los mensajes de arriba.
  pause
  exit /b 1
)
echo        Build OK.
echo.

echo [3/5] Creando la migracion EF "FuentesDatos" (si no existe)...
dir /b src\PetrolRios.Infrastructure\Persistence\Migrations\*_FuentesDatos.cs >nul 2>&1
if errorlevel 1 (
  dotnet ef migrations add FuentesDatos ^
    --project src\PetrolRios.Infrastructure ^
    --startup-project src\PetrolRios.Api ^
    --output-dir Persistence\Migrations
  if errorlevel 1 (
    echo        ERROR creando la migracion. Si falta la herramienta:
    echo            dotnet tool install --global dotnet-ef
    pause
    exit /b 1
  )
  echo        Migracion creada. Se aplicara sola al arrancar la API.
) else (
  echo        La migracion FuentesDatos ya existe; se omite.
)
echo.

echo [4/5] Ejecutando pruebas unitarias (Domain + Detectors)...
dotnet test tests\PetrolRios.Domain.Tests --nologo -c Release
if errorlevel 1 ( echo        FALLARON pruebas de Domain. & pause & exit /b 1 )
dotnet test tests\PetrolRios.Detectors.Tests --nologo -c Release
if errorlevel 1 ( echo        FALLARON pruebas de Detectors. & pause & exit /b 1 )
echo        Pruebas backend OK.
echo.

echo [5/6] Compilando el frontend (type-check + build)...
pushd frontend
call npm run build
set FE_ERR=%errorlevel%
popd
if not "%FE_ERR%"=="0" (
  echo        ERROR compilando el frontend. Revisa los mensajes de arriba.
  pause
  exit /b 1
)
echo        Frontend OK.
echo.

echo [6/6] Todo verde -> commit...
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Centralizar fuentes de datos + memoria de envio del agente + reglas por ambito" -m "- Agente: SentMemory (huella de contenido persistida) evita reenviar el mismo registro cada ciclo (corta el bucle del turno abierto EST_TURN='0'); el central ya estaba blindado por idempotencia, esto ahorra red/ruido." -m "- Reglas: badge Operativa/Auditoria derivado del parametro en ReglaService; operativas visibles y editables." -m "- Panel agente: buscador de tablas escribible (input+datalist) en vez del desplegable de ~200." -m "- Registro CENTRAL de fuentes: entidad FuenteDatos + migracion, /api/v1/fuentes-datos (CRUD admin + /activas para agentes), seccion 'Fuentes de datos' en Reglas; el agente descarga el catalogo central cada ciclo y lo combina con las locales (central tiene prioridad). El ingeniero registra una tabla una sola vez. Test FuenteDatosTests."
echo.
git log --oneline -1
echo.
echo ============================================================
echo   Listo. Para arrancar y probar en vivo:
echo     - ejecutables\1-INICIO\INICIAR_TODO.bat        (todo de una)
echo     - ejecutables\1-INICIO\REINICIAR_CENTRAL_RED.bat (acceso por RED/ZeroTier)
echo   La migracion FuentesDatos se aplica sola al arrancar la API.
echo ============================================================
pause
