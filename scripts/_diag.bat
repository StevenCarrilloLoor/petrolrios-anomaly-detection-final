@echo off
cd /d "%~dp0.."
> scripts\_diag.txt echo === git log -2 ===
git log --oneline -2 >> scripts\_diag.txt 2>&1
echo. >> scripts\_diag.txt
echo === git status --short === >> scripts\_diag.txt
git status --short >> scripts\_diag.txt 2>&1
echo. >> scripts\_diag.txt
echo === git fsck (resumen) === >> scripts\_diag.txt
git fsck >> scripts\_diag.txt 2>&1
echo Listo. Cierre esta ventana.
pause
