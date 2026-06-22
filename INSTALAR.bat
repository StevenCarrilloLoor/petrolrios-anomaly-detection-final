@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion
cd /d "%~dp0"
title PetrolRios - Instalacion del sistema central

echo ===================================================
echo    PetrolRios - Instalacion del sistema central
echo ===================================================
echo.

REM --- 1) Docker instalado y corriendo ---
where docker >nul 2>&1
if errorlevel 1 (
  echo  [X] Docker no esta instalado.
  echo      Instala "Docker Desktop" desde https://www.docker.com/products/docker-desktop
  echo      y vuelve a ejecutar este instalador.
  echo.
  pause
  exit /b 1
)
docker info >nul 2>&1
if errorlevel 1 (
  echo  [X] Docker esta instalado pero no esta corriendo.
  echo      Abre "Docker Desktop", espera a que diga "Running" y ejecuta esto de nuevo.
  echo.
  pause
  exit /b 1
)
echo  [OK] Docker detectado y corriendo.
echo.

REM --- 2) Configuracion (.env) ---
if exist ".env" (
  echo  Ya existe un archivo .env: se conservara tal cual.
  echo  Para reconfigurar desde cero, borra .env y vuelve a ejecutar.
  echo.
) else (
  echo  Voy a generar la configuracion. Las contrasenas internas se crean solas.
  echo.
  echo  Correo para los avisos del sistema (recuperacion / verificacion de cuenta).
  echo  Si todavia no lo tienes, deja ambos vacios y lo pones luego desde Ajustes.
  set /p EMAIL_USER="   Correo (Gmail):        "
  set /p EMAIL_PWD="   App Password del correo: "

  REM Secretos aleatorios alfanumericos (sin caracteres especiales)
  for /f "delims=" %%a in ('powershell -NoProfile -Command "-join ((48..57)+(65..90)+(97..122) ^| Get-Random -Count 40 ^| ForEach-Object {[char]$_})"') do set "PG_PWD=%%a"
  for /f "delims=" %%a in ('powershell -NoProfile -Command "-join ((48..57)+(65..90)+(97..122) ^| Get-Random -Count 48 ^| ForEach-Object {[char]$_})"') do set "JWT=%%a"

  REM IP de la red local
  for /f "delims=" %%i in ('powershell -NoProfile -Command "(Get-NetIPAddress -AddressFamily IPv4 ^| Where-Object {$_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*'} ^| Select-Object -First 1 -ExpandProperty IPAddress)"') do set "LANIP=%%i"
  if "!LANIP!"=="" set "LANIP=localhost"

  set "EMAIL_HAB=true"
  if "!EMAIL_USER!"=="" set "EMAIL_HAB=false"

  (
    echo POSTGRES_DB=petrolrios
    echo POSTGRES_USER=petrolrios
    echo POSTGRES_PASSWORD=!PG_PWD!
    echo CENTRAL_PORT=8080
    echo JWT_SECRET=!JWT!
    echo FRONTEND_URL=http://!LANIP!:8080
    echo EMAIL_HABILITADO=!EMAIL_HAB!
    echo EMAIL_HOST=smtp.gmail.com
    echo EMAIL_PUERTO=587
    echo EMAIL_SSL=true
    echo EMAIL_USUARIO=!EMAIL_USER!
    echo EMAIL_PASSWORD=!EMAIL_PWD!
    echo EMAIL_REMITENTE=!EMAIL_USER!
  ) > .env

  echo.
  echo  [OK] Configuracion guardada en .env  ^(IP detectada: !LANIP!^)
  echo.
)

REM --- 3) Construir y levantar ---
echo  Construyendo y levantando el sistema. La PRIMERA vez tarda unos minutos...
echo.
docker compose -f docker-compose.prod.yml up -d --build
if errorlevel 1 (
  echo.
  echo  [X] Hubo un error al levantar. Revisa el mensaje de arriba.
  echo.
  pause
  exit /b 1
)

REM --- 4) Resultado ---
for /f "delims=" %%i in ('powershell -NoProfile -Command "(Get-NetIPAddress -AddressFamily IPv4 ^| Where-Object {$_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*'} ^| Select-Object -First 1 -ExpandProperty IPAddress)"') do set "LANIP=%%i"
if "!LANIP!"=="" set "LANIP=localhost"

echo.
echo ===================================================
echo    LISTO. El sistema central esta corriendo.
echo ===================================================
echo    En esta maquina:      http://localhost:8080
echo    Desde otra maquina:   http://!LANIP!:8080
echo.
echo    Usuario:  admin@petrolrios.com
echo    Clave:    Admin123!   (te pedira cambiarla al entrar)
echo.
echo    TIP: en Docker Desktop activa "Start Docker Desktop when you log in"
echo         para que el sistema vuelva solo despues de un corte de luz.
echo ===================================================
echo.
pause
