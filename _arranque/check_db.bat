@echo off
setlocal
cd /d "%~dp0"
set LOG=dbcheck-%RANDOM%.log
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT COUNT(*) total, COUNT(*) FILTER (WHERE \"Procesada\"=false) sin_procesar FROM transacciones_staging;" > %LOG% 2>&1
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Id\",\"TipoTransaccion\",\"Procesada\",\"FechaOriginal\",\"CreatedAt\" FROM transacciones_staging ORDER BY \"Id\" DESC LIMIT 16;" >> %LOG% 2>&1
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT COUNT(*) FROM alertas;" >> %LOG% 2>&1
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Id\",\"TipoDetector\",\"Score\",\"FechaDeteccion\" FROM alertas ORDER BY \"Id\" DESC LIMIT 5;" >> %LOG% 2>&1
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Id\",\"FechaInicio\",\"Estado\",\"AlertasGeneradas\",\"DuracionSegundos\" FROM ejecuciones_job ORDER BY \"Id\" DESC LIMIT 5;" >> %LOG% 2>&1
echo Listo
timeout /t 3 > nul
