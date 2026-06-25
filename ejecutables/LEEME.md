# Scripts de PetrolRíos — índice

Todos los scripts operativos del proyecto viven aquí, ordenados por tipo. Cada `.bat`/`.ps1`/`.sh`
trae al inicio un bloque **RESUMEN** que explica qué hace. Para ejecutar uno: doble clic, o pégalo en
la barra de direcciones del Explorador de archivos y Enter.

> Los nombres son descriptivos a propósito. Si buscas "el de antes", aquí está el mapa.

## 1-INICIAR-Y-DETENER — arrancar y parar el sistema (o partes)
- **iniciar-todo-el-sistema.bat** — arranca TODO (Docker, PostgreSQL, Firebird, API, frontend, agente, monitor) y abre los paneles. *(antes `INICIAR_TODO.bat`)*
- **detener-todo-el-sistema.bat** — cierra app y contenedores. *(antes `DETENER_TODO.bat`)*
- **iniciar-central-accesible-por-red.bat** / **reiniciar-central-accesible-por-red.bat** — la API en `0.0.0.0:5170` para estaciones remotas (ZeroTier/Tailscale).
- **iniciar-monitor-de-estacion.bat** — panel de solo lectura de la estación (`:5190`).
- **reiniciar-solo-la-api.bat** — relanza la API con `dotnet run` (aplica migraciones EF). *(antes `reiniciar_api.bat`)*
- **reiniciar-solo-el-frontend.bat** / **iniciar-solo-el-frontend-dev.bat** — Vite (`:5173`).

## 2-BASE-DE-DATOS-Y-DEMO — Firebird, datos demo y consultas
- **levantar-firebird-y-restaurar-contaplus.bat** — crea el contenedor Firebird y restaura el backup real de Contaplus. *(antes `_arranque/05_firebird_demo.bat`)*
- **reparar-autenticacion-de-firebird.bat** — recrea SYSDBA y prueba la conexión. *(antes `fix_firebird_auth.bat`)*
- **restaurar-firebird-desde-cero.bat** — encadena los dos anteriores.
- **insertar-ventas-anomalas-en-firebird.bat** / **insertar-anulaciones-de-prueba-en-firebird.bat** — siembran datos en CONTAC.FDB para que el agente los detecte. *(la BD/backup viven en `firebird_data/`, no se versiona)*
- **limpiar-base-de-datos-central.bat** / **consultar-base-de-datos-central.bat** — vaciar o consultar PostgreSQL.

## 3-DIAGNOSTICO — estado y chequeos
- **ver-estado-de-servicios.bat** — contenedores, procesos y puertos. *(antes `estado_servicios.bat`)*
- **verificar-fuentes-dinamicas.bat** (+ `.ps1`) — confirma que las tablas configurables se sincronizan.

## 4-VERIFICACION-Y-PRUEBAS — antes de commitear
- **verificar-todo-gate-oficial.bat** — **GATE OFICIAL**: build Release + todas las pruebas + chequeo de migración EF + lint y build del frontend. *(antes `scripts/verificar-mejoras.bat`)*
- **paso-build-migracion-tests-commit.bat** — plantilla todo-en-uno (build + migración + tests + commit).
- **compilar-solo-el-frontend.bat** — `npm run build` con resumen.
- **generar-reporte-de-cobertura.ps1** / **.sh** — cobertura de pruebas (OE5).

## 5-PUBLICACION-Y-DESPLIEGUE — generar ejecutables y desplegar
- **publicar-servidor-agente-y-monitor.bat** — publica los 3 `.exe` self-contained en `dist/`. *(antes `4-PUBLICACION/publicar.bat`)*
- **publicar-solo-el-agente-multiplataforma.bat** — cross-publica el agente (Win/Linux/macOS). *(antes `scripts/publicar_agente.bat`)*
- **instalar-agente-como-servicio-windows.bat / -linux.sh / -macos.sh** y **instalar-monitor-como-servicio-windows.bat** — registran los `.exe` como servicio (se copian junto al ejecutable publicado).
- **actualizar-central-remoto.ps1** / **.sh** — actualizan el central en producción por SSH.
- `instalador_*.iss` (Inno Setup) y `agente-LEEME-*.txt` (se incrustan en `dist/`).

## 6-INSTALAR-EN-NUEVO-PC — instalación guiada del central
- **instalar-central-windows.bat** (→ `.ps1`) / **instalar-central-linux-mac.sh** — instalan Docker + PostgreSQL + central en una PC nueva. *(antes carpeta `INSTALACION/`)*
- **GUIA.md** — guía detallada paso a paso.

---

### Notas
- `dist/` (ejecutables publicados) y `firebird_data/` (BD/backup) **no se versionan**; se regeneran al publicar o al levantar Firebird.
- Se eliminaron scripts obsoletos: los verificadores de rondas puntuales (`verificar_2fa`, `verificar_ronda_fuentes`) y wrappers que solo llamaban a otro script. El gate oficial los reemplaza.
