@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Central: configuracion de estaciones por Admin (horario, correo, activa)" -m "- EstacionesController.Update aplica horario/correo/activa solo si el rol es Administrador; EstacionResponse y ConexionEstacionResponse exponen esos campos" -m "- Frontend: formulario de edicion en Conexiones con horario, correo de contacto y activar/desactivar (solo Admin)"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
