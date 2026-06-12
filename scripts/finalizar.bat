@echo off
setlocal
cd /d "%~dp0.."

echo [1/3] Test de regresion de detectores...
dotnet test tests\PetrolRios.Detectors.Tests > "test-final-%RANDOM%.log" 2>&1

echo [2/3] Limpiando archivos temporales...
del /q verificacion.log verif-result-*.log retest.log retest-result-*.log 2>nul
del /q _arranque\fbauth-*.log _arranque\fbcheck-*.log _arranque\dbcheck-*.log 2>nul
del /q _arranque\dberr-*.log _arranque\reset-*.log _arranque\apilog-*.log 2>nul
del /q _arranque\descargas-*.log _arranque\inserciones_resultado.log 2>nul
del /q _arranque\firebird_restore.log _arranque\check_fb.sql _arranque\fixauth.sql _arranque\tcptest.sql 2>nul
del /q _arranque\check_error.bat _arranque\copy_log.bat 2>nul
rmdir /s /q _sync_tmp 2>nul

echo [3/3] Commit de los cambios...
git add -A >> commit-result.log 2>&1
git commit -m "Mejoras post-jurado: 6 reglas nuevas, rediseno UI, reportes PDF/Excel, comentarios, metricas y fixes del pipeline real" >> commit-result.log 2>&1
git log --oneline -3 >> commit-result.log 2>&1
copy /Y commit-result.log "commit-final-%RANDOM%.log" > nul

echo Listo
timeout /t 5 > nul
