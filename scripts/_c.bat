@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Central: usuarios conectados en Monitoreo (SignalR)" -m "- AlertsHub registra usuario/rol por conexion (claims JWT + respaldo por query); expone UsuariosConectados" -m "- Endpoint GET /monitoreo/usuarios-conectados (Supervisor/Admin) + DTO" -m "- signalr.ts pasa usuarioId/nombre/rol; AuthContext lo envia; seccion 'Usuarios conectados' en ConexionesPage; CardHeader.title acepta ReactNode"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
