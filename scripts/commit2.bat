@echo off
setlocal
cd /d "%~dp0.."
del /f /q .git\index.lock .git\HEAD.lock 2>nul
del /q ronda2*.log diagtest*.log apilog-*.log commit-ok-*.log retest*.log 2>nul
del /q scripts\prueba_ronda2.bat scripts\diag_test.bat scripts\copiar_apilog.bat scripts\commit.bat scripts\retest.bat scripts\finalizar.bat 2>nul
del /q _arranque\dberr-*.log _arranque\dbcheck-*.log _arranque\fbauth-*.log _arranque\fbcheck-*.log _arranque\reset-*.log 2>nul
git add -A > nul 2>&1
git commit -m "Ronda 2: logs de auditoria activos, monitoreo de conexiones, reglas honestas, panel del agente, plug-and-play y publicacion .exe" > nul 2>&1
git log --oneline -2 > "commit2-ok-%RANDOM%.log" 2>&1
echo Listo
timeout /t 4 > nul
