# Ejecutables de PetrolRíos — guía de cada script

Todos los scripts operativos del proyecto, organizados por propósito y orden de uso. **Se ejecutan
con doble clic desde el Explorador de archivos** (no desde una terminal con permisos restringidos).

> **Esto es para desarrollo / demo en tu máquina** (compila y corre con `dotnet run` y `npm`).
> Para **instalar en producción** (Docker, ejecutables self-contained, varias computadoras) usa la
> carpeta **`INSTALACION/`** y su `GUIA.md`.

| Carpeta | Para qué es | ¿Cuándo la usas? |
|---|---|---|
| `1-INICIO` | Arrancar y apagar todo el sistema. | Día a día. |
| `2-DEMO` | Poblar datos para la sustentación. | Antes de presentar. |
| `3-DIAGNOSTICO` | Reparar cuando algo falla. | Solo si hay problemas. |
| `4-PUBLICACION` | Generar los ejecutables e instaladores de distribución. | Al preparar un despliegue. |
| `5-DESARROLLO` | Verificar el código (build + pruebas). | Antes de cada commit. |

---

## 1-INICIO — arranque y apagado (uso diario)

| Script | Qué hace |
|---|---|
| `INICIAR_TODO.bat` | **El de siempre.** En 7 pasos: abre Docker (lo inicia si está cerrado), levanta PostgreSQL (`:5432`) y Firebird (`:3051`), compila y arranca la API central (`:5170`, espera a que responda), el frontend (`:5173`), el Station Agent con su panel (`:5180`) y el Monitor de estación (`:5190`); al final abre el navegador en la app, el panel del agente y el monitor. Cada servicio queda en su propia ventana. |
| `INICIAR_CENTRAL_RED.bat` | Arranca **solo el central escuchando en `0.0.0.0:5170`** (toda la red) para que estaciones remotas se conecten por una red mesh tipo **ZeroTier/Tailscale**. Abre el puerto 5170 en el Firewall de Windows (requiere ejecutar como Administrador) y muestra tus IPs para identificar la de la red mesh. |
| `REINICIAR_CENTRAL_RED.bat` | Igual que el anterior pero primero **libera el puerto 5170** (mata instancias previas), rehace la regla de firewall y asegura PostgreSQL/Firebird en Docker. Usa el perfil de arranque `red` (conserva el entorno y Hangfire). Úsalo cuando el central por red quedó colgado. |
| `INICIAR_MONITOR_ESTACION.bat` | Arranca únicamente el **Monitor de estación** (`:5190`) y abre su panel. Visor de solo lectura de los problemas operativos de la estación. |
| `DETENER_TODO.bat` | Cierra la API, el agente, el monitor y el frontend, y detiene los contenedores PostgreSQL y Firebird. |

Tras `INICIAR_TODO`: app **http://localhost:5173** (`admin@petrolrios.com` / `Admin123!`), panel del
agente **:5180**, monitor **:5190**, Swagger **:5170/swagger**, Hangfire **:5170/hangfire**.

---

## 2-DEMO — datos de demostración (para la sustentación)

Flujo: **limpiar → insertar → esperar al agente (≤30 s) → "Trigger now" en Hangfire (o esperar el
ciclo de 1 min) → ver las alertas en el dashboard.**

| Orden | Script | Qué hace |
|---|---|---|
| 1 | `1_limpiar_bd.bat` | Vacía alertas, staging y ejecuciones del job, dejando intactos estaciones, reglas y usuarios. Deja el dashboard en cero para empezar limpio. |
| 2 | `2_insertar_ventas_anomalas.bat` | Inserta ventas con anomalías en la BD **Firebird real** (entra a `_arranque` y corre `96_insertar_anomalias_firebird.bat`). El agente las detecta solo por watermark y las envía al central. |
| 3 | `3_consultar_bd.bat` | Muestra en PostgreSQL el estado de staging, alertas y ciclos del job (consulta directa para verificar). |
| 4 | `4_insertar_anulaciones_prueba.bat` | Inserta anulaciones nuevas (tabla `ANUL`) en Firebird; el agente las envía como la fuente "Anulaciones" y dispara la regla de exceso de anulaciones (alerta de Auditoría). |

