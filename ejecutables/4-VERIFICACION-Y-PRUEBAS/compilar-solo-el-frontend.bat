@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM COMPILA SOLO EL FRONTEND
REM Ejecuta npm run build del frontend y resume el resultado (errores TS o built in).
REM ============================================================
setlocal
cd /d "%~dp0..\..\frontend"
call npm run build > ..\..\fe.log 2>&1
type ..\..\fe.log | findstr /C:"built in" /C:"error TS" /C:"error during build" /C:"✓"
echo --- fin ---
pause
