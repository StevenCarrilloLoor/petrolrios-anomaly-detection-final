@echo off
title PetrolRios - Estado de servicios
echo === CONTENEDORES DOCKER ===
docker ps -a --filter name=petrolrios --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.
echo === PROCESOS DE LA APLICACION ===
tasklist /FI "IMAGENAME eq PetrolRios.Api.exe" 2>nul | find /i "PetrolRios" && echo  (API corriendo) || echo  API: detenida
tasklist /FI "IMAGENAME eq PetrolRios.StationAgent.exe" 2>nul | find /i "PetrolRios" && echo  (Agente corriendo) || echo  Agente: detenido
echo.
echo === PUERTOS ===
netstat -ano | findstr ":5170 :5173 :5180 :5432 :3051" | findstr LISTENING
pause
