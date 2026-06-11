@echo off
chcp 65001 >nul
title PetrolRios - 3. Frontend React + Vite
echo ============================================================
echo  PetrolRios - Frontend (React 19 + TypeScript + Vite + Tailwind)
echo ============================================================
echo.

cd /d "%~dp0\..\frontend"

if not exist node_modules (
    echo [1/2] node_modules no existe. Ejecutando npm install ...
    call npm install
    if errorlevel 1 (
        echo   [ERROR] npm install fallo.
        pause
        exit /b 1
    )
) else (
    echo [1/2] node_modules ya existe. Salto npm install.
)
echo.

echo [2/2] npm run dev ...
echo   Frontend disponible en: http://localhost:5173
echo   Proxy /api  -> http://localhost:5170
echo   Proxy /hubs -> http://localhost:5170 (WebSocket)
echo.
echo   Login de prueba:
echo     admin@petrolrios.com / Admin123!
echo.
echo   [INFO] Vite quedara corriendo en esta ventana. CIERRA con Ctrl+C.
echo ============================================================
echo.

call npm run dev

echo.
echo Vite se detuvo.
pause
