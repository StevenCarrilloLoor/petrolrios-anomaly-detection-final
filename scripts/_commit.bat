@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Plataforma flexible (1/3): explorador de tablas Firebird con documentacion automatica" -m "- FirebirdExtractor.ListarTablasAsync / DescribirTablaAsync (RDB$RELATIONS / RDB$RELATION_FIELDS), solo lectura" -m "- Verifica existencia (lista blanca anti-inyeccion) y mapea tipos a nombres legibles + conteo de filas" -m "- Endpoints del agente /api/firebird/tablas y /api/firebird/tabla/{nombre}" -m "- Panel del agente: tarjeta 'Explorador de tablas (documentacion automatica)'"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
