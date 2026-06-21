@echo off
cd /d "C:\Users\steve\Desktop\Proyecto Tesis\petrolrios-anomaly-detection"
echo ===== TEST STATIONMONITOR ===== > _smtest.log
dotnet test tests\PetrolRios.StationMonitor.Tests\PetrolRios.StationMonitor.Tests.csproj -c Debug --no-build >> _smtest.log 2>&1
echo SM_EXIT=%ERRORLEVEL% >> _smtest.log
echo ===== DONE ===== >> _smtest.log
