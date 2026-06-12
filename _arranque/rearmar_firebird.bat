@echo off
setlocal
cd /d "%~dp0"
call 05_firebird_demo.bat < nul
call fix_firebird_auth.bat < nul
call 96_insertar_anomalias_firebird.bat < nul
echo TODO LISTO
timeout /t 5 > nul
