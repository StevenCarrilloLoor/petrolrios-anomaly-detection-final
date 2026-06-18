@echo off
setlocal
cd /d "%~dp0.."
dotnet build PetrolRios.sln -clp:NoSummary > scripts\build_errors.txt 2>&1
echo --- ERRORES --- >> scripts\build_errors.txt
findstr /i "error CS" scripts\build_errors.txt
echo Listo > scripts\ver_errores_build.done
pause
