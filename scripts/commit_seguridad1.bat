@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Seguridad (fase 1): endurecer accesos del servidor + notificador de correo" -m "- Dashboard de Hangfire protegido (HangfireLocalAuthorizationFilter): solo local o Administrador autenticado; cierra el panel de jobs abierto" -m "- JWT: fail-fast en produccion si la clave es debil (<32) o la de desarrollo" -m "- HTTPS/HSTS en produccion (configurable con Seguridad:ForzarHttps)" -m "- Notificador SMTP de alertas criticas (IEmailNotificacionService) configurable y apagado por defecto; envia a supervisores/administradores; sin credenciales quemadas; nunca tumba el ciclo" -m "- Seccion Notificaciones:Email y Seguridad en appsettings" -m "Build OK, 100 pruebas en verde."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo Listo. Cierre esta ventana.
pause
