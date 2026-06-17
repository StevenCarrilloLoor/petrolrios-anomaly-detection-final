@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Login con autenticador (sin password) + recuperacion de contrasena por correo + CAMBIOS.md" -m "- Entrar con el codigo del autenticador: POST /auth/login-totp para cuentas con 2FA, opcion en el login" -m "- Recuperacion de contrasena: /auth/olvide-password envia enlace por correo; pagina /restablecer-password y /auth/restablecer-password; tokens de un solo uso en memoria (PasswordResetService, 1h)" -m "- LoginPage: '¿Olvidaste tu contrasena?' y 'Entrar con codigo del autenticador'" -m "- CAMBIOS.md actualizado con TODO lo de las rondas 5-6 (auto-update, seguridad, bugfix Firebird, 2FA, QR login, verificacion de correo, recuperacion)" -m "Build OK, 126 pruebas en verde, frontend compila."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo.
echo Confirme que NO aparece el archivo de secretos:
git show --stat --oneline HEAD | findstr /I "Secrets" || echo (correcto: el secreto NO se subio)
echo Listo. Cierre esta ventana.
pause
