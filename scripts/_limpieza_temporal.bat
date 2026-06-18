@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

rem Borra artefactos de scratch y scripts de un solo uso de esta sesion.
git rm --ignore-unmatch -q ^
  scripts\*.done ^
  scripts\build_errors.txt ^
  scripts\diag_red.txt ^
  scripts\diag_red.bat ^
  scripts\ver_errores_build.bat ^
  scripts\add_migration_idempotencia.bat ^
  scripts\add_migration_ambito.bat ^
  scripts\commit_login_optin.bat ^
  scripts\commit_multiplataforma.bat ^
  scripts\commit_idempotencia.bat ^
  scripts\commit_central_red_fix.bat ^
  scripts\commit_investigacion_fecha.bat ^
  scripts\commit_ambito.bat ^
  scripts\commit_servicio_agente.bat

git add -A
git commit -m "Limpieza: quitar scripts de un solo uso y artefactos de scratch de la sesion"

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
