@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Seguridad (fase 3): sin contrasenas quemadas + bloqueo por intentos + cambio obligatorio" -m "- Contrasena inicial del admin desde configuracion/variable de entorno (Seguridad:AdminPasswordInicial), ya no quemada; se obliga a cambiarla en el primer ingreso (DebeCambiarPassword)" -m "- Bloqueo de cuenta por intentos fallidos (anti fuerza bruta), configurable (MaxIntentosLogin, MinutosBloqueo)" -m "- Endpoint POST /api/v1/auth/cambiar-password (autenticado) con validaciones; limpia el flag y desbloquea" -m "- Campos de seguridad en Usuario (lockout, must-change, y secreto TOTP listo para 2FA) + migracion EF SeguridadUsuario" -m "- 4 pruebas de dominio nuevas (UsuarioSeguridadTests). Build OK, 104 pruebas en verde, frontend compila."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo Listo. Cierre esta ventana.
pause
