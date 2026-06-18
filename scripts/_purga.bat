@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

rem Borra scripts de un solo uso (commits puntuales), scratch de sesion, el LEEME
rem superseded por los per-SO, y verify scripts redundantes con verificar_2fa.bat.
git rm --ignore-unmatch -q ^
  scripts\_add_migration.bat scripts\_build_agente.bat scripts\_build_agente_log.bat ^
  scripts\_commit.bat scripts\_limpieza_temporal.bat scripts\agente_build.txt ^
  scripts\agente-LEEME.txt ^
  scripts\commit3.bat scripts\commit_2fa.bat scripts\commit_agente_paquete.bat ^
  scripts\commit_autoupdate.bat scripts\commit_bugfix_firebird.bat scripts\commit_deploy_red.bat ^
  scripts\commit_fixes_ui.bat scripts\commit_instalacion.bat scripts\commit_login_extras.bat ^
  scripts\commit_qrlogin.bat scripts\commit_ronda4.bat scripts\commit_seguridad1.bat ^
  scripts\commit_seguridad2.bat scripts\commit_seguridad3.bat scripts\commit_verif_obligatoria.bat ^
  scripts\commit_verificacion_email.bat ^
  scripts\prueba_ronda4.bat scripts\verificar-mejoras.bat scripts\verificar_email.bat ^
  scripts\verificar_ronda5.bat scripts\verificar_seguridad3.bat

git add -A
git commit -m "Limpieza: purgar scripts de un solo uso, scratch y verify redundantes" -m "Conservados (reutilizables): publicar_agente, build_frontend, reiniciar_frontend, verificar_2fa, coverage.ps1/sh, agente-LEEME-windows/linux/macos. ejecutables/ intacto."

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
del /F /Q "%~f0"
