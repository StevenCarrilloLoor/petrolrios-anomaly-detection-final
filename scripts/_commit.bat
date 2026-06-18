@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Detector del inge: despacho no facturado (combustible servido sin cobrar)" -m "- DetalleFacturaDto.Facturado (FAC_DESP) + extraccion; InvoiceAnomalyDetector regla DespachoNoFacturadoHabilitado (carril Operativa)" -m "- Si FAC_DESP vacio no asume nada (anti falso positivo); sembrada como regla editable" -m "- 2 pruebas nuevas. Domain 16, Detectors 106, Api 29 verdes; agente compila"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
