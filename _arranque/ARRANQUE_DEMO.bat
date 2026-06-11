@echo off
chcp 65001 >nul
title PetrolRios - Arranque demo (orquestador)
echo ============================================================
echo  PetrolRios - Orquestador del demo
echo ============================================================
echo.
echo Este script lanzara en ventanas independientes:
echo   1) PostgreSQL via Docker Compose
echo   2) Backend API ASP.NET Core (puerto 5170)
echo   3) Frontend React/Vite (puerto 5173)
echo.
echo Despues podras opcionalmente:
echo   - Seed sintetico de staging (04_seed_staging_demo.bat)
echo   - Firebird de prueba + Station Agent (05 y 06)
echo.
pause

REM 1) Postgres
echo Lanzando 01_levantar_postgres.bat ...
start "PetrolRios - Postgres" cmd /k "call %~dp0\01_levantar_postgres.bat"

echo Esperando 5 segundos antes de lanzar el API...
timeout /t 5 /nobreak >nul

REM 2) API
echo Lanzando 02_levantar_api.bat ...
start "PetrolRios - API" cmd /k "call %~dp0\02_levantar_api.bat"

echo Esperando 25 segundos para que el API quede listo (migraciones + seed)...
timeout /t 25 /nobreak >nul

REM 3) Frontend
echo Lanzando 03_levantar_frontend.bat ...
start "PetrolRios - Frontend" cmd /k "call %~dp0\03_levantar_frontend.bat"

echo.
echo ============================================================
echo  Demo lanzado. Tres ventanas deberian estar corriendo.
echo.
echo  Abre el navegador en: http://localhost:5173
echo  Login: admin@petrolrios.com / Admin123!
echo.
echo  Otros endpoints:
echo    - Swagger:        http://localhost:5170/swagger
echo    - Hangfire:       http://localhost:5170/hangfire
echo ============================================================
echo.
pause
