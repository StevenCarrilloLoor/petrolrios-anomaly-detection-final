@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Pestania 'Problemas de estacion': endpoint de agregacion + vista agrupada" -m "- GET /alertas/problemas-estacion: ambito Operativa agrupado por estacion y dia con conteo y lista" -m "- AlertaResponse expone Ambito" -m "- Frontend: pagina ProblemasEstacionPage + ruta + item de menu; grupos expandibles, ventana 1/7/30 dias, refresco 30s"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
