@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Agente: instalador de servicio de Windows (auto-arranque) incluido en la publicacion" -m "- instalar_agente_servicio.bat: registra el agente como servicio (sc create, start auto) para que arranque con Windows" -m "- publicar_agente.bat copia el instalador de servicio a dist/agente junto al .exe y el LEEME" -m "Flujo recomendado: TU publicas el .exe (tienes el SDK) y pasas la carpeta; el amigo solo ejecuta el .exe o instala el servicio. La carpeta se puede pre-configurar (config/agent-config.json) con tu IP de ZeroTier."

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
