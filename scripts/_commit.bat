@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Plataforma flexible (3/3 UI): el builder de reglas auto-descubre las fuentes configurables" -m "- /reglas-personalizadas/catalogo async: descubre fuentes configurables del staging y auto-documenta campos (tipo inferido)" -m "- EvaluadorExpresion.Validar: en fuentes configurables valida solo sintaxis; en catalogo valida campos" -m "- Controlador de reglas acepta fuentes configurables al guardar" -m "Cierra el ciclo: explorar -> registrar fuente -> reglas basicas/avanzadas sin tocar codigo. 96 pruebas Detectors verdes"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
