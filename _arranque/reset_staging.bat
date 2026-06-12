@echo off
setlocal
cd /d "%~dp0"
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "UPDATE transacciones_staging SET \"Procesada\"=false WHERE \"CreatedAt\" > '2026-06-12';" > reset-%RANDOM%.log 2>&1
echo Listo
timeout /t 3 > nul
