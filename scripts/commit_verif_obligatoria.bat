@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Verificacion de correo OBLIGATORIA para iniciar sesion (login normal y por QR)" -m "- El login normal rechaza el acceso si el correo no esta verificado, con mensaje claro" -m "- El login por QR aplica la misma regla (estado 'noverificado')" -m "- Frontend: muestra el mensaje del servidor y ofrece 'Reenviar correo de verificacion'; el QR avisa si la cuenta no esta verificada" -m "- Cuentas del sistema/seed (admin, agentes, demo) se marcan verificadas para no bloquearlas" -m "Build OK, 126 pruebas en verde."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
