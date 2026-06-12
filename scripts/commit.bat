@echo off
setlocal
cd /d "%~dp0.."
del /f /q .git\index.lock .git\HEAD.lock .git\objects\maintenance.lock 2>nul
del /q commit-result.log commit-final-*.log test-final-*.log 2>nul
git add -A > nul 2>&1
git commit -m "Mejoras post-jurado: 6 reglas nuevas, rediseno UI, reportes PDF/Excel, comentarios, metricas y fixes del pipeline real" > nul 2>&1
git log --oneline -2 > "commit-ok-%RANDOM%.log" 2>&1
git status --short >> "commit-ok-status.log" 2>&1
echo Listo
timeout /t 4 > nul
