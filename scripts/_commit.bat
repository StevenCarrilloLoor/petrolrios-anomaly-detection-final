@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Subsistema de ambito: usuario-estacion + correo de contacto + aviso de problemas operativos" -m "- Usuario.EstacionId (adscripcion) expuesto en crear/editar y en la respuesta; indice" -m "- Estacion.CorreoContacto para el contacto/admin de la estacion" -m "- Digest por correo al final del ciclo: problemas operativos de la estacion a su contacto + usuarios adscritos" -m "- Migracion UsuarioEstacionYContacto (columnas nullable); build verde, 94 pruebas Detectors"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
