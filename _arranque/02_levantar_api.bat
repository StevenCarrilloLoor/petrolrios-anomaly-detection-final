@echo off
chcp 65001 >nul
title PetrolRios - 2. API ASP.NET Core 9
echo ============================================================
echo  PetrolRios - Backend API (ASP.NET Core 9 + Hangfire + SignalR)
echo ============================================================
echo.

cd /d "%~dp0\..\src\PetrolRios.Api"

echo [1/3] dotnet restore (solucion completa)...
pushd "%~dp0\.."
dotnet restore PetrolRios.sln
if errorlevel 1 (
    echo   [ERROR] dotnet restore fallo.
    popd
    pause
    exit /b 1
)
popd
echo.

echo [2/3] dotnet build PetrolRios.Api ...
dotnet build --no-restore -c Debug
if errorlevel 1 (
    echo   [ERROR] dotnet build fallo.
    pause
    exit /b 1
)
echo.

echo [3/3] dotnet run ...
echo   Endpoints expuestos:
echo     - API:                 http://localhost:5170
echo     - Swagger:              http://localhost:5170/swagger
echo     - Hangfire dashboard:   http://localhost:5170/hangfire
echo     - SignalR alerts hub:   ws://localhost:5170/hubs/alerts
echo.
echo   Usuarios sembrados:
echo     - admin@petrolrios.com / Admin123!
echo     - agent-est-001@petrolrios.com / Agent123! (uno por estacion EST-001..EST-010)
echo.
echo   [INFO] La API quedara corriendo en esta ventana. CIERRA con Ctrl+C.
echo ============================================================
echo.

set ASPNETCORE_ENVIRONMENT=Development
dotnet run --no-build --launch-profile http

echo.
echo La API se detuvo.
pause
