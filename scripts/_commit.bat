@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Plataforma flexible (3/3 motor): reglas (basica y avanzada) sobre cualquier fuente" -m "- DetectionContext.FuentesGenericas; el job deserializa las tablas configurables a diccionarios" -m "- CustomRuleDetector opera sobre fuentes genericas infiriendo tipo del valor; expresiones avanzadas incluidas" -m "- CatalogoReglasPersonalizadas: GetValor resuelve diccionarios; ValidarDefinicion acepta fuentes configurables" -m "- 2 pruebas nuevas (condicion y expresion sobre fuente generica); 96 en Detectors"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
