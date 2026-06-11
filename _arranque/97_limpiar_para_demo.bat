@echo off
chcp 65001 >nul
title PetrolRios - Limpiar BD para demo real
echo ============================================================
echo  Limpieza de la BD para demo en vivo:
echo    - VACIA  alertas, transacciones_staging, ejecuciones_job,
echo            asignaciones_alerta, logs_auditoria
echo    - MANTIENE roles, usuarios, estaciones, reglas_deteccion,
echo               estacion_watermarks (datos de referencia)
echo  Despues de esta limpieza, las unicas alertas que apareceran
echo  seran las que venga del Station Agent leyendo Firebird REAL.
echo ============================================================
echo.

REM Reset de watermarks: arrancan desde "hace 5 minutos" para que el
REM agente solo levante lo NUEVO (lo que insertemos en la demo).
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -P pager=off ^
    -c "TRUNCATE TABLE alertas RESTART IDENTITY CASCADE;" ^
    -c "TRUNCATE TABLE transacciones_staging RESTART IDENTITY;" ^
    -c "TRUNCATE TABLE ejecuciones_job RESTART IDENTITY;" ^
    -c "TRUNCATE TABLE logs_auditoria RESTART IDENTITY;" ^
    -c "UPDATE estacion_watermarks SET \"UltimaExtraccion\" = NOW() AT TIME ZONE 'UTC' - INTERVAL '5 minutes';"

if errorlevel 1 (
    echo   [ERROR] La limpieza fallo. Esta corriendo el contenedor petrolrios-postgres?
    pause
    exit /b 1
)

echo.
echo Estado tras limpieza:
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -P pager=off ^
    -c "SELECT 'alertas' AS tabla, COUNT(*) FROM alertas UNION ALL SELECT 'staging', COUNT(*) FROM transacciones_staging UNION ALL SELECT 'ejecuciones', COUNT(*) FROM ejecuciones_job UNION ALL SELECT 'estaciones', COUNT(*) FROM estaciones UNION ALL SELECT 'reglas', COUNT(*) FROM reglas_deteccion;"

echo.
echo ============================================================
echo  BD lista para demo. Ahora:
echo    1) Restaurar Firebird   -> 05_firebird_demo.bat
echo    2) Arrancar StationAgent -> 06_levantar_station_agent.bat
echo    3) Insertar anomalias    -> 96_insertar_anomalias_firebird.bat
echo ============================================================
pause
