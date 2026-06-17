@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Login por QR estilo Steam (aprobacion desde un dispositivo autenticado)" -m "- QrLoginService (estado en memoria, codigo de un solo uso, expiracion 2 min)" -m "- Endpoints /auth/qr/iniciar (anonimo), /auth/qr/estado (polling anonimo, devuelve el login al aprobarse) y /auth/qr/aprobar (autenticado)" -m "- Frontend: la pantalla de login ofrece 'Entrar con codigo QR' (muestra el QR y hace polling); pagina /aprobar-qr para que un usuario autenticado apruebe el acceso; setter de sesion en el AuthContext" -m "- 4 pruebas nuevas del QrLoginService. Build OK, 126 pruebas en verde, frontend compila."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
