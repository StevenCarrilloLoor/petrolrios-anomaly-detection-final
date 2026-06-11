@echo off
chcp 65001 >nul
title PetrolRios - 4. Datos demo de staging (sin Firebird)
echo ============================================================
echo  PetrolRios - Inyectar transacciones sinteticas en staging
echo  para forzar deteccion de anomalias en la proxima corrida
echo  del job de Hangfire (cada 5 min, o "Trigger now").
echo ============================================================
echo.
echo  Este paso es OPCIONAL: solo es util si NO vas a levantar
echo  Firebird + StationAgent. Inserta facturas, cierres de turno
echo  y depositos sinteticos en la tabla TransaccionesStaging
echo  con datos que disparan las 4 categorias de detectores.
echo.

cd /d "%~dp0"
if not exist seed_demo.sql (
    echo   [ERROR] No se encuentra seed_demo.sql junto a este .bat
    pause
    exit /b 1
)

echo Copiando seed_demo.sql al contenedor de Postgres...
docker cp seed_demo.sql petrolrios-postgres:/tmp/seed_demo.sql
if errorlevel 1 (
    echo   [ERROR] docker cp fallo. Asegurate de que el contenedor petrolrios-postgres este corriendo.
    pause
    exit /b 1
)

echo Ejecutando seed dentro de Postgres...
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -f /tmp/seed_demo.sql
if errorlevel 1 (
    echo   [ERROR] psql fallo. Revisa el SQL.
    pause
    exit /b 1
)

echo.
echo ============================================================
echo  Staging sembrado. Para forzar la deteccion ya mismo:
echo    1) Entra al dashboard Hangfire: http://localhost:5170/hangfire
echo    2) Ve a "Recurring Jobs" -> "anomaly-detection" -> "Trigger now"
echo    3) Refresca el frontend en http://localhost:5173/alertas
echo ============================================================
echo.
pause
