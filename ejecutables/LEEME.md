# Ejecutables del proyecto — guía de uso

Todos los scripts del proyecto, organizados por propósito y en orden de ejecución.
**Para el día a día solo necesitas la carpeta `1-INICIO`.**

## 1-INICIO — arranque y apagado (uso diario)

| Orden | Script | Qué hace |
|---|---|---|
| 1 | `INICIAR_TODO.bat` | **Plug and play:** arranca Docker (PostgreSQL + Firebird), la API, el frontend, el Station Agent y el Monitor de estación. |
| 2 | `INICIAR_MONITOR_ESTACION.bat` | Arranca únicamente el cliente local de solo lectura que muestra los problemas operativos de la estación. |
| 3 | `DETENER_TODO.bat` | Detiene la API, el agente, el monitor, el frontend y los contenedores. |

Tras `INICIAR_TODO`:
- Aplicación: http://localhost:5173 (`admin@petrolrios.com` / `Admin123!`)
- Panel del agente: http://localhost:5180
- Monitor de estación: http://localhost:5190
- Swagger: http://localhost:5170/swagger · Hangfire: http://localhost:5170/hangfire

## 2-DEMO — datos de demostración (para la sustentación)

| Orden | Script | Qué hace |
|---|---|---|
| 1 | `1_limpiar_bd.bat` | Vacía alertas/staging dejando estaciones, reglas y usuarios. |
| 2 | `2_insertar_ventas_anomalas.bat` | Inserta 12 ventas con anomalías en la BD Firebird REAL (el agente las detecta solo). |
| 3 | `3_consultar_bd.bat` | Muestra el estado de staging, alertas y ciclos directamente en PostgreSQL. |

Flujo de demo: limpiar → insertar → esperar al agente (≤60 s) → "Trigger now" en
Hangfire (o esperar 5 min) → ver las alertas en el dashboard.

## 3-DIAGNOSTICO — cuando algo falla

| Script | Qué hace |
|---|---|
| `estado_servicios.bat` | Lista contenedores Docker y procesos de la aplicación. |
| `reiniciar_api.bat` | Mata la API y la vuelve a compilar/arrancar. |
| `restaurar_firebird.bat` | Recrea el contenedor Firebird 3.0 y restaura el backup real CONTACONSTANZA (~1 min). |
| `reparar_auth_firebird.bat` | Re-crea el usuario SYSDBA del contenedor si el agente no autentica. |
| `verificar_fuentes_dinamicas.bat` | Muestra el doble check agente/central de cada tabla extra: cursor, estado, filas leídas/enviadas y último error. |

## 4-PUBLICACION — generar los .exe de distribución

| Script | Qué hace |
|---|---|
| `publicar.bat` | Genera el servidor, el agente y `dist\PetrolRios-Monitor\` como ejecutables self-contained. |
| `instalador_servidor.iss` / `instalador_agente.iss` / `instalador_monitor.iss` | Instaladores Inno Setup independientes para cada subsistema. |
| `instalar_monitor_servicio.bat` | Instala el Monitor como servicio de Windows desde su carpeta publicada. |

## 5-DESARROLLO — verificación de código

| Script | Qué hace |
|---|---|
| `verificar_build_y_tests.bat` | restore + build + tests + build del frontend, con log en `verificacion.log`. |

> Nota: la carpeta `_arranque/` se conserva con los guiones de presentación
> (GUION_*.md) y scripts históricos de las demos anteriores.
