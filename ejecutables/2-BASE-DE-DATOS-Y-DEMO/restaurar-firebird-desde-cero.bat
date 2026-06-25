@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM RESTAURA FIREBIRD DESDE CERO
REM Recrea el contenedor Firebird, restaura Contaplus y repara la autenticacion (encadena los dos scripts de esta carpeta).
REM ============================================================
title PetrolRios - Restaurar Firebird
cd /d "%~dp0"
call "%~dp0levantar-firebird-y-restaurar-contaplus.bat"
call "%~dp0reparar-autenticacion-de-firebird.bat" < nul
echo Firebird restaurado y autenticacion reparada.
pause
