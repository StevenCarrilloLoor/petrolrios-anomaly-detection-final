@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Agente: publicacion multiplataforma (Windows, Linux y macOS)" -m "- publicar_agente.bat genera 4 paquetes autocontenidos via cross-publish: win-x64, linux-x64, osx-x64, osx-arm64" -m "- Instaladores de arranque automatico por SO: servicio Windows (sc), systemd (Linux) y launchd (macOS)" -m "- LEEME especifico por sistema con chmod/xattr, rutas del CONTAC.FDB y comandos de servicio" -m "- Sin cambios en la logica del agente: AddWindowsService es no-op fuera de Windows; el codigo ya es multiplataforma"

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