> Requiere el contenedor Firebird arriba. Si no existe, créalo con
> `3-DIAGNOSTICO/restaurar_firebird.bat`.

---

## 3-DIAGNOSTICO — cuando algo falla

| Script | Qué hace |
|---|---|
| `estado_servicios.bat` | Lista los contenedores Docker de PetrolRíos, los procesos de la app (API/Agente/Monitor) y los puertos en escucha (5170/5173/5180/5190/5432/3051). El primer comando para saber "qué está vivo". |
| `reiniciar_api.bat` | Mata la API y la vuelve a arrancar en una ventana nueva (`dotnet run` del proyecto Api). Para recargar cambios del backend (no hay hot-reload). |
| `restaurar_firebird.bat` | Recrea el contenedor Firebird 3.0 y **restaura el backup real CONTACONSTANZA** (~1 min) y luego repara la autenticación. Úsalo si Firebird no existe o se corrompió. |
| `reparar_auth_firebird.bat` | Recrea el usuario SYSDBA del contenedor Firebird (Srp) y prueba la conexión TCP. Úsalo si el agente no autentica contra Firebird. |
| `verificar_fuentes_dinamicas.bat` (+ `.ps1`) | Doble check agente↔central de cada tabla/fuente configurable: cursor, estado, filas leídas/enviadas y último error. Para depurar la sincronización de fuentes dinámicas. |

---

## 4-PUBLICACION — generar los ejecutables de distribución

| Script | Qué hace |
|---|---|
| `publicar.bat` | **El principal.** Compila el frontend e integra la SPA en `wwwroot`, y publica los 3 ejecutables **self-contained** (sin necesidad de .NET instalado) en `dist\`: `PetrolRios-Servidor` (API + frontend en uno), `PetrolRios-Agente` (panel `:5180`) y `PetrolRios-Monitor` (`:5190`). Si Inno Setup 6 está instalado, además compila los `setup.exe`. |
| `instalar_agente_servicio.bat` | Instala el **agente** como servicio de **Windows** (arranca solo al prender la PC). |
| `instalar_agente_servicio.sh` | Instala el agente como servicio **systemd** en **Linux**. |
| `instalar_agente_servicio_macos.sh` | Instala el agente como servicio **launchd** en **macOS**. |
| `instalar_monitor_servicio.bat` | Instala el **Monitor** como servicio de Windows desde su carpeta publicada. |
| `instalador_servidor.iss` / `instalador_agente.iss` / `instalador_monitor.iss` | Guiones de **Inno Setup** para generar instaladores `.exe` independientes de cada subsistema (los usa `publicar.bat`). |
| `agente-version.example.json` | **Plantilla del manifiesto de actualización remota.** Se copia a `config/agente-version.json` junto al ejecutable del central y se editan `version`, `url` y `sha256`; los agentes lo consultan en `/api/v1/agente/version` y se auto-actualizan. (El monitor usa el equivalente en `/api/v1/monitor-estacion/version`.) |

---

## 5-DESARROLLO — verificación de código

| Script | Qué hace |
|---|---|
| `verificar_build_y_tests.bat` | Llama a `scripts/verificar-mejoras.bat`: restore + build en Release + todas las pruebas + chequeo de migraciones EF + lint y build del frontend, con log en `verificacion.log`. **El gate oficial antes de commitear.** |

---

> **Nota sobre `_arranque/`:** ya **no** es el panel de arranque (eso es esta carpeta). Solo conserva
> los recursos de la BD Firebird de demostración (el backup real y los scripts de restauración/inserción)
> que invocan `2-DEMO` y `3-DIAGNOSTICO`. Ver `_arranque/LEEME.md`.
