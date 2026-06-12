@echo off
setlocal
cd /d "%~dp0"
echo SELECT COUNT(*) AS TOTAL_DCTO FROM DCTO; SELECT COUNT(*) AS TOTAL_TURN FROM TURN; SELECT COUNT(*) AS TOTAL_DESP FROM DESP; > /tmp_check.sql 2>nul
(
echo SELECT COUNT(*^) AS TOTAL_DCTO FROM DCTO;
echo SELECT COUNT(*^) AS TOTAL_TURN FROM TURN;
echo SELECT COUNT(*^) AS TOTAL_DESP FROM DESP;
) > check_fb.sql
docker cp check_fb.sql petrolrios-firebird:/tmp/check_fb.sql
docker exec petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -pas masterkey /firebird/data/CONTAC.FDB -i /tmp/check_fb.sql > "fbcheck-%RANDOM%.log" 2>&1
echo Listo
timeout /t 3 > nul
