@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM REINICIA LA CENTRAL ACCESIBLE POR RED
REM Libera el puerto 5170, abre el firewall, asegura PostgreSQL y relanza la API escuchando en toda la red.
REM ============================================================
setlocal enabledelayedexpansion
title PetrolRios - Reiniciar central accesible por RED (ZeroTier)
cd /d "%~dp0..\.."

echo ============================================================
echo   PetrolRios - Central accesible por la RED
echo   (libera el puerto, abre el firewall y escucha en 0.0.0.0)
echo ============================================================
echo.

echo [1/4] Liberando el puerto 5170 (cerrando instancias previas)...
for /f "tokens=5" %%a in ('netstat -ano ^| findstr ":5170" ^| findstr LISTENING') do (
  taskkill /F /PID %%a >nul 2>&1
)
echo        Puerto 5170 libre.
echo.

echo [2/4] Abriendo el puerto 5170 en el Firewall de Windows...
netsh advfirewall firewall delete rule name="PetrolRios Central 5170" >nul 2>&1
netsh advfirewall firewall add rule name="PetrolRios Central 5170" dir=in action=allow protocol=TCP localport=5170
if errorlevel 1 (
  echo        AVISO: no se pudo agregar la regla de firewall.
  echo        Cierra esta ventana y ejecuta este .bat COMO ADMINISTRADOR.
) else (
  echo        Puerto 5170 permitido en el firewall.
)
echo.

echo [3/4] Tus direcciones IP (la de ZeroTier suele ser 10.x.x.x):
ipconfig | findstr /i "IPv4"
echo.
echo        En el agente de la estacion remota, pon:
echo            URL del servidor = http://TU-IP-DE-ZEROTIER:5170
echo.

echo [4/5] Asegurando PostgreSQL (Docker) en :5432 ...
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
    echo        ERROR: Docker no arranco. Abrelo manualmente y reintenta.
    pause
    exit /b 1
  )
)
docker start petrolrios-postgres >nul 2>&1
if errorlevel 1 docker compose up -d >nul 2>&1
docker start petrolrios-firebird >nul 2>&1
echo        PostgreSQL listo.
echo.

echo [5/5] Arrancando el servidor central en 0.0.0.0:5170 ...
echo        (la BD se crea/migra sola la 1a vez)
rem  Perfil "red": escucha en 0.0.0.0:5170 (toda la red) conservando el entorno
rem  Development y toda la configuracion (Hangfire, etc.). Evita el truco de
rem  --no-launch-profile que rompia Hangfire al descartar el entorno.
dotnet run --project src\PetrolRios.Api --launch-profile red
pause
