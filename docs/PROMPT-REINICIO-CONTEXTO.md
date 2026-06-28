# Prompt de reinicio de contexto — Proyecto PetrolRíos

> **Cómo usarlo:** cada vez que la conversación se compacte (o empieces una nueva),
> pégame este documento completo como primer mensaje. Reconstruye mis instructivos,
> el flujo de trabajo y el contexto del código para que no perdamos el hilo.

---

## 1. Quién eres aquí y cómo trabajamos (mi mandato)

Eres mi compañero de ingeniería en mi **proyecto de tesis** PetrolRíos. Trabajamos así, sin excepción:

- **No espero menos que la perfección.** Calidad de tesis: código limpio, probado y verificado.
- **Trabaja en etapas.** Después de **cada cambio grande**:
  1. Compruébalo en el **código** (compila + corre las pruebas; añade pruebas nuevas para lo nuevo).
  2. Compruébalo en la **interfaz** usando **Chrome** (prueba botones, flujos, inserciones reales).
  3. Haz un **commit** con descripción detallada.
- **Hazlo todo seguido**, en secuencia, **sin preguntarme si avanzas** entre etapas. Solo
  detente a preguntarme cuando yo lo pedí explícitamente (p. ej. antes de un refactor grande).
- **Agrupa cambios pequeños** para no repetir el ciclo de verificación por cada uno.
- Si encuentras **bugs**, arréglalos.
- **Actualiza SIEMPRE la documentación de contexto legado con CADA cambio** (no solo al final):
  `CAMBIOS.md` (bitácora con sección numerada + commits + verificación), `docs/PENDIENTES.md`
  (marcar hecho / agregar lo nuevo + fecha) y este mismo `docs/PROMPT-REINICIO-CONTEXTO.md`
  (estado actual, sección 6). Así, si el contexto se compacta, el siguiente arranque no pierde el hilo.

## 2. Primero, reconstruye el contexto (ANTES de cualquier cambio)

1. **Pide todos los permisos** que necesites (Chrome, control de la computadora, acceso a la carpeta).
2. Lee **`CLAUDE.md`** (reglas del repo; stack obligatorio; tablas reales; lo que NO debes hacer).
3. Lee **`CAMBIOS.md`** completo (bitácora de todo lo que ya hicimos, etapa por etapa).
4. Lee **`docs/contac-schema.sql`** = **fuente de verdad** de los nombres reales de tablas/columnas
   Firebird (Contaplus). El documento transaccional real es **`DCTO`** (no `FACT`). Verifica
   siempre las columnas ahí antes de escribir SQL o lógica de detector.
5. **`docs/tesis.md` está DESACTUALIZADA**: úsala solo como referencia, **NO la edites**. El
   **CÓDIGO es la fuente de verdad**, mi proyecto está mucho más avanzado que la tesis.
6. Revisa el historial: `git log --oneline -40`.
7. Lee el **código relevante a la tarea, archivo por archivo**, sin inventar ni asumir nombres.

## 3. Reglas de entorno y flujo de trabajo (críticas — si las ignoras, fallarás)

- **El sandbox Linux NO compila .NET**, y su **montaje sirve lecturas viejas/truncadas**. Para el
  estado real usa **SIEMPRE** las herramientas `Read`/`Write`/`Edit` (no `cat` del sandbox) y
  compila/pruebas/commits en **Windows**.
- **Build / tests / commits en Windows:** escribe un `.bat` y ejecútalo desde la **barra de
  direcciones del Explorador de archivos** (clic en la barra → escribe la ruta del `.bat` → Enter);
  luego **lee el `.log`** de salida con la herramienta `Read`. (Si el clic no entra en modo edición,
  clic en una zona vacía a la derecha de la barra y reintenta.)
- **Gate de verificación oficial:** `ejecutables/4-VERIFICACION-Y-PRUEBAS/verificar-todo-gate-oficial.bat`
  (build Release + todos los tests + chequeo de migración EF + lint frontend + build frontend). Úsalo antes de commitear.
  *(TODOS los scripts viven ahora bajo `ejecutables/`, ordenados por tipo y con un resumen al inicio; ver `ejecutables/LEEME.md`.)*
- **ANTES de compilar, DETÉN los servicios en ejecución** (bloquean los binarios y el build falla
  con `MSB3027`):
  `taskkill /F /IM PetrolRios.Api.exe` · `PetrolRios.StationAgent.exe` · `PetrolRios.StationMonitor.exe`.
- **Genera migraciones EF solo con build fresco** (nunca `--no-build` sobre binarios bloqueados/viejos:
  salen **vacías** y rompen el arranque por desajuste modelo↔esquema). El nombre del archivo de
  migración lleva timestamp nuevo: úsalo exacto al hacer `git add`.
- **Para reiniciar TODO corre SIEMPRE primero `ejecutables/1-INICIAR-Y-DETENER/detener-todo-el-sistema.bat` y
  LUEGO `iniciar-todo-el-sistema.bat`.** (Aprendido a la mala: `iniciar-todo` NO mata el agente/API/monitor
  previos, así que llamarlo varias veces APILA instancias; con varios agentes EST-001 vivos, una consulta en
  vivo la "toma" una instancia de la cola y, si está colgada, expira aunque haya otra sana → Consultas/Factura
  dan "la estación no respondió a tiempo". `detener-todo` hace el `taskkill` de las 3 + baja Docker, dejando UNA
  sola instancia limpia.)
- **Reinicia todo el sistema** con `ejecutables/1-INICIAR-Y-DETENER/iniciar-todo-el-sistema.bat` (Docker, PostgreSQL,
  Firebird, API, agente, monitor, frontend). Para solo el API: `ejecutables/1-INICIAR-Y-DETENER/reiniciar-solo-la-api.bat`.
- **Commits con mi identidad:**
  `git -c user.name="StevenCarrilloLoor" -c user.email="stevencarrilloloor@gmail.com" commit`.
  Haz `git add` **por ruta, solo de tus archivos**. **NUNCA** toques mis cambios sin commitear.
- **Limpia** los `.bat`/`.log` temporales que crees.
- Detalle conocido: los screenshots por CDP a veces dan timeout → **reintenta** (sale al 2º intento).
- **Probar el agente = INSERTAR EN FIREBIRD con datos REALES (nunca en Postgres).** La forma fiable de
  verificar el agente end-to-end es **insertar transacciones directo en la Firebird de la estación**
  (`type x.sql | docker exec -i petrolrios-firebird /usr/local/firebird/bin/isql -user SYSDBA -password
  masterkey /firebird/data/CONTAC.FDB`, con `FEC_DCTO/FIN_DESP = CURRENT_TIMESTAMP` para que la marca de
  agua las tome) y confirmar que (a) el agente las **extrae** (consola "Extraídas N"), (b) llegan al central
  (Datos recibidos / alertas), (c) la consulta en vivo las muestra. **ANTES de insertar, LEER el Firebird
  real** (mismo `isql`, p. ej. helper `_fbq.bat`) para saber qué hay y cómo viene, y **usar datos reales del
  catálogo**: clientes `ZZ0000xxx` (tienen `NOM_CLIE`) en vez de códigos sintéticos `CLI777` (no están en
  CLIE → no resuelven nombre); despachadores `VEND` 001–011 con nombre (004=JORGE MENDOZA, 010=CARLA
  VALAREZO…). **Verificar SIEMPRE columnas/tipos en `docs/contac-schema.sql` antes del INSERT** (regla de
  oro). Inserciones demo en `ejecutables/2-BASE-DE-DATOS-Y-DEMO/`; usar PK sintéticas `99xxxxx` para no chocar.

