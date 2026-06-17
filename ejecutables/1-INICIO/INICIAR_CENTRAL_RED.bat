@echo off
title PetrolRios - Servidor central accesible por RED (ZeroTier)
cd /d "%~dp0..\.."

echo ============================================================
echo   PetrolRios - Central accesible por la RED (para conectar
echo   estaciones remotas via ZeroTier/Tailscale)
echo ============================================================
echo.

rem ── Regla de firewall para el puerto 5170 (requiere admin) ──
echo [1/3] Abriendo el puerto 5170 en el Firewall de Windows...
netsh advfirewall firewall add rule name="PetrolRios Central 5170" dir=in action=allow protocol=TCP localport=5170 >nul 2>&1
if errorlevel 1 (
  echo        AVISO: no se pudo agregar la regla. Ejecuta este .bat
  echo        como ADMINISTRADOR para abrir el puerto 5170.
) else (
  echo        Puerto 5170 permitido.
)
echo.

rem ── Mostrar las IPs (para identificar la de ZeroTier) ──
echo [2/3] Tus direcciones IP (usa la de ZeroTier, suele ser 10.x.x.x):
ipconfig | findstr /i "IPv4"
echo.
echo        En el agente de la estacion remota, pon:
echo            URL del servidor = http://TU-IP-DE-ZEROTIER:5170
echo.

rem ── Arrancar la API escuchando en toda la red ──
echo [3/3] Arrancando el servidor central en 0.0.0.0:5170 ...
echo        (PostgreSQL debe estar corriendo; la BD se crea sola la 1a vez)
set ASPNETCORE_URLS=http://0.0.0.0:5170
dotnet run --project src\PetrolRios.Api
pause
