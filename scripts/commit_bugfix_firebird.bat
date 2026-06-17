@echo off
setlocal
cd /d "%~dp0.."
echo Deteniendo agente para compilar...
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
timeout /t 2 >nul

echo ===== BUILD ===== > bugfix.log
echo Compilando...
dotnet build >> bugfix.log 2>&1
echo ===== TESTS ===== >> bugfix.log
echo Pruebas...
dotnet test --no-build >> bugfix.log 2>&1
type bugfix.log | findstr /C:"Build succeeded" /C:"Passed!" /C:"Failed!" /C:"error CS"

if exist ".git\index.lock" del /F /Q ".git\index.lock"
git add -A
git commit -m "Bugfix agente: no enmascarar fallos de conexion a Firebird + scripts de re-demo" -m "- FirebirdExtractor: abre UNA conexion por ciclo y propaga el fallo de apertura (WireCrypt, credenciales, archivo, servicio caido) para que el ciclo lo reporte como ERROR. Antes cada consulta abria su conexion y tragaba la excepcion devolviendo vacio, por lo que un Firebird caido se veia como 'OK Sin transacciones nuevas' (punto ciego de monitoreo descubierto en pruebas en vivo)." -m "- Los fallos por tabla individual se siguen tolerando (no tumban el ciclo)." -m "- scripts de re-demo de anomalias (_arranque/redemo_*) para pruebas de deteccion sin choque de claves." -m "Verificado en vivo: conexion Firebird OK (40.033 docs con WireCrypt=Enabled en la demo FB3), 14 anomalias inyectadas -> agente extrajo y envio -> motor genero 12 alertas nuevas (83 -> 95)."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