## 4. El sistema (qué es)

PetrolRíos detecta **anomalías transaccionales** en ~13–15 mil transacciones diarias de 10 estaciones
de servicio (cada una con un Firebird `CONTAC.FDB` en **solo lectura**). Tres aplicaciones:

- **Central** = API ASP.NET Core 9 en `:5170` + frontend React/Vite en `:5173` + PostgreSQL.
  Hangfire corre los detectores **cada minuto** en dev. SignalR para alertas en vivo.
- **Station Agent** (`:5180`): extrae de Firebird por marca de agua y envía al central; panel propio.
- **Station Monitor** (`:5190`): vista operativa de solo lectura por estación.

**5 detectores** (Strategy Pattern): `CashFraud`, `InvoiceAnomaly`, `PaymentFraud`,
`ComplianceViolation`, `CustomRule`. **Dos carriles de alerta** (enum `AmbitoAlerta`):
**Operativa** (error de estación → avisa al administrador, pestaña "Problemas de estación") y
**Auditoría** (fraude → bandeja del central). El carril de cada regla del motor **ya es editable**
(propiedad `Ambito` en `ReglaDeteccion`; los detectores la respetan).

**Roles (RBAC):** `Administrador`, `Supervisor`, `Auditor` (personas que usan la app central) y
**`Agente`** (cuenta de servicio de cada estación — usuarios `agent-*` que SOLO envían datos; la
política "Central" los excluye, no pueden entrar al dashboard/bandeja). **Escalable a >10 estaciones:**
el Administrador da de alta una estación + su usuario-agente desde "Nueva estación" (modal con
credenciales) o desde "Nuevo Usuario" (código de estación nuevo). El agente corre v2.3.0.

## 5. Mis credenciales y los permisos que te doy

- **admin@petrolrios.com / Oportunidad1234** (Administrador).
- **stevencarrilloloor@gmail.com / Oportunidad1234** (mi cuenta).
- Te **autorizo** a usar mi computadora, Chrome y a **ver mi Gmail abierto** para pruebas E2E
  (p. ej. leer los correos de recuperación/desbloqueo).
- Recuerda tus reglas de seguridad: **no escribes contraseñas en campos** → haz login por API
  (`POST /api/v1/auth/login` y guarda el token en `localStorage`).

## 6. Estado actual del trabajo (ACTUALÍZAME al avanzar)

**RONDA G — PRODUCCIÓN / calibración con datos REALES (28-jun-2026). EN CURSO.** Steven conectó la estación
**REAL SanPio (EST-015)** y re-sincronizó **un mes** (~19.8k transacciones reales). Se recorrió TODO en vivo y se
documentaron **19 hallazgos** en `HALLAZGOS-PRODUCCION.md` (outputs). Top: (1) `TNI_DCTO`=0 en datos reales (rompe
$0.00 + scoring plano + reglas con umbral de monto); (2) regla "Despachos rápidos" = 6.080 alertas — patrón real
(misma placa repetida en minutos, confirmado con datos reales) pero hay que agregar/keyear por placa + ámbito
Ambos; (3) brechas de extracción (SanPio no envía TURN_DEPO/TURN_TARJ/CRED_CABE; despachador `DD…` sin nombre);
(4) encoding Ñ ("MUÃ?OZ"). **Detalle importante de infra:** `iniciar-todo` arranca SOLO el agente **EST-001** vía
`dotnet run` (toma código nuevo); **SanPio (EST-015) es un agente SEPARADO de Steven** (binario propio) → cuando un
cambio toca el AGENTE, **Steven reinicia/actualiza el agente de SanPio** (avísale). La **API central** sí se
reconstruye con `iniciar-todo` (los cambios de detectores/DTO quedan vivos al instante).
- **G1 HECHA (`737043e`, CAMBIOS §101):** **total real cuando `TNI_DCTO`=0** → `FacturaDto.TotalNeto` getter con
  backing field cae a `TotalSinIva+Iva` (recalcula existentes + nuevos, sin re-extraer; respeta TNI del demo) +
  `ConsultarDocumentos` con `CASE`. Arregla $0.00, scoring plano y reglas con umbral $. Gate verde **337** (+3
  `FacturaDtoTests`). Central/detector vivo al reconstruir API; la pantalla de factura/consulta requiere que el
  agente tome el build (es cambio de agente → reiniciar el de SanPio).
- **Pendiente G2** (despachos rápidos: placa + agregación + Ambos), **G3** (nombre despachador + encoding Ñ).

---

**NUEVA RONDA ERP/UX-2 (27-jun-2026, pedido de Steven con la entrevista de auditoría + 8 screenshots como base)
— ✅ A–F COMPLETA (gate verde 334 pruebas + QA en vivo en Chrome con datos REALES del backup en cada etapa).**
Base: la entrevista (`docs/Juan Valdez - Transcripcion ES.txt`).
- **A+B (commit `3050c94`, CAMBIOS §96):** A) `ConsultasPage` con **deep-link autobuscable**
  (`?est&codigo&tipo&desde&hasta` → busca sola), **fecha visible** en tema oscuro (`[color-scheme:dark]`),
  **Imprimir** lo filtrado y **celdas relacionables** (cliente/RUC/placa/despachador → consulta relacionada en
  ventana nueva). B) `DetalleAlertaPage`: los chips de evidencia de ENTIDAD abren la **consulta RELACIONADA en
  vivo en ventana nueva** (ya **NO** `/alertas?buscar`); el nº de factura sigue abriendo la **factura completa**.
  QA: alerta #80 → 13 enlaces de factura, 0 a `/alertas?buscar`, 7 RUC relacionables; deep-link por RUC → 6 docs.
