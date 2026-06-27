
## 91. Consultas EN VIVO a Firebird: cola on-demand (agente) + pestaña "Consultas" + factura en ventana nueva

**Motivación.** La auditora quiere trabajar "como un ERP": desde una alerta o un buscador, **ver la factura
completa**, **buscar todas las facturas de un cliente/RUC por rango de fechas** y **abrir en ventana nueva**
para comparar. Esos datos viven en la Firebird de cada estación, no en el central — y el central **no puede
conectarse al agente** (los agentes están detrás de NAT y son ellos quienes llaman al central).

**Qué se hizo — cola de consulta NAT-friendly (espejo de `ISolicitudesEsquema`).**

- **Central encola, el agente recoge en su heartbeat.** `IConsultasFirebird` (en memoria, singleton) guarda
  las consultas pendientes por estación. `ConsultasController`: `POST /api/v1/consultas` (encola y devuelve
  un id), `GET /api/v1/consultas/{id}` (la interfaz sondea el estado/resultado) y `POST .../{id}/resultado`
  (el agente reporta). El **heartbeat** (ahora cada 1 s) entrega al agente sus consultas pendientes.
- **Agente: consulta SOLO LECTURA.** En cada heartbeat el agente recoge las consultas y corre
  `FirebirdExtractor.ConsultarDocumentosAsync` — un `SELECT` parametrizado sobre **`DCTO`** con filtros: tipo
  de documento + rango de fechas + un código que coincide (`CONTAINING`) con **RUC, placa, cliente o número
  de documento** — y devuelve las filas. La conexión es `ReadOnly`; nunca modifica la base.
- **Pestaña "Consultas" (frontend).** Busca documentos en vivo por estación + tipo + fechas + código; tabla de
  resultados (n.º, fecha, tipo, cliente, RUC, placa, turno, total). Buscar por RUC = **el reporte de todas las
  facturas de ese cliente** por rango de fechas que pidió la auditora.
- **Factura completa en VENTANA NUEVA.** Cada fila abre `/consultas/factura` (vista standalone, sin barra
  lateral) con todos los campos del documento y un botón **Imprimir** — para comparar lado a lado, como pidió.

**🐞 Bug cazado por el QA E2E en vivo (lo que el gate no veía).** El agente devolvía las columnas en
**MAYÚSCULAS** (`NUMERODOCUMENTO`, `TOTALNETO`…) porque Firebird pasa a mayúsculas los **alias sin comillas**,
pero el frontend lee `NumeroDocumento`/`TotalNeto` → la tabla saldría **vacía**. **Fix:** alias entre comillas
(`AS "NumeroDocumento"`) para preservar el PascalCase (commit `699dbb1`). La prueba unitaria usa un JSON fijo,
así que el gate no lo veía; lo destapó la **consulta real**.

**Diseño honesto (sin inventar).** El **detalle de líneas de despacho (`DESP`)** dentro de la factura NO se
incluye todavía: el enlace `DCTO.NDO_DCTO → DESP.NUM_DESP` es 1:1 y la consolidación es no trivial (marcado ⚠️
en el análisis, entrelazado con el rework de "Despacho no facturado", #136). Se entrega lo **fiable** (la
cabecera completa + el buscador) y las líneas quedan como refinamiento verificado contra datos.

**Verificación.** Gate oficial en la PC de Steven (3 etapas): **build Release 0/0**, **pruebas 334 / 0 skipped**
(Domain 40, Monitor 2, Detectors 193, **Api 99 (+5)** — `ConsultasFirebirdTests`), **EF sin migraciones
pendientes** (cola en memoria), **lint + build de frontend OK**. **QA E2E en vivo** (iniciar-todo, agente local
EST-001 + Firebird Docker): se encoló una consulta `FV` con código `1790` y el agente **devolvió 50 facturas
FV reales** (las sintéticas del turno 990001 + históricas 2025, todas con RUC que contiene "1790") — pipeline
central→agente→Firebird→resultado **confirmado**. La pestaña **Consultas** carga y arma la búsqueda.

Commits: `8d0e651` (cola agente+central), `7166e40` (pestaña Consultas + factura), `699dbb1` (fix casing).
*(Pendiente: enlazar la factura desde el detalle de alerta; líneas DESP; pulido UI.)*

---
