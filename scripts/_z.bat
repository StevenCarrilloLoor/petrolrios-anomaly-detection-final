@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git rm -q --ignore-unmatch scripts\_ba.txt scripts\_c.bat
git commit scripts\_ba.txt scripts\_c.bat -m "Limpieza: quitar scratch (_ba.txt, _c.bat)"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
