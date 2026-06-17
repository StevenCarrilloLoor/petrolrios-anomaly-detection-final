@echo off
setlocal
title PetrolRios - Limpiar cuenta de prueba + marcar verificados
set EMAIL=stevencarrilloloor@gmail.com

echo Marcando como verificados los usuarios del sistema existentes...
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "UPDATE usuarios SET \"EmailVerificado\"=true WHERE \"Email\" <> '%EMAIL%';"

echo Borrando dependencias y la cuenta de prueba (%EMAIL%)...
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "DELETE FROM refresh_tokens WHERE \"UsuarioId\" IN (SELECT \"Id\" FROM usuarios WHERE \"Email\"='%EMAIL%');"
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "DELETE FROM logs_auditoria WHERE \"UsuarioId\" IN (SELECT \"Id\" FROM usuarios WHERE \"Email\"='%EMAIL%');"
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "DELETE FROM asignaciones_alerta WHERE \"AuditorId\" IN (SELECT \"Id\" FROM usuarios WHERE \"Email\"='%EMAIL%');"
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "DELETE FROM usuarios WHERE \"Email\"='%EMAIL%';"

echo Verificando...
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Email\", \"EmailVerificado\" FROM usuarios ORDER BY \"Id\";"
echo.
echo Listo.
pause
