@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM REINICIA SOLO EL FRONTEND
REM Detiene Vite, limpia su cache y relanza el dev server en http://localhost:5173.
REM ============================================================
setlocal
cd /d "%~dp0..\..\frontend"
echo Deteniendo el dev server (node) y limpiando cache de Vite...
taskkill /F /IM node.exe >nul 2>&1
timeout /t 2 >nul
if exist "node_modules\.vite" rmdir /S /Q "node_modules\.vite"
echo Reiniciando el frontend (Vite) con qrcode...
start "PetrolRios Frontend" cmd /c "npm run dev"
echo Listo. Espere ~10 s y recargue http://localhost:5173
timeout /t 4 >nul
