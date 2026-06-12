@echo off
title PetrolRios - Reparar autenticacion Firebird
cd /d "%~dp0..\..\_arranque"
call fix_firebird_auth.bat
echo Revise el log fbauth-*.log generado en _arranque.
pause
