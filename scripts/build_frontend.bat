@echo off
setlocal
cd /d "%~dp0..\frontend"
call npm run build > ..\fe.log 2>&1
type ..\fe.log | findstr /C:"built in" /C:"error TS" /C:"error during build" /C:"✓"
echo --- fin ---
pause
