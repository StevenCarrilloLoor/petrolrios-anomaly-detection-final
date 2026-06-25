@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM INICIA SOLO EL FRONTEND (DEV)
REM Lanza el dev server de Vite (npm run dev) en http://localhost:5173.
REM ============================================================
cd /d "%~dp0..\..\frontend"
npm run dev
pause