- **C (commit `6e8da8d`, CAMBIOS §97):** **factura enriquecida tipo ERP** — nombre de cliente (CLIE) +
  despachador (VEND) + chofer/autorización SRI/guía/observaciones/subtotal, tipo de doc y forma de pago legibles.
  **Probada con SIMULACIÓN REAL en Firebird:** se insertó la factura `001-001-009900070` a cliente real
  `ZZ0000176` + despachador `010` → el AGENTE la extrajo por marca de agua (Datos recibidos #3393/#3394) y la
  factura la mostró enriquecida. Fix de rendimiento: se quitó `NOM_CLIE` del WHERE (forzaba join por fila → timeout).
- **D (commit `ab17578`, CAMBIOS §98):** **auditoría del agente** confirmada (reactivo: heartbeat 1 s config en
  vivo + store-and-forward; escalable: watermark por estación + fuentes configurables + aislamiento; dinámico:
  auto-extrae + auto-relaciona + reporta esquema) + **consultar CUALQUIER tabla auto-estructurada**: agente
  `ConsultarTablaGenericaAsync` (`SELECT FIRST n *` con **lista blanca anti-inyección**) + página "Explorar
  tabla" (`/consultas/tabla`) con columnas dinámicas. QA: VEND→6 columnas+11 filas; tabla inexistente rechazada.
- **E (commit `62eb3bf`, CAMBIOS §99):** **dashboard rediseñado como centro de mando ACCIONABLE** (investigué
  referencias de dashboards de fraude/SOC) — feed "Atención inmediata" (alertas recientes clicables = triage),
  KPIs con **drill-down** (→ bandeja filtrada), tendencia con **periodo 7/14/30/90 + delta**, **drill-down en
  gráficos**, filas de empleados clicables, métricas con **semántica de color**; `AlertasPage` lee filtros
  `?nivel/?estado/?tipo/?estacionId` de la URL. QA: feed con 7 alertas, 5 KPIs-enlace, sin errores.
- **F (commit `c426d58`, CAMBIOS §100):** **cuadre de liquidaciones** (pedido directo de la auditora) — para
  las liquidaciones de un periodo, **qué facturas componen cada una** (`LIQU.NUM_TURN ↔ DCTO.NUM_TURN`,
  `TIP_DCTO='FV'`, total real `TSI_DCTO+IVA_DCTO` porque `TNI_DCTO` viene en 0 en el backup). Agente
  `ConsultarLiquidacionesAsync` + Worker enruta `LIQUIDACIONES`; página `/consultas/liquidaciones` agrupa las
  facturas bajo cada liquidación (faltante/sobrante + total) + imprimir + enlace desde Consultas. QA en vivo con
  backup REAL: EST-001 → 4 liquidaciones con sus facturas y totales reales (1000 celdas con valores, ya no $0.00).

Permisos de Windows/Chrome concedidos; sistema corriendo vía `iniciar-todo-el-sistema.bat` (siempre `detener-todo`
antes). Regla de seguridad firme: login por API (no tecleo contraseñas). **La ronda A–F está cerrada.**

**CIERRE DE PENDIENTES (27-jun-2026, §94, commits `25334b6`/`ac371aa`/`f239a3a`, gate verde 334 tests + QA en
vivo):** (a) **`Fuente` extendida a las 4 reglas de factura restantes** (FueraHorario, FechaFueraDeRango,
ReversionTardia, DespachosRapidos) → su evidencia hereda RUC/nº doc (enlace)/placa/cliente/turno/forma de
pago/monto; las reglas **agregadas** (por empleado/vendedor/placa) y las de **crédito/turno/tarjeta NO se
tocan** (no tienen documento único; sería redundante/engañoso). (b) **«Despacho no facturado» DESACTIVADA por
defecto (#136)**: `FAC_DESP` es la FORMA DE PAGO del despacho, no un flag 0/1 de facturado (lo confirma el
stored procedure del esquema) → la heurística daba falsos positivos; off por `SeedData` (Activa=false, espejo
del patrón de FueraHorario); QA en vivo: toggle inactivo. La detección correcta exige el cruce
`DESP.NUM_DESP↔DCTO.NDO_DCTO` con staging+gracia (futuro). (c) **Despachador (COD_VEND) buscable en Consultas +
Nº de despacho (NDO_DCTO) en la factura** («Despacho (origen)»); QA en vivo: 200 docs por «005»; factura real
`006102000024646` → despacho **39904**. (d) **Detalle de la línea de surtido (DESP) en la factura
(`a173b5a`, §95)**: consulta DESP aislada por `NUM_DESP = CAST(NDO_DCTO)` (discriminador `Tabla` en la cola;
`ConsultarDespachosAsync` en el agente; `consultarDespachos` + tabla en la FacturaPage). QA en vivo: la
factura `006102000024646` muestra su línea (manguera 02, 1.01 gal @ $2.47 = $2.50). **El round ERP/UX está
COMPLETO, sin pendientes.**

**Ronda ERP/UX (27-jun-2026, EN PROGRESO por etapas — Steven pidió 5 cosas, "tú decides el orden, todo seguido"):**
(1) pulir UI del detalle de alerta; (2) **hipervínculos de verdad**: nº de documento → factura COMPLETA
(cabecera DCTO + líneas DESP) y reportes por cliente/RUC y despachador (rango de fechas, **abrir en ventana
nueva**, imprimir), datos **en vivo de Firebird** (nuevo endpoint on-demand en el agente) + **pestaña
"Consultas"** (buscar por tipo de doc + fechas + código/RUC/placa/nombre); (3) agente cada **1 s**;
(4) **refresco configurable** en todas las pantallas; (5) **enriquecer la evidencia de TODAS las reglas**
(RUC, nº de doc corto, placa, vendedor, turno…) de forma genérica + creador de reglas + hipervínculos
automáticos. Base: la entrevista (`docs/Juan Valdez - Transcripcion ES.txt`) y `ANALISIS-CONTAPLUS…` §4.
- **Etapa 1 HECHA (CAMBIOS §89, commit `626c3e0`, gate verde):** tasa de refresco **global y configurable**
  (`OperacionConfig.RefrescoSegundos`, default 1 s, acotada 1 s…1 h; `GET /api/v1/refresco` para todo rol;
  selector en Ajustes; `RefrescoContext`/`useRefrescoMs` en TODAS las pantallas) + **agente cada 1 s** por
  defecto (clamp mín. 5→1). +8 pruebas → Api 94. *(Pendiente: QA en vivo al final, junto con las demás etapas.)*
- **Etapa 2 HECHA (CAMBIOS §90, commit `1114d65`, gate verde):** **evidencia identificable automática** en las
  alertas. `DetectedAnomaly.Fuente` (objeto de origen) + `EvidenciaEnriquecida` reflejado por
  `RuleBasedDetector` → toda anomalía trae RUC, nº de documento, placa, cliente, turno, fecha, monto, forma de
  pago, sin pisar lo de la regla. `Fuente` fijada en 10 reglas (incl. diferencia de efectivo, la del caso de la
  auditora). Frontend etiqueta/enlaza las claves nuevas. +4 pruebas → Detectors 193. *(Falta: extender `Fuente`
  al resto de reglas built-in + a las personalizadas.)*
- **Etapas 3–4 HECHAS + VERIFICADAS EN VIVO (CAMBIOS §91–92, commits `8d0e651` + `7166e40` + `699dbb1` + `5f149a9`,
  gate verde 334 tests + QA en Chrome):** **consulta en vivo a Firebird**. Cola `IConsultasFirebird` (en memoria,
  espejo de `ISolicitudesEsquema`): `ConsultasController` (POST encolar / GET sondear / POST resultado del agente);
  el heartbeat (1 s) entrega las consultas pendientes; el agente corre `FirebirdExtractor.ConsultarDocumentosAsync`
  (SELECT SOLO LECTURA sobre DCTO por tipo+fechas+código que CONTAINING RUC/placa/cliente/nº) y devuelve. Frontend:
  **pestaña "Consultas"** + **`/consultas/factura`** (factura completa en **ventana nueva**, imprimir). +5 pruebas
  → Api 99. **QA en vivo en Chrome (§92):** buscar por RUC largo `1301790737` → **21 facturas FV reales** en la
  tabla + **factura completa en ventana nueva** (Nº `006103000014084`, FV, cliente ZZ0067712, RUC, placa GPO0947,
  despachador DD0000006, turno 161, base $6.09 / IVA $0.91). 🐞 **Dos fixes cazados por el QA real (el gate no los
  veía, la prueba del agente usa JSON fijo):** `699dbb1` alias del SELECT entre comillas (Firebird preserva
  PascalCase — confirmado en vivo tras recompilar el agente; el MAYÚSCULAS previo era binario viejo en memoria);
  `5f149a9` `CAST(col AS VARCHAR(60))` antes de `CONTAINING` (evita `string right truncation` cuando el código es
  más largo que una columna estrecha como `PLA_DCTO` CHAR(8)) + `normalizarDoc` case-insensitive en el frontend.
- **Etapa 5 HECHA + VERIFICADA EN VIVO (CAMBIOS §93, commit `e0a1f4f`, gate verde 334 tests):** el **detalle de
  alerta enlaza con la factura COMPLETA en vivo**. `AlertaResponse` lleva ahora `EstacionCodigo` (mapeo en
  `AlertaService`: el diccionario de estaciones pasó a `id→(Nombre,Codigo)`); en la evidencia, las claves de
  número de factura (`NumeroDocumento`/`NumerosFactura`) muestran una pastilla **"factura"** que abre
  `/consultas/factura?est=CODIGO&num=NUMERO` en ventana nueva. QA en vivo: alerta #80 → factura
  `001-001-009900060` completa (cliente CLI060, RUC 1790000000001, turno 990001, total $500). **→ Las 5 etapas
  de la ronda ERP/UX quedan COMPLETAS.** *(Refinamientos opcionales: líneas DESP en la factura, extender
  `Fuente` al resto de reglas built-in, enlace despachador→sus despachos.)*

---

**Ronda previa — Mejoras de auditoría (27-jun-2026) — LAS 4 HECHAS (#1 placa, #2 buscador, #3 cuadre, #4 dashboard):**
Steven seleccionó las 4 mejoras del backlog de auditoría (§82) y pidió implementarlas todas:
(1) 🔴 placa reutilizada N+/día, (2) 🟠 UX de alertas (hipervínculos + copiar + nº factura + buscador),
(3) 🔴 factura fuera de liquidación, (4) 🟠 dashboard por estación + reportería. Método de trabajo: scripts
`.bat` lanzados desde el **Explorador** (full tier) — el portapapeles ya está concedido, así que el pegado
en la barra de direcciones funciona (Terminal/VS Code son "clic", no se puede teclear en ellos).
- **#1 HECHO (CAMBIOS §83, commit `99d3330`):** `PlacaReutilizadaRule` (InvoiceAnomaly, carril Auditoría).
  Agrupa facturas por **(placa, día)** y alerta si una placa supera el umbral en el día. Excluye placa
  genérica `ZZZ999949` y vacías. **Programada DIARIA** (Calendario 23:55 EC; el conteo es por jornada, así
  que NO corre "cada ciclo": el Pass B la evalúa una vez/día sobre la ventana del día, `DiasVentana=1`, sin
  duplicados). Umbral `PlacaReutilizadaDiaUmbral` (default **5**; auditora sugiere **2**), sembrado idempotente
  en `SeedData`. +7 pruebas → **Detectors 189**. *Pendiente: QA en vivo en Chrome (poner ProximaEjecucion en
  pasado o bajar el umbral para dispararla en una corrida).*
- **#2 HECHO (CAMBIOS §84, commit `e5d7bdb`):** UX de alertas. **Backend:** `?buscar=` en
  `AlertaRepository.ApplyFilters` (LIKE sobre Descripción/Referencia/MetadataJson/EmpleadoCodigo) hilado
  por controller→service→repo (parámetro opcional). **Frontend:** caja de búsqueda con rebote 350 ms en
  `AlertasPage` (placa/RUC/nº factura/cliente/código) + `?buscar=` en URL; en el detalle, evidencia con
  **pastillas-enlace** (placa/nº factura/cliente → `/alertas?buscar=`) + **botón copiar** + **abrir en
  ventana nueva** + etiquetas de la regla de placa. Gate verde (build/tests/eslint/vite). *Pendiente: QA Chrome.*
- **#4 HECHO (CAMBIOS §85, commit `594f186`):** dashboard por estación. **Backend:** `AlertasAuditoria`
  ahora es método `AlertasAuditoria(int? estacionId)`; `?estacionId=` opcional en KPIs/tendencia/tipo/
  nivel/métricas/top-empleados (la comparativa "por estación" queda global). **Frontend:** selector de
  estación en `DashboardPage` (acota todo salvo la comparativa, `keepPreviousData` para no parpadear) +
  botón **Imprimir/PDF** (`window.print()` + `print:hidden`). Gate verde. *Pendiente: QA Chrome.*
- **#3 HECHO (CAMBIOS §88, commit `43fe272`, gate verde + QA E2E en vivo):** factura fuera de liquidación
  (cuadre de turno). El agente lee `LIQU` (`LiquidacionDto`, solo lectura, tipo "Liquidacion").
  `CuadreLiquidacionService` (Infrastructure) marca **turnos CERRADOS** (`EST_TURN='1'`) con facturas FV,
  fuera del periodo de gracia (umbral `FacturaSinLiquidacionHorasUmbral`, default 12 h), que **no** están en
  `LIQU`; alerta **idempotente** por turno (`SINLIQU-{est}-{turno}`) sobre el **staging acumulado** (30 d).
  **No es detector de ventana** (se gestiona aparte del gate, corre 1×/día Calendario 23:50, avanza su
  agenda); es correcto porque `FEC_LIQU ≥ FFI_TURN` → cierre y `LIQU` viajan juntos. +9 pruebas puras
  (`CuadreLiquidacionServiceTests`) → **Api 86**; fix ctor en `AnomalyDetectionJobE2ETests`. **QA en vivo:**
  cerré el turno de prueba 990001 en la Firebird de demo → el agente lo re-extrajo (rebobinando la marca) →
  **alerta #80** "13 factura(s) por $1674,20 fuera del cuadre", Alto (71.7), evidencia OK. *(Futuro opcional:
  persistir `LIQU` en tabla del central; hoy no hace falta por el orden temporal.)*
- **QA en vivo en Chrome — HECHO (CAMBIOS §86):** se reinició API (`reiniciar-solo-la-api.bat`, siembra la
  regla) + Vite dev (`reiniciar-solo-el-frontend.bat`, :5173) y se verificó: **#1** la regla placa aparece
  con cadencia "Todos los días 23:55" + umbral 5; **#2** el buscador filtra ("CZ0060636"→#60) y el detalle
  muestra chips/copiar/ventana-nueva; **#4** el dashboard tiene selector de estación + Imprimir/PDF.
- **🐞 Fix en vivo (`c011374`):** el buscador daba **500** porque `Alerta.MetadataJson` era **jsonb** y
  `lower()` sobre jsonb no traduce (los tests de integración estaban *skipped* → el gate no lo vio).
- **Buscador COMPLETO (`69ac236`, CAMBIOS §87, QA en vivo OK):** `MetadataJson` jsonb→**text** (migración
  `BuscarEvidenciaAlertaTexto`, lossless) → la **evidencia es buscable** (placa/RUC/cliente/nº factura);
  +`Rucs` en la regla de placa; y **búsqueda por NOMBRE** resolviendo nombre→códigos contra el catálogo
  `Empleados` (`AlertaService` pasa `codigosPorNombre` al repo). Verificado: "MENDOZA"→8 (JORGE MENDOZA),
  "032101000020765"→#60 (solo en evidencia). El buscador cubre placa/RUC/nº factura/cliente/código/nombre.
- **Lectura COMPLETA de `ejecutables`** (las 6 carpetas), a pedido de Steven. Scripts clave: reiniciar
  api/front (1), `docker exec petrolrios-firebird isql` (2, así se verificó #3), gate oficial (4),
  `publicar-solo-el-agente-multiplataforma.bat` (5, para el republish de #3).

**Ronda — Frecuencia/calendario por regla (26-jun-2026) — ✅ COMPLETO (Etapas 1-5):**
- Investigación (pedida): **Quartz** = SimpleTrigger (intervalo) vs CronTrigger (calendario) → diseño de
  **doble modo** validado. **Cronos** (NuGet) calcula próximas fechas con `L`=último día, bisiestos, TZ →
  se usa para el modo Calendario (no reinventar la matemática).
- **Etapa 1 (hecha):** `ProgramacionEjecucion` + `CalculadoraProgramacion` en `Application/Programacion`
  (Cronos 0.8.4). Intervalo seg/min/h/d/sem/mes + Calendario diario/semanal/mensual (día-D, "último día").
  15 tests. *(CAMBIOS §75)*
- **Etapa 2 (hecha):** columnas `ProgramacionJson`/`ProximaEjecucion`/`UltimaEjecucion` en `ReglaDeteccion`
  + `ReglaPersonalizada` + migración `20260626145512_ProgramacionDeRegla` (default '' = cada ciclo).
  *(CAMBIOS §76)*
- **Etapa 3 (hecha + gate verde):** `AnomalyDetectionJob` en **dos pasadas**. **Pasada A** (incremental):
  reglas "cada ciclo" sobre el lote no procesado, igual que antes; las programadas del motor se presentan
  `Activa=false` (las reglas se cargan **sin tracking** para alternar `Activa` sin tocar la BD), las custom
  se filtran. **Pasada B** (ventana): cada regla programada a la que le toca se evalúa sola sobre
  `FechaOriginal ∈ (UltimaEjecucion ?? ahora−díasVentana, ahora]` (sin marcar `Procesada`). Ventana **no
  solapada** entre corridas → **sin alertas duplicadas** (no hizo falta idempotencia nueva ni tocar Alerta).
  Avance de `ProximaEjecucion`/`UltimaEjecucion` una vez por ciclo (zona EC UTC-5 vía Cronos, normalizado a
  UTC). **Con todo en "cada ciclo" = idéntico al comportamiento previo** (feature opt-in). +2 pruebas E2E →
  Api 77. *(CAMBIOS §77)*
- **Etapa 4 (hecha + gate verde):** `ProgramacionDto` (enums como texto + `TryConvertir` que valida listas
  cerradas y rangos → 400 limpio, nunca 500). Respuestas de **motor** (`ReglaDeteccionResponse`/`ReglaService`)
  y **personalizadas** (`ReglaPersonalizadaResponse`/`ReglasPersonalizadasController`) devuelven `Programacion`
  + `ProximaEjecucion` + `UltimaEjecucion`; `PUT`/`POST` aceptan `Programacion` y al cambiarla reinician
  `ProximaEjecucion=null` (el job ancla). En custom, `null` en update conserva la previa. +12 pruebas
  (`ProgramacionDtoTests` → Detectors 177). *(CAMBIOS §78)*
- **Etapa 5 (hecha + gate verde):** `<ProgramacionSelector>` reutilizable (cada ciclo / intervalo
  seg-min-h-d-sem-mes / calendario diario-semanal-mensual día-D o "último día" + hora, con vista previa en
  vivo); constantes/helpers en `lib/programacion.ts` (separados para no romper Fast Refresh). Integrado en
  reglas del **motor** (`ReglasPage.tsx`: chip + panel inline por fila) y **personalizadas**
  (`ReglasPersonalizadasSection.tsx`: sección del formulario + cadencia/próxima en la lista; el toggle y el
  cambio de fuente conservan la programación). Tipos TS espejo en `types/regla.ts` + `types/reglaPersonalizada.ts`.
  eslint+vite OK. *(CAMBIOS §79)*
- **Pulido UX (feedback Steven, CAMBIOS §80):** el selector de calendario ahora es **grilla 1–31** + pastillas
  de día de semana + **hora nativa** (`<input type="time">`); y en "información a mostrar en la alerta" hay un
  **filtro por tabla** (📂) para distinguir campos del mismo concepto que el autoenlace expone desde tablas
  distintas. Autoenlazador (`DescubridorRelacionesService`) auditado = correcto (no era bug). Gate frontend verde.
- **✅ Feature COMPLETO + QA Chrome en vivo OK** (regla "Despacho Excesivo" cada 30 s muestra cadencia +
  próxima; filtro por tabla y grilla de días 1–31 renderizan bien).

**Ronda — Análisis ContaPlus + entrevista auditoría (27-jun, CAMBIOS §82):** Steven dio entrevista de
auditoría + docs ContaPlus (PDF/`.txt`, **algo viejos** → verificar contra `docs/contac-schema.sql`; las
imágenes/entrevista son recientes). **Hallazgo crítico: `FAC_DESP` = FORMA DE PAGO del despacho, NO
"facturado"** (datos reales {2,4,5,7}; mapa 0-9; PDF §3.4; la auditora no sabía qué era "5"). → Diccionario
corregido (`FAC_DESP`="Forma de pago", `COD_PAGO`=catálogo real) + comentario de `DetalleFacturaDto`. La
regla "Despacho no facturado" (y el fix §81) están sobre premisa equivocada — rehacer con cruce
`DESP.NUM_DESP↔DCTO.NDO_DCTO` (verificar). Síntesis + **backlog de auditoría priorizado** en
`docs/ANALISIS-CONTAPLUS-Y-ENTREVISTA-AUDITORIA.md` (🔴 placa reutilizada N+/día, factura fuera de
liquidación; 🟠 hipervínculos/buscador/ventana-nueva/dashboard por estación). Cambios no funcionales:
build verde.

**Ronda — FIX regla "Despacho NO facturado" (26-jun, CAMBIOS §81):** disparaba en TODOS los despachos de San
Pío. Causa: `DespachoNoFacturadoRule` leía `FAC_DESP` como 0/1 y marcaba todo lo que no fuera "1". En los datos
reales `FAC_DESP` ∈ {2,4,5,7, vacío} (código de estado de facturación, **nunca "1"**): poblado = ya facturado.
Fix: solo marca "no facturado" si viene **vacío o "0"**. +5 pruebas (Detectors 182). Corregido el comentario de
`DetalleFacturaDto.Facturado`. **Hay que reiniciar el API** para aplicarlo. Aclaración de tablas: **DCTO** =
cabecera de factura, **DESP** = detalle del despacho (una factura agrupa varios despachos).

**Ronda — Documentación: frecuencia por regla (anotada) + guía de relanzamiento (26-jun-2026):**
- **`docs/PROPUESTA-FRECUENCIA-POR-REGLA.md`** (nuevo): diseño/alcance del feature del ingeniero (cada regla
  con su cadencia). Conclusión: **mediano, no enorme**; lo difícil es la **ventana de datos** de reglas
  lentas (hoy el job marca `Procesada` tras una pasada), pero `IReglaBacktestService` (corre una regla sobre
  N días de staging) + idempotencia ya dan la base. Fases: 1 throttle, 2 ventana real. **Anotado, sin
  implementar** (a decisión de Steven).
- **`docs/RUNBOOK-PUESTA-EN-MARCHA.md`** §6: cómo relanzar cada componente (central Docker / estación / dev).
- Sin cambios de código. *(CAMBIOS §74)*

**Ronda — Fix bug grave: selector "campos a mostrar" (keys duplicadas) (26-jun-2026):**
- Síntoma: en el creador de reglas, "Información a mostrar en la alerta", al filtrar/borrar/seleccionar el
  buscador y los chips dejaban de responder.
- Raíz: `ReglasPersonalizadasController` agregaba el mismo campo relacionado **por cada relación** a la misma
  tabla destino → `nombre` duplicado → `key={c.nombre}` duplicada en React → reconciliado corrupto.
- Fix: **dedupe por Nombre** del catálogo de relacionados (backend, raíz) + dedupe defensivo de `todos` en el
  picker (frontend). Gate verde (Api 75, eslint+tsc+vite OK). *(CAMBIOS §73)*

**Ronda — Release 2.4.0 + cadena de despliegue lista para San Pío (26-jun-2026):**
- **Versión 2.4.0** (Directory.Build.props). Auditada toda la fábrica de despliegue: instalador del central
  (Docker, `instalar-central-windows`), portables (`publicar-*`), versionado y actualización.
- **Agente = avisa + 1 clic** (no auto); **monitor = auto** (6 h). Manifiesto en `/api/v1/agente/version`,
  exe en `/descargas` (wwwroot).
- Para dejarlo "listo para ambas" (reemplazo y update): URL del manifiesto **absoluta desde el request**
  (`AgenteController.Absolutizar`, sin IP fija); **volúmenes** `central-config` + `central-descargas` en
  `docker-compose.prod.yml` (sirve el update sin reconstruir imagen + persiste connection/operacion.json);
  script **`publicar-actualizacion-del-agente.bat`** (SHA256 + copia exe + genera `agente-version.json`);
  **`docs/RUNBOOK-PUESTA-EN-MARCHA.md`**.
- **Generado:** portables 4 plataformas (`dist/`) + paquete de actualización 2.4.0
  (`central-descargas/PetrolRios.StationAgent.exe` SHA256 `fda86354…`, `central-config/agente-version.json`).
- **San Pío:** dos vías listas — reemplazo limpio (recomendado este viaje) o update con 1 clic. *(CAMBIOS §72)*

**Ronda — "Frecuencia del análisis" a prueba de errores (25-jun-2026):**
- En Ajustes → "Operación del sistema", el cron crudo se cambió por un **desplegable en español**
  (Cada 5 min, Cada hora…) + opción **"Personalizado (avanzado)"** que revela el cron. `AjustesPage.tsx`.
- Backend `ParametrosOperacionController`: valida el cron **registrando el job en `try/catch`** (Hangfire
  parsea y lanza si es inválido **antes** de tocar storage) → **400 limpio en vez de 500**, y solo persiste
  si es válido. Test `Operacion_CronInvalido_DevuelveBadRequestNo500` (Api 75). *(CAMBIOS §71)*

**Ronda — Agente: ContaGober + arranque automático + portable nuevo (25-jun-2026):**
- **Auto-detección Firebird ampliada** (`FirebirdExtractor.RutasCandidatas`): añadidas rutas de ContaGober
  (`C:\Programas\ContaGober1\Datosc\CONTAB.FDB`) y variantes `CONTAB.FDB`; además **escanea** raíces de
  instalación (`C:\Programas`, Program Files, `C:\Conta`, `C:\` superficial) por `CONTA*.FDB`
  (IgnoreInaccessible, profundidad limitada).
- **Arranque automático al encender (las dos vías):** (a) botón en el panel **sin admin** —
  `InicioAutomatico` + `GET/POST /api/inicio-automatico` escribe un `.vbs` oculto en la carpeta de Inicio;
  (b) **servicio de Windows** (admin) con el `instalar_agente_servicio.bat` que ya viaja en el portable.
  Documentado en el panel (desplegable) y en `agente-LEEME-windows.txt`.
- **Portable reconstruido (4 plataformas)** con TODO lo acumulado (watermark TZ, nombres naturales,
  ContaGober, arranque automático) vía `publicar-solo-el-agente-multiplataforma.bat` → `dist/`.
- **Conteos:** Domain 40, Detectors 150, Monitor 2, Api 74 (gate verde con Docker). *(CAMBIOS §70)*
- **Pendiente San Pío (mañana):** copiar `dist/agente-windows` a la estación, configurar (auto-detectar la
  base), activar el arranque automático y validar que la built-in `Factura` fluye al día (fix de watermark).

**Ronda — "Datos recibidos" con nombre natural + tabla técnica (25-jun-2026):**
- En los logs el tipo se muestra como **"Nombre natural (TABLA)"** — `Factura (DCTO)`, `Anulación (ANUL)` —
  en la columna y en el desplegable del filtro. Backend: `CatalogoTiposTransaccion` (Application/Fuentes)
  resuelve tipo→(natural, tabla) para los 7 built-ins (+ variante `Anulaciones` y `Dcto`→DCTO), y para las
  fuentes configurables toma la tabla del catálogo (`FuentesDatos`). `DatoRecibidoResponse` ganó
  `TipoNatural`+`Tabla`; `GET /datos-recibidos/tipos` devuelve `{ tipo, etiqueta }`. 15 tests nuevos.
- Steven **borró la fuente `Dcto` duplicada** del selector (era el mismo DCTO que el built-in `Factura`).
- **Conteos:** Domain 40, **Detectors 150**, Monitor 2, Api 74. *Pendiente: screenshot en vivo (la
  extensión de Chrome se desconectó al relanzar el sistema; el gate y los 15 unit tests ya validan la lógica).*
  *(CAMBIOS §69)*

**Ronda — Robustez del creador de reglas + Logs + guard (25-jun-2026):**
- **Sección "Datos recibidos"** (logs crudos de agentes: tabla con tipo/estación/fecha/estado + **filas
  expandibles** con el JSON crudo, **filtros** tipo/estación/estado y **buscador**): `DatosRecibidosController`
  (`GET /api/v1/datos-recibidos` + `/tipos`) y página en Monitoreo. Verificada en vivo: 3.241 registros
  reales de San Pío (`DCTO` con COD_PAGO/COD_VEND/COD_CLIE reales). *(CAMBIOS §67, commit `d4b6fc4`)*
- **Guard anti-duplicación** del selector (no registrar tablas que ya extrae un built-in:
  DCTO→Factura, ANUL→Anulacion, …): `FuenteDatosPolicy.TablasBuiltIn` + 11 tests. Verificado en vivo:
  TURN_DEPO/CRED_CABE/TURN_TARJ→**400** ("ya se procesa como …"), DCTO/`dcto`→**409** ("ya registrada").
  *(CAMBIOS §66, commit `6f2d559`)*
- **Stress-test en Chrome del creador de reglas** (25+ casos: inyección SQL/XSS inertes — la tabla
  sobrevive y EF parametriza; 1000 condiciones y fuente configurable arbitraria → 201; aguanta y escala).
  **Bug cazado y arreglado:** un nombre/descripción/expresión más largo que su columna provocaba **HTTP 500**
  al guardar → ahora **400 limpio** (guards de longitud en `ReglasPersonalizadasController.Validar()`,
  espejo de la BD: Nombre 150 / Descripción 500 / Fuente 50 / Expresión 2000). Test de regresión
  `ReglasPersonalizadas_NombreDemasiadoLargo_DevuelveBadRequestNo500`. *(CAMBIOS §68)*
- **Autounidor `DescubridorRelacionesService`:** revisado (similitud de nombre + solape de valores en
  staging, umbrales prudentes) — bien implementado y escalable; sin cambios.
- **Conteos tras esta ronda:** Domain 40, Detectors 135, Monitor 2, **Api 74** (con Docker; +1 regresión).
  Agente v2.3.0.
- **Pendiente San Pío (mañana):** republicar el agente con FIX 1 (watermark por reloj de Firebird),
  validar que la built-in `Factura` fluye al día y la regla nueva dispara; luego **quitar del selector la
  fuente `Dcto` duplicada** (el agente ya la omite, pero conviene limpiar el catálogo).

**Ronda — Auditoría agente/reglas San Pío (25-jun-2026), commiteado:**
- Síntoma: en San Pío (estación real UTC-5) el agente envía datos (vía fuente configurable `Dcto`) pero la
  regla nueva no disparaba y los 4 detectores predeterminados se quedaban sin datos nuevos.
- **FIX 1 (watermark por reloj de Firebird):** el extractor built-in avanzaba la marca con `DateTime.UtcNow`
  pero `FEC_DCTO` es hora local → en UTC-5 saltaba 5 h y congelaba `Factura`/`DetalleFactura`/etc. Ahora la
  marca usa `CURRENT_TIMESTAMP` de Firebird (`ResultadoExtraccionAgente.RelojFirebird`), `Unspecified` +
  `RoundtripKind`, re-siembra de marcas viejas en UTC. (`FirebirdExtractor.cs`, `CycleRunner.cs`)
- **FIX 2 (tolerancia de nombres en reglas):** `CatalogoReglasPersonalizadas.GetValor` resuelve fuentes
  configurables con exacto → sin mayúsculas/espacios → puente amigable→crudo (`TotalNeto`→`TNI_DCTO`). +5 tests.
- Diagnóstico completo en `docs/DIAGNOSTICO-AGENTE-REGLAS.md`. Gate verde (Detectors **124**, +5). **Falta
  validar en San Pío mañana** (el desfase TZ solo aparece en estación real; el demo es UTC). *(CAMBIOS §65)*



**Última ronda — Reorganización de scripts (25-jun-2026), commiteado:**
- **TODOS** los scripts (`.bat`/`.ps1`/`.sh`) viven ahora bajo `ejecutables/` en 6 carpetas por tipo
  (`1-INICIAR-Y-DETENER`, `2-BASE-DE-DATOS-Y-DEMO`, `3-DIAGNOSTICO`, `4-VERIFICACION-Y-PRUEBAS`,
  `5-PUBLICACION-Y-DESPLIEGUE`, `6-INSTALAR-EN-NUEVO-PC`), con **nombres descriptivos** y un **bloque
  RESUMEN** al inicio de cada uno. Se vaciaron `scripts/`, `_arranque/` e `INSTALACION/`. Se borraron
  obsoletos (`verificar_2fa`, `verificar_ronda_fuentes` y 3 wrappers). `dist/` y `firebird_data/`
  quedan ignorados (build/datos). El **gate** ahora es
  `ejecutables/4-VERIFICACION-Y-PRUEBAS/verificar-todo-gate-oficial.bat`. Índice en `ejecutables/LEEME.md`. *(CAMBIOS §64)*

**Última ronda — Asignación de alertas "al 1000%" (25-jun-2026), commiteado:**
- Asignar una alerta ahora **avisa al asignado por correo** (`IEmailNotificacionService`, dirigido solo a
  él) y por **SignalR** (evento nuevo `AlertaAsignada` → toast personalizado "Te asignaron una alerta" en
  `NotificationProvider`), **registra quién la asignó** (`AsignacionAlerta.AsignadoPorId`, migración
  `AsignacionAsignadoPor`) y **muestra a quién está/fue asignada**: en el **detalle** un banner
  "Asignada a X (rol) · asignada por Y · fecha" + la tarjeta cambia a **"Reasignar"**, y en la **lista**
  el responsable bajo el estado. `AlertaResponse` ganó 5 campos de asignación; `AsignarAsync` recibe el
  asignador (claim → `ClaimsPrincipal.GetUsuarioId()`) y **devuelve 200** con la alerta actualizada.
  4 pruebas nuevas (`AlertaServiceAsignacionTests`). Verificado en Chrome (asigné #32 a Maria Fernanda
  Auditora; #33 histórica ya muestra su responsable). *(CAMBIOS §63)*
- **Conteos de pruebas tras esta ronda:** Domain 40, Detectors 119, Monitor 2, **Api 73** (con Docker
  arriba corren las de integración; +4 de asignación). Agente v2.3.0.

**Ronda previa — preparación de producción (24-jun-2026), commiteado:**
- **Nombre del empleado en las alertas** (no solo el código): catálogo central `Empleado` que el
  agente sincroniza desde Firebird (`VEND`/`EMPL`); `IEmpleadoDirectorio` resuelve `(estación,código)→
  nombre` en alertas, dashboard y reportes, sin tocar las 25 reglas. *(CAMBIOS §43–44)*
- **Escalabilidad >10 estaciones + rol `Agente`** propio (seguridad): alta de estación + usuario-agente
  desde "Nueva estación" (modal con credenciales) y desde "Nuevo Usuario" (código nuevo). Los `agent-*`
  ya NO son Auditor; la política "Central" excluye al rol Agente. *(CAMBIOS §45, commits `2d3d12a`,`bf8782a`)*
- **Robustez del agente y del gate:** el contador de enviadas cuenta los reenvíos; guardado del panel
  endurecido; `verificar-mejoras.bat` detiene servicios, asegura `dotnet-ef` y muestra progreso en vivo.
  *(CAMBIOS §46, commits `a50a79f`,`e4146c7`)*
- **Validación de contraseña del agente (≥6)** al crear estación (front+back). Prueba E2E **EST-777**
  en Chrome: rechazo de `1234`, creación con `123456`, agente conectado (182 ms, 2 enviadas,
  Sincronizada). *(CAMBIOS §47, commit `c66fb56`)*
- **Fix de producción CET/Shadow Stack:** el `.exe` abortaba en el servidor (assert
  `AreShadowStacksEnabled`, threads.cpp) en CPUs con CET + Windows con protección de pila por hardware.
  Solución: **`<CETCompat>false</CETCompat>`** en los 3 proyectos `.exe` (StationAgent, StationMonitor,
  Api) y republicar. Workaround sin recompilar: `Set-ProcessMitigation -Name <exe> -Disable UserShadowStack`.
  *(CAMBIOS §49)*
- **Conectividad/VPN:** `docs/CONECTIVIDAD-VPN.md` (subredes distintas + NAT en la red POS; ZeroTier
  queda en REQUESTING_CONFIGURATION; recomendado **Tailscale** por su relay sobre 443). Pendiente:
  levantar la VPN real en la estación.
- **Creador de reglas más usable (pedido del ingeniero), 2 partes:**
  - **Parte 1 (HECHA, commit `353740c`, CAMBIOS §50):** documentación automática de campos — el builder
    muestra ícono + nombre legible + descripción (`DiccionarioCamposContaplus`: glosario + inferencia por
    prefijo + tipo del esquema) en vez de códigos crudos.
  - **Parte 2 (HECHA, commits `74b84f9` backend + `cd4e3df` frontend, CAMBIOS §51):** juntar tablas +
    elegir qué campos relacionados se muestran en la alerta (placa/cliente/vendedor/factura). Entidad
    `RelacionTabla` + migración `EnriquecimientoReglasYRelaciones` + seed Despacho→Factura +
    `CamposMostrarJson` + enriquecimiento en `CustomRuleDetector` (cruza en memoria) + `/catalogo` con
    campos relacionados + CRUD `/api/v1/relaciones-tabla` + selector en el builder. Verificado en vivo
    (migración aplicada, relación sembrada, selector muestra 🚗 Placa / vendedor / cliente de "Factura
    del despacho").
  - **Pulido (CAMBIOS §52, commit `fe725a7`):** tooltip con el código técnico del campo + filtro por
    tipo en el modo avanzado + glosario ampliado (sin inventar códigos opacos de Contaplus).
  - **Autodescubridor de relaciones (CAMBIOS §53, commit `4135873`):** `ConceptosRelacion` +
    `DescubridorRelacionesService` emparejan llaves compartidas (concepto + nombre) y validan por
    solapamiento de valores en staging; corre al arrancar el central y por `POST /relaciones-tabla/descubrir`;
    flag `EsAutomatica` + migración `RelacionAutomatica`. Verificado en vivo: crea las relaciones solo.
    Opcional pendiente: pantalla Admin de relaciones y botón "Descubrir" en la UI.
- **Conteos de pruebas actuales:** Domain 40, Detectors 119, Monitor 2, Api 53 (+16 de integración
  saltadas sin Docker). Cobertura de `PetrolRios.Detectors` ≈ 96% líneas (OE5). Agente v2.3.0.
- **Pendiente para go-live:** republicar dist del Servidor y Monitor; prueba física en otra PC con la
  red real; TLS/HTTPS + secretos definitivos; watermark por ID monotónico. (Ver `docs/PENDIENTES.md`.)

**Hecho y commiteado (rondas previas):**
- 5 etapas previas: arreglo de login/correo/verificación; sacar alertas operativas de la bandeja de
  auditoría; separar Reglas y Fuentes de datos + rediseño; página de Ajustes (QOL); rediseño del Monitor.
- Función de **desbloqueo de cuenta** por autoservicio desde el login (con demo en vivo end-to-end).
- **Revisión E2E** de los 3 paneles con Chrome: sin bugs funcionales.
- **Informe de seguridad**: `docs/ANALISIS-SEGURIDAD.md` (sin vulnerabilidades críticas; 7
  recomendaciones de endurecimiento).
- **Etapa 1a de reglas** (`a2080c2`): carril **Operativa/Auditoría editable** por regla
  (entidad + migración + los 4 detectores lo leen); clasificación correcta sembrada; **recalibración**
  de *Tasa de anulaciones* 5%→3%.
- **Etapa 2** (`4be0564`): **rediseño de la UI de Reglas** — buscador, grupos colapsables, filas
  compactas y **carril editable con un clic** (chip).
- **Etapa 3** (`89b0be5`): **tablas estándar** visibles en Fuentes (central) y en el panel del agente
  (DCTO, DESP, TURN, TURN_DEPO, ANUL, CRED_CABE, TURN_TARJ); contador **Leídas/Enviadas acumulativo**.
- **Etapa 1b** (`1a0713c`): **2 reglas nuevas** (ComplianceViolation, Auditoría): *Venta sin
  identificación del cliente* (RUC/cédula, SRI) y *Despacho de alto volumen sin placa* (desvío, ARCERNNR).
- **Etapa 4 / limpieza** (`a4334c7`): borrados scripts scratch (`_z`, `_c`, `_diag`) y arreglado el
  PID fijo de `reiniciar_api.bat`.
- Verificación: suites en verde por etapa (hasta **206 pruebas**), lint + frontend build OK.

**Pendiente / opcional (lo demás ya está hecho):**
- **Deduplicar `SeedData`** (cosmético): consolidar las definiciones de reglas en una sola fuente
  (`SeedReglasDeteccionAsync` y `EnsureReglasNuevasAsync` repiten ~6 reglas). El código funciona;
  hacerlo en un pase enfocado y verificar que las 25 reglas (sus parámetros) sigan presentes vía la API.
- **Code-splitting del frontend** (bundle ~970 KB): `manualChunks` para separar vendor.
- **Endurecimiento de seguridad** (`docs/ANALISIS-SEGURIDAD.md`): rate-limit por IP en endpoints
  anónimos, lista blanca de identificadores en fuentes dinámicas, etc.
- Reglas adicionales que quedaron descartadas por ahora por ruido/datos: cupo mensual de galones por
  placa (requiere agregación histórica, no per-ciclo) y saltos de secuencia de facturación (falsos
  positivos entre lotes).

## 7. Recordatorio final

Hazlo todo seguido, en etapas, verificando (código + Chrome + pruebas) y commiteando cada una con mi
identidad, sin tocar mis cambios sin guardar. **No espero menos que la perfección.**
NOTA IMPORTANTE: pero primero lee todos los scrips, .bats, archivos, ETC, TODO DE TODO no te saltes nada y no continuas hasta que termines de analisar todo al completo cosa que este en mi proyecto de todas las carpetas para que armes un contexto completo y sepas que hacer cada cosa nates de que continues Y asi tener un contexto envidiable