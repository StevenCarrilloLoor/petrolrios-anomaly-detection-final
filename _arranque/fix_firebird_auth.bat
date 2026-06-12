@echo off
setlocal
cd /d "%~dp0"
set LOG=fbauth-%RANDOM%.log

echo === 1. Localizar security3.fdb === > %LOG%
docker exec petrolrios-firebird sh -c "find / -name 'security*.fdb' 2>/dev/null" >> %LOG% 2>&1

echo === 2. Crear/actualizar SYSDBA via conexion embebida === >> %LOG%
(
echo create or alter user SYSDBA password 'masterkey' using plugin Srp;
echo commit;
) > fixauth.sql
docker cp fixauth.sql petrolrios-firebird:/tmp/fixauth.sql
docker exec petrolrios-firebird sh -c "/usr/local/firebird/bin/isql -user SYSDBA /firebird/system/security3.fdb -i /tmp/fixauth.sql" >> %LOG% 2>&1

echo === 3. Probar conexion TCP === >> %LOG%
(
echo SELECT COUNT(*^) FROM DCTO;
) > tcptest.sql
docker cp tcptest.sql petrolrios-firebird:/tmp/tcptest.sql
docker exec petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -pas masterkey localhost:/firebird/data/CONTAC.FDB -i /tmp/tcptest.sql >> %LOG% 2>&1

echo Listo
timeout /t 3 > nul
