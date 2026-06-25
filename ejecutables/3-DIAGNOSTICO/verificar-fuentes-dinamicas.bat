@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM VERIFICA LAS FUENTES DINAMICAS
REM Lanza el chequeo (PowerShell) del agente y el central para confirmar que las tablas configurables se sincronizan.
REM ============================================================
title PetrolRios - Verificar fuentes dinamicas
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0verificar-fuentes-dinamicas.ps1"
pause
