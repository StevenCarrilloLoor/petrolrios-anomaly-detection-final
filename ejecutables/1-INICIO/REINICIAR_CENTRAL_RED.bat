@echo off
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

echo [4/4] Arrancando el servidor central en 0.0.0.0:5170 ...
echo        (PostgreSQL debe estar corriendo; la BD se crea sola la 1a vez)
rem  --no-launch-profile: ignora launchSettings.json (que forzaba localhost)
rem  y respeta ASPNETCORE_URLS para escuchar en toda la red.
set ASPNETCORE_ENVIRONMENT=Development
set ASPNETCORE_URLS=http://0.0.0.0:5170
dotnet run --project src\PetrolRios.Api --no-launch-profile
pause
