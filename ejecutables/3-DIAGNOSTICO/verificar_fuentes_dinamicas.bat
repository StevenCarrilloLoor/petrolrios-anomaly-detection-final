@echo off
title PetrolRios - Verificar fuentes dinamicas
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0verificar_fuentes_dinamicas.ps1"
pause
