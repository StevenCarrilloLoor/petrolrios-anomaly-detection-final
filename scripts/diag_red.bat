@echo off
cd /d "%~dp0.."
set "OUT=scripts\diag_red.txt"
> "%OUT%" echo === PUERTO 5170 (LISTENING) ===
netstat -ano | findstr :5170 >> "%OUT%" 2>&1
echo. >> "%OUT%"
echo === IPs de esta maquina (IPv4) === >> "%OUT%"
ipconfig | findstr /i "IPv4" >> "%OUT%" 2>&1
echo. >> "%OUT%"
echo === REGLA FIREWALL 5170 === >> "%OUT%"
netsh advfirewall firewall show rule name="PetrolRios Central 5170" >> "%OUT%" 2>&1
echo. >> "%OUT%"
echo === PROCESOS dotnet === >> "%OUT%"
tasklist | findstr /i "dotnet" >> "%OUT%" 2>&1
echo Listo > "%OUT%.done"
