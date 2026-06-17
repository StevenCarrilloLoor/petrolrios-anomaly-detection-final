@echo off
setlocal
title PetrolRios - Establecer contrasena del admin
echo Estableciendo la contrasena del administrador...
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "UPDATE usuarios SET \"PasswordHash\"='$2b$11$R1N6hE62gzCCHQLnvEb2QuGy/TdztkCIsRjP0fkKzCOaCjYidDO/6', \"DebeCambiarPassword\"=false, \"AccessFailedCount\"=0, \"LockoutEnd\"=NULL WHERE \"Email\"='admin@petrolrios.com';"
docker exec petrolrios-postgres psql -U petrolrios -d petrolrios -c "SELECT \"Email\", \"DebeCambiarPassword\" FROM usuarios WHERE \"Email\"='admin@petrolrios.com';"
echo.
echo Listo: admin@petrolrios.com ahora usa la contrasena solicitada.
pause
