@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Despliegue en red: central accesible por ZeroTier (0.0.0.0 + firewall) + guia CONEXION_RED.md" -m "- INICIAR_CENTRAL_RED.bat: abre el puerto 5170, muestra las IPs y arranca la API en 0.0.0.0:5170" -m "- CONEXION_RED.md: como conectar una estacion remota (Firebird + agente) al central via ZeroTier, sin abrir puertos del router; verificacion y notas de seguridad" -m "- CAMBIOS.md: ronda 7 de correcciones documentada" -m "Recordatorio: la BD se crea sola en el primer arranque (migraciones EF Core)."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
