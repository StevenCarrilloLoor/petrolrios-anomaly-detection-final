# Arranque del demo — PetrolRíos

Scripts pensados para levantar el sistema completo en tu máquina (Windows) y mostrarlo al profesor.
**Ejecuta los `.bat` haciendo doble clic en File Explorer** (no en una terminal con tier de seguridad restringido).

## Orden recomendado

| # | Script | Propósito | Ventana que abre |
|---|--------|-----------|------------------|
| 0 | `00_verificar_entorno.bat` | Comprueba que `dotnet`, `node`, `npm`, `docker` están en PATH y Docker Desktop está corriendo. | una, se cierra al pulsar Enter |
| 1 | `01_levantar_postgres.bat` | `docker compose up -d` → PostgreSQL 16 en `localhost:5432`. | una, queda en pausa al final |
| 2 | `02_levantar_api.bat` | `dotnet restore` + `dotnet build` + `dotnet run` del API (`http://localhost:5170`). Ejecuta migraciones y seed. | una, **persistente** mientras la API corre |
| 3 | `03_levantar_frontend.bat` | `npm install` (si hace falta) + `npm run dev` → `http://localhost:5173`. | una, **persistente** mientras Vite corre |
| 4 | `04_seed_staging_demo.bat` *(opcional, recomendado para demo sin Firebird)* | Inyecta facturas, cierres y anulaciones sintéticas en `transacciones_staging` que disparan los 4 detectores. | una, se cierra al pulsar Enter |
| 5 | `05_firebird_demo.bat` *(opcional)* | Restaura `CONTACONSTANZA-20250609.FBK` (en `Programas\ContaGober1\DatosC`) a `_arranque\firebird_data\CONTAC.FDB` usando `gbak.exe` de la instalación local de Firebird 2.5. **Requiere el servicio Firebird corriendo en `localhost:3050`** (services.msc → "Firebird Server – DefaultInstance"). | una |
| 6 | `06_levantar_station_agent.bat` *(opcional, depende de #5)* | Worker .NET que conecta a `localhost:3050/CONTAC.FDB` en modo solo lectura, extrae transacciones nuevas (DCTO, DESP, TURN, TURN_DEPO, ANUL, CRED_CABE, TURN_TARJ) y postea lotes a `/api/v1/ingesta` con JWT del usuario `agent-est-001`. | una, persistente |

**Atajo:** `ARRANQUE_DEMO.bat` lanza 1, 2 y 3 secuencialmente en ventanas separadas.

## Endpoints clave

| Servicio | URL |
|----------|-----|
| Frontend (login) | http://localhost:5173 |
| API root | http://localhost:5170 |
| Swagger UI | http://localhost:5170/swagger |
| Hangfire Dashboard | http://localhost:5170/hangfire |
| SignalR Hub | ws://localhost:5170/hubs/alerts |

## Credenciales sembradas

| Rol | Email | Password |
|-----|-------|----------|
| Administrador | admin@petrolrios.com | Admin123! |
| Auditor (Station Agent EST-001..010) | agent-est-001@petrolrios.com … agent-est-010@petrolrios.com | Agent123! |

## Cómo demostrar las 4 categorías de detección al profesor

1. Ejecuta `ARRANQUE_DEMO.bat` y espera a que los 3 servicios queden levantados.
2. Abre el navegador en http://localhost:5173 y haz login con `admin@petrolrios.com`.
3. Ejecuta `04_seed_staging_demo.bat` para inyectar transacciones sintéticas.
4. Ve a http://localhost:5170/hangfire → *Recurring Jobs* → busca `anomaly-detection` → botón **Trigger now**.
5. Vuelve al frontend (`/alertas`) y verás aparecer alertas de **Cash Fraud**, **Invoice Anomaly**, **Payment Fraud** y **Compliance Violation** clasificadas por nivel de riesgo.

El job recurrente vuelve a correr cada 5 minutos automáticamente (`*/5 * * * *`), así que con SignalR abierto las nuevas alertas aparecen como toasts en tiempo real (CU-10).

## Detener todo

- En cada ventana de servicio: **Ctrl + C** y luego cerrarla.
- Para detener Postgres y Firebird:
  ```
  docker stop petrolrios-postgres petrolrios-firebird
  docker rm   petrolrios-postgres petrolrios-firebird
  ```

## Notas

- El API escucha en **`5170`** (perfil `http` de `launchSettings.json`), no en `5000` como menciona el README raíz.
- El `appsettings.json` por defecto del `StationAgent` apunta a `localhost:5000`; `06_levantar_station_agent.bat` lo sobreescribe vía variables de entorno (`Agent__ServerUrl=http://localhost:5170`).
- Las migraciones se ejecutan automáticamente al arrancar la API (`SeedData.InitializeAsync`).
- Si reseteas el seed sintético, el script `04` también hace `TRUNCATE alertas` para que la demo no acumule basura entre corridas.
