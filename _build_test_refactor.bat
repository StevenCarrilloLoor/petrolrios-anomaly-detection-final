@echo off
cd /d "C:\Users\steve\Desktop\Proyecto Tesis\petrolrios-anomaly-detection"

echo Deteniendo servicios para liberar locks de los DLL...
taskkill /F /IM PetrolRios.Api.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationAgent.exe >nul 2>&1
taskkill /F /IM PetrolRios.StationMonitor.exe >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 3 /nobreak >nul

echo ===== BUILD SOLUCION ===== > _refactor.log
dotnet build PetrolRios.sln -c Debug >> _refactor.log 2>&1
echo BUILD_EXIT=%ERRORLEVEL% >> _refactor.log

echo ===== TEST DETECTORS ===== >> _refactor.log
dotnet test tests\PetrolRios.Detectors.Tests\PetrolRios.Detectors.Tests.csproj -c Debug --no-build >> _refactor.log 2>&1
echo TEST_DETECTORS_EXIT=%ERRORLEVEL% >> _refactor.log

echo ===== TEST DOMAIN ===== >> _refactor.log
dotnet test tests\PetrolRios.Domain.Tests\PetrolRios.Domain.Tests.csproj -c Debug --no-build >> _refactor.log 2>&1
echo TEST_DOMAIN_EXIT=%ERRORLEVEL% >> _refactor.log

echo ===== TEST API ===== >> _refactor.log
dotnet test tests\PetrolRios.Api.Tests\PetrolRios.Api.Tests.csproj -c Debug --no-build >> _refactor.log 2>&1
echo TEST_API_EXIT=%ERRORLEVEL% >> _refactor.log

echo ===== DONE ===== >> _refactor.log
