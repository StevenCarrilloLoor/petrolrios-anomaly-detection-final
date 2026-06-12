@echo off
title PetrolRios - Consultar BD
echo === STAGING ===
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT COUNT(*) total, COUNT(*) FILTER (WHERE \"Procesada\"=false) sin_procesar FROM transacciones_staging;"
echo === ALERTAS (ultimas 10) ===
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Id\", \"TipoDetector\", \"NivelRiesgo\", \"Score\", LEFT(\"Descripcion\",60) descripcion FROM alertas ORDER BY \"Id\" DESC LIMIT 10;"
echo === ULTIMOS CICLOS ===
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Id\", \"FechaInicio\", \"Estado\", \"AlertasGeneradas\", \"DuracionSegundos\" FROM ejecuciones_job ORDER BY \"Id\" DESC LIMIT 5;"
echo === LOGS DE AUDITORIA (ultimos 5) ===
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Id\", \"Accion\", \"Entidad\", \"DireccionIp\", \"CreatedAt\" FROM logs_auditoria ORDER BY \"Id\" DESC LIMIT 5;"
pause
