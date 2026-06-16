@echo off
setlocal
cd /d "%~dp0.."
echo Limpiando lock de git si existe...
if exist ".git\index.lock" del /F /Q ".git\index.lock"

echo Anadiendo cambios...
git add -A

echo Creando commit ronda 4...
git commit -m "Ronda 4: agente como aplicacion individual + generador de reglas avanzado por expresiones" -m "Agente standalone:" -m "- AgentConfigStore: configuracion editable y persistente en disco (config/agent-config.json), reemplaza IOptions, cambios en caliente sin reiniciar" -m "- Nombre de estacion desde el panel del agente, se refleja en Conexiones del servidor central (auto-registro/actualizacion sin pisar nombres manuales)" -m "- Panel con pestanas Monitoreo/Configuracion, banner de bienvenida, opciones avanzadas de conexion Firebird (host, puerto, ruta, charset, dialect, WireCrypt) y botones de prueba por extremo" -m "- Arranque seguro en modo manual hasta configurar; heartbeat siempre activo" -m "Reglas avanzadas:" -m "- Motor de expresiones propio y seguro (Tokenizer/Parser/Evaluador), sin ejecucion de codigo arbitrario" -m "- Operadores y funciones (vacio, contiene, empieza, abs, longitud, etc.), aritmetica entre campos y condiciones compuestas" -m "- Editor en UI con paletas y validacion en vivo; CustomRuleDetector compila y filtra sin tumbar el ciclo" -m "- Columna ExpresionAvanzada + migracion EF ReglasPersonalizadasAvanzadas" -m "Verificado en vivo: agente EST-001 'Estacion Santo Domingo Centro' En linea en el central; regla avanzada validada y persistida. 113 pruebas en verde."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo.
echo Listo. Cierre esta ventana.
pause
