@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Fix: SignalR no recrea la conexion al cargar el usuario (dep [token])" -m "- AuthContext: el efecto depende solo de [token]; evita abortar/recrear la conexion cuando user llega un instante despues. La identidad para 'Usuarios conectados' viene de los claims del JWT" -m "- Verificado en Chrome: Dashboard, Conexiones (usuarios conectados + config estacion Admin guardando), Problemas de estacion (turno sin cerrar en vivo) y Reglas funcionando"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
