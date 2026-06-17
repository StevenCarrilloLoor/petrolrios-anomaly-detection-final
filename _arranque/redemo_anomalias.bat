@echo off
setlocal
cd /d "%~dp0"
title PetrolRios - Re-demo anomalias (borra + inserta)

echo [1/3] Copiando SQLs al contenedor Firebird...
docker cp "redemo_delete.sql" petrolrios-firebird:/tmp/del.sql
docker cp "inserciones_anomalias.sql" petrolrios-firebird:/tmp/ins.sql
if errorlevel 1 ( echo ERROR: contenedor petrolrios-firebird no esta corriendo. & pause & exit /b 1 )

echo [2/3] Borrando anomalias previas...
docker exec petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -pas masterkey /firebird/data/CONTAC.FDB -i /tmp/del.sql

echo [3/3] Insertando ventas anomalas nuevas (fecha actual)...
docker exec petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -pas masterkey /firebird/data/CONTAC.FDB -i /tmp/ins.sql > redemo_resultado.log 2>&1
type redemo_resultado.log

echo.
echo Listo. El Station Agent las tomara en su proximo ciclo (max 30 s).
pause
