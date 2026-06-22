@echo off
REM Instalacion guiada del sistema central PetrolRios.
REM Solo lanza el script de PowerShell (que hace todo el trabajo).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0INSTALAR.ps1"
