@echo off
setlocal
cd /d "%~dp0.."
echo ===== RETEST ===== > retest.log
dotnet test >> retest.log 2>&1
copy /Y retest.log "retest-result-%RANDOM%.log" > nul
echo Listo. (cierra en 5 s)
timeout /t 5 > nul
