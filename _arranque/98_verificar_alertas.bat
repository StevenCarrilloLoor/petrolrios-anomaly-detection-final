@echo off
chcp 65001 >nul
title PetrolRios - Verificar alertas post-ingesta
echo ============================================================
echo  Verificacion del modelo push de extremo a extremo
echo ============================================================
echo.

echo [a] Total de alertas en la BD:
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -t -c ^
    "SELECT COUNT(*) FROM alertas;"
echo.

echo [b] Ultimas 10 alertas (id, tipo, nivel, descripcion):
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -P pager=off -c ^
    "SELECT \"Id\", \"TipoDetector\", \"NivelRiesgo\", \"Score\", LEFT(\"Descripcion\", 80) AS descripcion, \"FechaDeteccion\" FROM alertas ORDER BY \"Id\" DESC LIMIT 10;"
echo.

echo [c] Alertas generadas por las transacciones empujadas por el agent (PUSH-TEST):
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -P pager=off -c ^
    "SELECT a.\"Id\", a.\"TipoDetector\", a.\"NivelRiesgo\", a.\"Score\", a.\"Descripcion\", a.\"FechaDeteccion\" FROM alertas a WHERE a.\"MetadataJson\"::text LIKE '%%PUSH-TEST%%' OR a.\"Descripcion\" LIKE '%%900001%%' OR a.\"TransaccionReferencia\" LIKE '%%900001%%' ORDER BY a.\"Id\" DESC;"
echo.

echo [d] Estado del staging (cuantas filas procesadas vs pendientes):
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -P pager=off -c ^
    "SELECT \"Procesada\", COUNT(*) FROM transacciones_staging GROUP BY \"Procesada\";"
echo.

echo [e] Ultima ejecucion del job (metricas):
docker exec -i petrolrios-postgres psql -U petrolrios -d petrolrios -P pager=off -c ^
    "SELECT \"Id\", \"Estado\", \"AlertasGeneradas\", \"EstacionesProcesadas\", \"DuracionSegundos\", \"FechaInicio\", \"FechaFin\" FROM ejecuciones_job ORDER BY \"Id\" DESC LIMIT 3;"
echo.

echo ============================================================
echo  Si en [c] aparece una alerta de Compliance/ZZZ999949,
echo  entonces el ciclo PUSH -> staging -> Hangfire -> detector -> alerta
echo  esta funcionando de extremo a extremo.
echo ============================================================
pause
