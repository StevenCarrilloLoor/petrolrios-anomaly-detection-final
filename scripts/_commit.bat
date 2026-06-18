@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Detectores del inge: turno sin cerrar (operativa) y credito sin garante" -m "- CierreTurnoDto.EstadoTurno + extraccion trae turnos abiertos; CashFraudDetector regla TurnoSinCerrarHorasUmbral (18h, carril Operativa)" -m "- PaymentFraudDetector regla CreditoSinGaranteHabilitado (COD_GARA vacio = autorizacion indebida)" -m "- Sembradas en SeedData; 4 pruebas nuevas. Domain 16, Detectors 104, Api 29 verdes; agente compila"
echo.
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
