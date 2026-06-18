@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Ingesta idempotente (anti reenvio duplicado) + login QR opcional" -m "- TransaccionStaging.HashContenido (SHA-256 de estacion|tipo|datos) + indice unico (EstacionId, HashContenido)" -m "- IngestaService descarta reenvios y duplicados de lote; un registro fechado al futuro se alerta una sola vez" -m "- Migracion IdempotenciaStaging (rellena filas previas con su Id para no chocar con el indice unico)" -m "- Login QR oculto tras VITE_QR_HABILITADO; login movil cubierto por TOTP (offline)" -m "- Correo de verificacion: documentado App__FrontendUrl para no depender de localhost"

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
