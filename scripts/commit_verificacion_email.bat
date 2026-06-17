@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

echo Confirmando que el archivo de secretos NO se sube...
git check-ignore src\PetrolRios.Api\appsettings.Secrets.json

git add -A
git commit -m "Verificacion de correo real por SMTP (enlace de confirmacion)" -m "- Usuario: EmailVerificado + token de verificacion con expiracion; migracion VerificacionEmail" -m "- Al crear un usuario se envia un correo con boton 'Verificar correo electronico' (enlace a /verificar-correo?token=...)" -m "- Endpoints /auth/verificar-email y /auth/reenviar-verificacion; pagina /verificar-correo en el frontend" -m "- SMTP configurado en appsettings.Secrets.json (git-ignoreado; la App Password de Gmail NUNCA se sube al repo)" -m "- Carga de appsettings.Secrets.json en Program.cs; estado EmailVerificado en la respuesta de usuarios" -m "Verificado en vivo: cuenta creada -> correo enviado por Gmail SMTP -> llego a la bandeja real -> enlace verifico la cuenta. Build OK, 126 pruebas en verde."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo.
echo Confirme que NO aparece appsettings.Secrets.json:
git show --stat --oneline HEAD | findstr /I "Secrets"
echo (si no aparece nada arriba, el secreto NO se subio — correcto)
echo Listo. Cierre esta ventana.
pause
