@echo off
chcp 65001 >nul
title PetrolRios - 0. Verificacion de entorno
echo ============================================================
echo  PetrolRios - Verificacion de prerrequisitos
echo ============================================================
echo.

set ERR=0

echo [.NET SDK]
where dotnet >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] dotnet no encontrado en PATH.
    set ERR=1
) else (
    dotnet --version
)
echo.

echo [Node.js]
where node >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] node no encontrado en PATH.
    set ERR=1
) else (
    node --version
)
echo.

echo [npm]
where npm >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] npm no encontrado en PATH.
    set ERR=1
) else (
    npm --version
)
echo.

echo [Docker]
where docker >nul 2>&1
if errorlevel 1 (
    echo   [ERROR] docker no encontrado en PATH.
    set ERR=1
) else (
    docker --version
    echo.
    echo   Estado del demonio Docker:
    docker info --format "{{.ServerVersion}}" 2>nul
    if errorlevel 1 (
        echo   [ATENCION] Docker Desktop no esta corriendo. Abrelo manualmente y reintenta.
        set ERR=1
    ) else (
        echo   Docker Desktop esta operativo.
    )
)
echo.

echo [Puertos clave]
netstat -ano | findstr ":5170 :5173 :5432 :3050" | findstr LISTENING
echo.

if "%ERR%"=="0" (
    echo ============================================================
    echo  Entorno OK. Puedes continuar con 01_levantar_postgres.bat
    echo ============================================================
) else (
    echo ============================================================
    echo  Hay errores. Resuelvelos antes de continuar.
    echo ============================================================
)

echo.
pause
