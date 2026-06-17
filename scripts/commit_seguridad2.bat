@echo off
setlocal
cd /d "%~dp0.."
if exist ".git\index.lock" del /F /Q ".git\index.lock"

git add -A
git commit -m "Seguridad (fase 2): autenticacion + RBAC en el panel del agente" -m "- El panel del agente exige iniciar sesion; configurar/sincronizar/actualizar quedan bloqueados sin sesion valida (cierra el hueco de que cualquiera reconfigure el agente)" -m "- RBAC real: las credenciales se verifican contra el servidor central y solo Administrador/Supervisor pueden administrar el agente (el rango de la empresa importa)" -m "- Respaldo local offline: contrasena local (PBKDF2, nunca en claro) para entrar cuando el central no esta disponible; configurable desde el panel" -m "- Bootstrap seguro: solo el primer setup (agente sin configurar y sin clave local) es abierto; una vez configurado el panel queda bloqueado" -m "- Middleware protege /api/* salvo login/logout/sesion; sesiones en memoria con cookie HttpOnly y expiracion" -m "Build OK, 100 pruebas en verde, frontend compila."

echo.
echo ===== RESULTADO =====
git log --oneline -1
git status --short
echo Listo. Cierre esta ventana.
pause
