@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM PUBLICA UNA ACTUALIZACION DEL AGENTE (un clic).
REM Toma el exe del agente ya publicado (dist\agente-windows), calcula su SHA256,
REM lo copia a central-descargas\ y genera central-config\agente-version.json (el manifiesto).
REM Esas dos carpetas son las que monta docker-compose.prod.yml, asi que el central las sirve sin
REM reconstruir la imagen. Los agentes veran la actualizacion en su panel (boton "Aplicar").
REM Antes de esto: corre publicar-solo-el-agente-multiplataforma.bat para tener el exe nuevo.
REM ============================================================
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0publicar-actualizacion-del-agente.ps1" %*
pause
