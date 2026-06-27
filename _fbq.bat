@echo off
cd /d "%~dp0"
echo ===FBQ=== > _fbq.log
type _fbq.sql | docker exec -i petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -password masterkey /firebird/data/CONTAC.FDB >> _fbq.log 2>&1
echo ===FIN=== >> _fbq.log
