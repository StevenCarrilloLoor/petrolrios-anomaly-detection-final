@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Central en red: perfil 'red' (0.0.0.0) y arranque autosuficiente (Postgres)" -m "- launchSettings: perfil 'red' escucha en 0.0.0.0:5170 conservando entorno Development (Hangfire OK)" -m "- REINICIAR_CENTRAL_RED: asegura Docker+PostgreSQL antes de la API; usa --launch-profile red" -m "- Corrige el fallo 'Failed to connect 127.0.0.1:5432' al reiniciar solo el central"

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
