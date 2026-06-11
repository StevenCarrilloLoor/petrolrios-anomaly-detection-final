@echo off
chcp 65001 >nul
title PetrolRios - 1. PostgreSQL en Docker
echo ============================================================
echo  PetrolRios - Levantando PostgreSQL 16 en Docker
echo ============================================================
echo.

cd /d "%~dp0\.."

echo [1/3] Verificando docker-compose.yml...
if not exist docker-compose.yml (
    echo   [ERROR] No se encuentra docker-compose.yml en %CD%
    pause
    exit /b 1
)
type docker-compose.yml
echo.

echo [2/3] Lanzando contenedor petrolrios-postgres...
docker compose up -d
if errorlevel 1 (
    echo   [ERROR] docker compose fallo.
    pause
    exit /b 1
)
echo.

echo [3/3] Esperando a que PostgreSQL este healthy (max 30s)...
set /a TRIES=0
:WAIT_LOOP
set /a TRIES+=1
docker inspect --format "{{.State.Health.Status}}" petrolrios-postgres 2>nul | findstr /R "^healthy$" >nul
if not errorlevel 1 (
    echo   PostgreSQL HEALTHY.
    goto :DONE
)
if %TRIES% GEQ 15 (
    echo   [ATENCION] PostgreSQL aun no esta healthy. Revisa "docker ps" manualmente.
    goto :DONE
)
timeout /t 2 /nobreak >nul
goto :WAIT_LOOP

:DONE
echo.
echo ============================================================
echo  PostgreSQL listo en localhost:5432
echo    DB:       petrolrios
echo    Usuario:  petrolrios
echo    Password: petrolrios_dev_2025
echo ============================================================
echo.
docker ps --filter "name=petrolrios-postgres" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
echo.
echo Siguiente paso: 02_levantar_api.bat
echo.
pause
