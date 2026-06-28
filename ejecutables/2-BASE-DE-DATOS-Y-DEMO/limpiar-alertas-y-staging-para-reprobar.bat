@echo off
REM ============================================================
REM RESUMEN (que hace este script):
REM LIMPIAR ALERTAS + STAGING PARA RE-PROBAR (deja todo en CERO)
REM Pone en 0 las alertas (y sus vistas/asignaciones/comentarios), el staging de transacciones y el
REM historial de ciclos del job, SIN tocar estaciones, usuarios, reglas ni catalogos (empleados, etc).
REM Sirve para volver a probar la re-sincronizacion de un mes (p. ej. SanPio) con las reglas
REM recalibradas (G1 total real, G2 despachos rapidos acumulables/escalables).
REM
REM ORDEN RECOMENDADO PARA PROBAR:
REM   1) Reinicia la CENTRAL con el build nuevo (1-INICIAR-Y-DETENER\iniciar-todo-el-sistema.bat):
REM      al arrancar aplica la migracion AlertaAcumulable y el seed (regla -> ambito Ambos).
REM   2) Corre ESTE script (escribe SI para confirmar).
REM   3) En el panel del agente de SanPio (http://localhost:5180) usa
REM      "Re-sincronizar desde <fecha>" -> "Re-enviar datos" para reinyectar el mes.
REM   Las alertas nuevas saldran acumulables/escalables (una por caso, escalando por cantidad).
REM ============================================================
title PetrolRios - Limpiar alertas y staging para re-probar
setlocal
set PSQL=docker exec petrolrios-postgres psql -U petrolrios -d petrolrios

echo Esto BORRA todas las alertas y el staging del central.
echo NO toca estaciones, usuarios, reglas ni catalogos.
echo.
echo === ANTES ===
%PSQL% -c "SELECT (SELECT COUNT(*) FROM alertas) AS alertas, (SELECT COUNT(*) FROM transacciones_staging) AS staging, (SELECT COUNT(*) FROM ejecuciones_job) AS ciclos;"
echo.
set /p OK="Escribe SI (mayusculas) para borrar, cualquier otra cosa cancela: "
if /I not "%OK%"=="SI" (
  echo Cancelado. No se borro nada.
  pause
  exit /b 0
)

%PSQL% -c "TRUNCATE TABLE alertas, alertas_vistas, asignaciones_alerta, comentarios_alerta, transacciones_staging, ejecuciones_job RESTART IDENTITY CASCADE;"

echo.
echo === DESPUES (todo debe estar en 0) ===
%PSQL% -c "SELECT (SELECT COUNT(*) FROM alertas) AS alertas, (SELECT COUNT(*) FROM transacciones_staging) AS staging, (SELECT COUNT(*) FROM ejecuciones_job) AS ciclos;"

echo.
echo Listo. Ahora, en el panel del agente de SanPio: "Re-sincronizar desde <fecha>" -> "Re-enviar datos".
echo (Si cambiaste codigo del central, reinicialo ANTES para que tome el build nuevo y aplique la migracion.)
pause
