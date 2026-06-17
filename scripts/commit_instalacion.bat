@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Agregar guia de instalacion y configuracion (INSTALACION.md)" -m "Guia para instalar y configurar el servidor central y el agente de estacion: requisitos, publicacion, configuracion sin secretos quemados, seguridad del panel, notificaciones por correo, actualizacion remota y solucion de problemas. Verificado en vivo: dashboard, conexiones (API v2.2.0, SignalR activo), alertas, usuarios, logs, reportes y el login del panel del agente."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo Listo. Cierre esta ventana.
pause
