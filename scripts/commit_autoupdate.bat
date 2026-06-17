@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Auto-actualizacion remota del agente con control de versiones" -m "- Directory.Build.props: version unica (2.2.0) para todo el sistema; el agente reporta su version de ensamblado (se elimina el 2.1 hardcodeado)" -m "- Servidor: GET /api/v1/agente/version (anonimo) sirve el manifiesto editable desde config/agente-version.json o appsettings; ejemplo versionado" -m "- Agente: UpdateService consulta el feed (URL configurable + fallback), compara semver, y aplica con un clic (descarga, verifica sha256, intercambia el .exe y reinicia servicio/proceso)" -m "- Worker revisa el feed cada ~5 min; panel del agente muestra banner 'actualizacion disponible' + botones Buscar/Aplicar y campos de URL de feed" -m "- Frontend central (Conexiones): badge 'actualizacion vX disponible' por estacion comparando su version con el manifiesto" -m "Build OK, 100 pruebas unitarias en verde, frontend compila."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo Listo. Cierre esta ventana.
pause
