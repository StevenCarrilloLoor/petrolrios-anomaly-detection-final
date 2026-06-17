@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Agente: panel con login opt-in y re-sincronizacion desde fecha (QoL)" -m "- RequiereLoginPanel (default false): el panel arranca abierto en localhost para configurar la conexion en maquina nueva; el admin activa el login por agente una vez enlazado con el central" -m "- Program.cs: middleware exige sesion solo si RequiereLoginPanel; /api/sesion devuelve requiereLogin; config GET/POST mapea la bandera" -m "- PanelHtml: selector 'Requerir inicio de sesion' en Seguridad; verificarSesion usa !requiereLogin || autenticado" -m "- QoL 'Re-sincronizar desde': CycleRunner.ReiniciarWatermark + endpoint /api/reiniciar-watermark + control en Monitoreo" -m "- Incluye correcciones previas: login 500 con 2FA, editar/crear/eliminar usuarios con confirmar contrasena, UI de login mejorada"

echo.
git log --oneline -3
echo Listo. Cierre esta ventana.
pause
