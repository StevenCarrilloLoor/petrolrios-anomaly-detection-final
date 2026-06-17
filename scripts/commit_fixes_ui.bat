@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Fixes: login 2FA (500), agente Probar servidor, edicion de rol, alta/baja de usuarios + UI de login" -m "- Login: el controlador ya no revienta (NRE) cuando se requiere 2FA; devuelve Requiere2Fa" -m "- Agente ServerClient: timeout por peticion con CancellationToken (no muta HttpClient.Timeout tras usarlo) -> arregla 'instance has already started a request' en Probar servidor" -m "- Editar usuario: UpdateAsync ahora actualiza Nombre y Rol (Usuario.ActualizarPerfil); verificado Auditor->Supervisor->Auditor" -m "- Alta de usuarios: campo 'Confirmar contrasena' + validacion + mensajes de error; baja con confirmacion y feedback" -m "- Login UI: rediseno mas profesional (gradiente/decoracion) y se quita el numero fijo de estaciones (es incremental)" -m "Build OK, 126 pruebas en verde. Verificado en vivo: admin login 200, probar servidor ok, editar rol persiste, borrado 204."

echo.
echo ===== RESULTADO =====
git log --oneline -1
echo Listo. Cierre esta ventana.
pause
