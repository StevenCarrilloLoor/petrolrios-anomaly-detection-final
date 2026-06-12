@echo off
setlocal
rem ============================================================
rem  Arranca el Station Agent (Worker .NET) apuntando al
rem  Firebird real restaurado (localhost:3050, CONTAC.FDB).
rem  El agente extrae con watermark y empuja a /api/v1/ingesta.
rem ============================================================
cd /d "%~dp0..\src\PetrolRios.StationAgent"

rem Reiniciar watermark para que tome las transacciones recientes
if exist pending\watermark.txt del pending\watermark.txt

echo Station Agent EST-001 — Firebird localhost:3050 - API localhost:5170
dotnet run
pause
