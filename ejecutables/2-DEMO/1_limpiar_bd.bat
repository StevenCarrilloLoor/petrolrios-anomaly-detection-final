@echo off
title PetrolRios - Limpiar BD para demo
echo Vaciando alertas, staging y ejecuciones (se conservan estaciones, reglas y usuarios)...
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "TRUNCATE comentarios_alerta, asignaciones_alerta, alertas RESTART IDENTITY CASCADE; TRUNCATE transacciones_staging RESTART IDENTITY; TRUNCATE ejecuciones_job RESTART IDENTITY CASCADE;"
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT (SELECT COUNT(*) FROM alertas) alertas, (SELECT COUNT(*) FROM transacciones_staging) staging, (SELECT COUNT(*) FROM reglas_deteccion) reglas;"
pause
