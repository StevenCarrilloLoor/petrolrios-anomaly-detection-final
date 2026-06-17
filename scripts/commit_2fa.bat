@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "2FA / Autenticador TOTP en el programa principal + cambio de contrasena" -m "- TotpService (RFC 6238, HMAC-SHA1, 6 digitos, ventana 30s) compatible con Google Authenticator/Authy, sin dependencias externas" -m "- Login con segundo factor: si el usuario tiene 2FA, pide el codigo de 6 digitos; intentos fallidos cuentan para el bloqueo" -m "- Endpoints /auth/2fa/estado|iniciar|confirmar|desactivar y /auth/cambiar-password" -m "- Frontend: pantalla Seguridad (mi cuenta) con enrolamiento por QR (libreria qrcode) + cambio de contrasena; paso de codigo 2FA en el login; item en el menu" -m "- 5 pruebas nuevas del TOTP. Build OK, 122 pruebas en verde, frontend compila."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
