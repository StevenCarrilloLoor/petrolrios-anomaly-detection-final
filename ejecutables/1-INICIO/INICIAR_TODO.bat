@echo off
setlocal enabledelayedexpansion
title PetrolRios - Arranque del sistema
cd /d "%~dp0..\.."
set RAIZ=%cd%

echo ============================================================
echo   PetrolRios - Sistema de Deteccion de Anomalias
echo   Arranque automatico de todos los servicios
echo ============================================================
echo.

rem ── [1/6] Docker ─────────────────────────────────────────────
echo [1/6] Verificando Docker...
docker info >nul 2>&1
if errorlevel 1 (
  echo        Docker no responde. Iniciando Docker Desktop...
  start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
  set /a intentos=0
  :esperar_docker
  timeout /t 5 /nobreak >nul
  docker info >nul 2>&1
  if errorlevel 1 (
    set /a intentos+=1
    if !intentos! lss 24 (
      echo        Esperando a Docker... [!intentos!/24]
      goto esperar_docker
    )
    echo  ERROR: Docker Desktop no arranco en 2 minutos. Abrelo manualmente y reintenta.
    pause
    exit /b 1
  )
)
echo        Docker OK.

rem ── [2/6] PostgreSQL ─────────────────────────────────────────
echo [2/6] PostgreSQL...
docker start petrolrios-postgres >nul 2>&1
if errorlevel 1 docker compose up -d >nul 2>&1
echo        PostgreSQL arrancando en :5432

rem ── [3/6] Firebird (opcional, demo con BD real) ──────────────
echo [3/6] Firebird (BD Contaplus real)...
docker start petrolrios-firebird >nul 2>&1
if errorlevel 1 (
  echo        Contenedor no existe. Para crearlo: ejecutables\3-DIAGNOSTICO\restaurar_firebird.bat
) else (
  echo        Firebird arrancando en :3051
)

rem ── [4/6] API central ────────────────────────────────────────
echo [4/6] API central (compilando y arrancando)...
start "PetrolRios API" cmd /c "cd /d "%RAIZ%" && dotnet run --project src\PetrolRios.Api && pause"

echo        Esperando a que la API responda en :5170...
set /a intentos=0
:esperar_api
timeout /t 4 /nobreak >nul
powershell -NoProfile -Command "try{(Invoke-WebRequest -UseBasicParsing -TimeoutSec 3 http://localhost:5170/swagger/index.html).StatusCode}catch{exit 1}" >nul 2>&1
if errorlevel 1 (
  set /a intentos+=1
  if !intentos! lss 30 goto esperar_api
  echo        ADVERTENCIA: la API no respondio aun; revisa su ventana.
) else (
  echo        API OK.
)

rem ── [5/6] Frontend ───────────────────────────────────────────
echo [5/6] Frontend (Vite)...
start "PetrolRios Frontend" cmd /c "cd /d "%RAIZ%\frontend" && npm run dev && pause"

rem ── [6/6] Station Agent ──────────────────────────────────────
echo [6/6] Station Agent EST-001 (con panel local)...
start "PetrolRios Agente" cmd /c "cd /d "%RAIZ%\src\PetrolRios.StationAgent" && dotnet run && pause"

timeout /t 8 /nobreak >nul
echo.
echo ============================================================
echo   TODO LISTO
echo   - Aplicacion:        http://localhost:5173
echo   - Panel del agente:  http://localhost:5180
echo   - Swagger:           http://localhost:5170/swagger
echo   - Hangfire:          http://localhost:5170/hangfire
echo   Usuario: admin@petrolrios.com / Admin123!
echo ============================================================
start http://localhost:5173
timeout /t 4 /nobreak >nul
start http://localhost:5180
echo.
echo (esta ventana puede cerrarse; los servicios siguen en sus ventanas)
pause
