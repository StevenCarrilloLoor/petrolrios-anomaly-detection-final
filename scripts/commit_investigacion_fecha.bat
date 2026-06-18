@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Investigacion del esquema Firebird + detector de fecha fuera de rango (backdating)" -m "- docs/investigacion-deteccion-anomalias.md: analisis tabla por tabla, patrones de fraude validados con industria, plan de 6 fases hacia plataforma configurable" -m "- InvoiceAnomalyDetector regla 6: fecha futura/backdating (FechaFuturaToleranciaHoras, default 24h) sobre DCTO y CRED_CABE" -m "- Sembrada en SeedData (editable en Reglas) + 3 pruebas unitarias nuevas (92 en total, verde)"

echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
