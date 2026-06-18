@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
if exist "scripts\_ba.txt" del /F /Q "scripts\_ba.txt"
git add -A
git commit -m "Agente: autodeteccion de Firebird (boton) + pendientes nuevos anotados" -m "- FirebirdExtractor.AutodetectarAsync: pre-chequeo TCP + prueba de host/puerto/ruta/WireCrypt comunes; primera que conecta gana" -m "- Endpoint POST /api/autodetectar-firebird; boton 'Detectar Firebird automaticamente' en el panel que rellena host/puerto/ruta" -m "- docs/PENDIENTES.md: apartado config conexiones (admin), usuarios conectados en monitoreo, pruebas Chrome"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
del /F /Q "%~f0"
