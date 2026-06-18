@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Guia de despliegue en la nube + backlog actualizado" -m "- docs/DESPLIEGUE-NUBE.md: PostgreSQL gestionado (Azure Flexible Server / AWS RDS), TLS, firewall, connection string por env, migraciones, secretos, checklist" -m "- docs/PENDIENTES.md: estado actualizado (plataforma, detectores del inge, limpieza de bats, guia nube hechos)"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
del /F /Q "%~f0"
