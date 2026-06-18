@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Detector del inge: anulaciones recurrentes (kiting / cancelar-reingresar)" -m "- InvoiceAnomalyDetector regla AnulacionRecurrenteDiasMinimo (default 3): mismo punto de emision con anulaciones en varios dias distintos" -m "- Sembrada como regla editable; 2 pruebas nuevas. Domain 16, Detectors 108, Api 29 verdes" -m "Cierra los 4 patrones del inge: turno sin cerrar, credito sin garante, despacho no facturado y kiting"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
