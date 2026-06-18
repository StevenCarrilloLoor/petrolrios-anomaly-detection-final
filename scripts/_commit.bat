@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Pruebas para lo nuevo: idempotencia, ambito, motor generico y backdating de creditos" -m "- TransaccionStagingTests: hash determinista, longitud, sensibilidad por componente, Create asigna hash" -m "- AlertaAmbitoTests: default Auditoria, conserva Operativa" -m "- CustomRuleDetectorTests: agregacion, condicion de texto y expresion-no-cumple sobre fuente generica" -m "- InvoiceAnomalyDetectorTests: credito fechado al futuro" -m "Totales: Domain 16, Detectors 100, Api 29 (verde)"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
