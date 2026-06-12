@echo off
title PetrolRios - Detener sistema
echo Deteniendo servicios de PetrolRios...

taskkill /F /IM PetrolRios.Api.exe >nul 2>&1 && echo  - API detenida
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1 && echo  - Agente detenido
taskkill /F /FI "WINDOWTITLE eq PetrolRios API*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq PetrolRios Agente*" >nul 2>&1
taskkill /F /FI "WINDOWTITLE eq PetrolRios Frontend*" >nul 2>&1 && echo  - Frontend detenido

docker stop petrolrios-postgres >nul 2>&1 && echo  - PostgreSQL detenido
docker stop petrolrios-firebird >nul 2>&1 && echo  - Firebird detenido

echo Listo.
timeout /t 4 > nul
