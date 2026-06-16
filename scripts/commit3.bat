@echo off
setlocal
cd /d "%~dp0.."
del /f /q .git\index.lock .git\HEAD.lock 2>nul
del /q ronda3*.log diagtest*.log persalert-*.log verifpers-*.log diagpers-*.log commit2-ok-*.log 2>nul
del /q scripts\prueba_ronda3.bat scripts\diag_test.bat scripts\verificar_personalizada.bat scripts\check_pers_alertas.bat scripts\diag_pers.bat scripts\commit2.bat scripts\prueba_ronda2.bat 2>nul
git add -A > nul 2>&1
git commit -m "Ronda 3: heartbeat del agente, estaciones dinamicas con CRUD, motor de reglas personalizadas, branding profesional y tiempo real" > nul 2>&1
git log --oneline -3 > "commit3-ok-%RANDOM%.log" 2>&1
echo Listo
timeout /t 4 > nul
