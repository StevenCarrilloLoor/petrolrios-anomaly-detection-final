@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Plataforma flexible (2/3): fuentes de extraccion configurables (multi-tabla)" -m "- AgentSettings.FuentesExtraccion persistido; FirebirdExtractor.ExtractFuenteAsync con validacion anti-inyeccion (tabla/columna contra el catalogo)" -m "- ExtractSinceAsync incluye las fuentes activas (tolerancia a fallos); idempotencia del central descarta reenvios" -m "- Endpoints /api/fuentes (GET/POST con validacion de existencia) + tarjeta de gestion en el panel del agente"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
