@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A

git commit -m "Empaquetar el agente dentro del repo: plantilla de config sin secretos + script de publicacion" -m "- agent-config.example.json: plantilla versionada con valores por defecto y SIN contrasenas; se copia a la salida en cada publicacion" -m "- .gitignore: ignora el config real por estacion (agent-config.json) que contiene contrasenas; mantiene la plantilla" -m "- scripts/publicar_agente.bat: dotnet publish self-contained single-file (win-x64) a dist/agente, con LEEME.txt de instalacion" -m "- scripts/agente-LEEME.txt: instrucciones de instalacion en la estacion" -m "- csproj: copia la plantilla de config a la salida de publicacion" -m "Deploy = correr publicar_agente.bat, copiar dist/agente a la estacion y ejecutar el .exe. El codigo del agente ya vivia en el repo (monorepo)."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo.
echo Listo. Cierre esta ventana.
pause
