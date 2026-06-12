@echo off
title PetrolRios - Restaurar Firebird
cd /d "%~dp0..\..\_arranque"
call 05_firebird_demo.bat
call fix_firebird_auth.bat < nul
echo Firebird restaurado y autenticacion reparada.
pause
