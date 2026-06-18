@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Subsistema de alertas por ambito: Operativa (estacion) vs Auditoria (fraude)" -m "- Enum AmbitoAlerta + Alerta.Ambito persistido + migracion (previas=Auditoria) + indices" -m "- DetectedAnomaly.Ambito: detectores etiquetan el carril; campos obligatorios vacios = Operativa" -m "- SignalR: central ve todo; grupo de estacion recibe solo operativas via evento ProblemaEstacion" -m "- 2 pruebas nuevas de ambito (94 en Detectors, verde)" -m "Pendiente: pestania Problemas de estacion, endpoint de agregacion, usuario-estacion y correo al contacto"

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
