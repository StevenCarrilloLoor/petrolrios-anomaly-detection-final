# Investigación: detección de anomalías sobre Firebird/Contaplus y plataforma de reglas flexible

Documento de trabajo de tesis. Reúne (1) el análisis tabla por tabla del esquema real
`contac-schema.sql`, (2) los patrones de fraude validados con literatura de la industria y con
la experiencia del ingeniero de PetrolRíos, (3) las anomalías nuevas detectables y su mapeo a
detectores, y (4) la arquitectura propuesta para convertir el sistema en una **plataforma
configurable** (extracción multi-tabla + registro dinámico de tablas con auto-documentación +
creador de reglas sin tocar código). Fecha: junio 2026.

---

## 1. Qué usamos hoy vs. qué hay realmente en la base

Hoy el agente extrae un conjunto **fijo** de 7 consultas (FACT/DCTO, DESP, TURN, TURN_DEPO,
ANUL, CRED_CABE, TURN_TARJ) con SQL escrito a mano en `FirebirdExtractor.cs`. Pero
`CONTAC.FDB` tiene **~200 tablas**. Muchas son contables/auxiliares, pero un grupo es muy
relevante para detección de fraude y operación, y hoy no se está aprovechando.

Un detalle técnico clave descubierto en el esquema: **casi todas las tablas tienen dos fechas**:
una de negocio que escribe el usuario (`FEC_*`, `FIN_*`, `FFI_*`) y una de inserción real con
`DEFAULT current_timestamp` (`FUL_*`). Comparar ambas detecta **backdating/postdating**
directamente — es la mejor respuesta al experimento que hiciste con la inserción fechada al
futuro. También hay **generadores** (`GEN_FACT_ID`, `GEN_DCTO_ID`, …) que respaldan IDs
monotónicos, útiles para un watermark inmune al reloj.

---

## 2. Tablas relevantes (análisis por dominio)

### 2.1 Turnos y cierre de caja
- **TURN** — `NUM_TURN` (PK, secuencial), `COD_VEND` (empleado), `FIN_TURN`/`FFI_TURN`
  (inicio/fin), **`EST_TURN`** (estado del turno: abierto/cerrado), `FAL_TURN` (faltante),
  `SOB_TURN` (sobrante), `CRE_TURN` (créditos), `ING_TURN`/`EGR_TURN`/`SFI_TURN`.
- **TURN_DEPO** — depósitos del turno (`VAL_TUDP`, `TOT_TUDP`).
- **TURN_TARJ** — tarjetas del turno (`VAL_TURN_TARJ`).

> **Anomalía nueva (pedido del ingeniero): turno sin cerrar.** `EST_TURN` = abierto con
> `FFI_TURN` nulo o muy anterior a ahora (p. ej. > 18 h). Es el "se olvidan de cerrar turno".

### 2.2 Despachos (surtidor)
- **DESP** — `NUM_DESP` (PK), `COD_MANG` (manguera), `CAN_DESP` (galones), `VUN_DESP`
  (precio), `COD_PROD` (producto), **`FAC_DESP`** (¿facturado?), `EST_DESP` (estado),
  `SUR_DESP` (surtidor), `COD_CLIE`, `FIN_DESP` (timestamp real).

> **Anomalía nueva: despacho no facturado.** `FAC_DESP` distinto de facturado = combustible
> servido que no se cobró (robo/merma). Cruce directo con la venta.

### 2.3 Tanques (cuadre volumétrico)
- **TANQ** — niveles y configuración del tanque.
- **TANQ_REPO** — reposición/cuadre por período: `DESCARGAS_TANQ`, `VENTAS_TANQ`,
  `FINAL_SISTEMA`, **`DIFERENCIA`** (descuadre físico vs. sistema).
- **TANQ_RIND** — rendimiento del tanque por cierre: **`DIF_VEN_RIND`** (diferencia de venta).
- **TANQ_MOV** — movimientos/mediciones.

> **Anomalía nueva: descuadre de tanque.** `DIFERENCIA`/`DIF_VEN_RIND` por encima de una
> tolerancia. Es el control estándar de la industria: comparar lo que salió del tanque (ATG)
> contra lo que el surtidor reportó / lo vendido. Detecta fugas, robo y surtidores manipulados.

### 2.4 Créditos
- **CRED_CABE** — cabecera: `NUM_CABE` (PK), `FEC_CABE` (fecha negocio), **`FUL_CABE`**
  (inserción real), `COD_SOCI` (cliente), `COD_GARA` (garante), `TCR_CABE` (monto),
  `COD_CRED` (tipo), `NUMMCOMP` (comprobante).
- **CRED_MOVI** — movimientos del crédito: `FVE_MOVI` (vencimiento), `FPA_MOVI` (pago),
  `BPA_MOVI` (pagado), `FUL_MOVI`.
- **CRED / CRED_TIPO / CRED_DECR** — catálogos.
- **CLIE_CUPO / CLIE_CUMO** — cupos de crédito del cliente.

> **Anomalías nuevas:** crédito **sin garante** (`COD_GARA` vacío) o **sobre el cupo** del
> cliente (`TCR_CABE` > cupo en CLIE_CUPO) o **a cliente no autorizado** (`COD_SOCI` no
> habilitado) — el caso que mencionó el ingeniero ("autorizan créditos a quien no debe").

### 2.5 Anulaciones y reversa (lo del ingeniero: cancelar y reingresar)
- **ANUL** — `NUMAN`, `FECHAANULACION`, `TIPOCOMPROBANTE`, `ESTABLECIMIENTO`,
  `SECUENCIALINICIO`/`FIN`, `AUTORIZACION`.

> **Anomalía nueva: cancelar-reingresar recurrente (kiting/jineteo).** Un comprobante/crédito
> anulado y vuelto a crear al día siguiente, una y otra vez, para "rodar" la deuda o mover el
> período. Se detecta correlacionando ANUL con re-creaciones del mismo documento/cliente en
> días consecutivos. Combinado con el **backdating** (`FEC_*` ≠ `FUL_*`) es una firma muy
> fuerte de manipulación.

### 2.6 Placas y cupos (regulación ARCERNNR)
- **PLACA** — `CODI_PLA`, `CUP_PLA` (¿con cupo?), `VAL_CUP_PLA`/`CON_CUP_PLA` (cupo/consumido).
- **PLACA_BLOQ** — placas **bloqueadas**.
- **PLACA_CUPO** — cupo por placa: `EST_PLCU`, `VAL_PLCU`, vigencia `FIN_PLCU`/`FFI_PLCU`.

> **Anomalías nuevas:** despacho a **placa bloqueada** (existe en PLACA_BLOQ) o que **excede el
> cupo** regulado (consumido > `VAL_CUP_PLA`). Complementa la regla existente de `ZZZ999949`.

### 2.7 Auditoría interna del POS
- **KEY_LOG_AAAAMM** — bitácora de teclas/acciones **particionada por mes** (KEY_LOG_202506…).
- **USUA / SESS / LOG** — usuarios del POS, sesiones y log. Útil para correlacionar quién hizo
  qué (operaciones fuera de horario, usuario inusual, etc.).

---

## 3. Patrones de fraude (validados con la industria)

La literatura de prevención de pérdidas en estaciones coincide con lo anterior:

- **Cuadre de inventario tanque vs. surtidor** como control primario; diferencias entre lo que
  sale del tanque y lo que el dispensador reporta delatan surtidores alterados o robo.
- **Controles por turno**: revisión de transacciones al cierre, validación de medidas, monitoreo
  de desviaciones — exactamente nuestro "turno sin cerrar" + faltante/sobrante.
- **Fraude de anulaciones/reversas**: anular ventas para sacar efectivo; el control es analizar
  voids por cajero/fecha/hora/monto/turno y marcar empleados con exceso de anulaciones.
- **"Sweetheart deals"**: descuentos a conocidos (descuento > permitido), ya contemplado.

Tipos de merma reconocidos: proveedor/transportista, empleado, cliente, y errores de papeleo —
nuestro scoring debe poder distinguir entre "error" y "patrón".

---

## 4. Mapeo a detectores

| Detector | Reglas actuales | Reglas nuevas propuestas |
|---|---|---|
| **CashFraudDetector** | faltante > umbral; faltantes recurrentes (gineteo) | **turno sin cerrar**; faltante/sobrante de tanque (cuadre) |
| **InvoiceAnomalyDetector** | anulaciones > %; precio/descuento fuera de rango; campos vacíos | **despacho no facturado**; **anulaciones por empleado** (frecuencia); **backdating** (`FEC_*` ≠ `FUL_*`) |
| **PaymentFraudDetector** | reversa tardía; crédito sobre límite sin autorización; duplicados | crédito **sin garante**; crédito a **cliente no autorizado**; **cancelar-reingresar recurrente** |
| **ComplianceViolationDetector** | `ZZZ999949` + galones; diésel y extra mismo día; fuera de horario | **placa bloqueada**; **placa sobre cupo** regulado |
| **(nuevo) IntegrityDetector** | — | **fecha fuera de rango plausible** (futuro/backdating); descuadre de tanque genérico |

---

## 5. La idea grande: de "4 detectores fijos" a **plataforma configurable**

> Tu pregunta: el agente hoy solo manda un conjunto fijo de tablas de `CONTAC.FDB`. ¿Se puede
> hacer flexible —enviar múltiples tablas o las que elijas—, mejorar el creador de reglas para
> que funcione sobre cualquier tabla, registrar tablas nuevas verificando que existan, y
> auto-documentar sus campos para crear reglas sin llamar a un ingeniero?

**Veredicto: es viable y es la jugada correcta para una tesis final.** Convierte el sistema de
una herramienta con lógica "cableada" a un **motor de detección dirigido por metadatos**. Diseño
profesional, por capas:

### 5.1 Fuentes de extracción configurables (multi-tabla)
Reemplazar el SQL fijo por una tabla de configuración **`FuenteExtraccion`** (en PostgreSQL,
editable desde el panel del agente/central):

- `Nombre`, `TablaFirebird`, `ColumnasJson` (o `*`), `ColumnaWatermark` (fecha o ID),
  `TipoTransaccion`, `Activa`, `FiltroOpcional` (WHERE adicional).
- El agente arma el `SELECT` a partir de esa config (siempre **solo lectura**, validado), igual
  que hoy pero sin recompilar. Watermark por **ID monotónico** cuando exista el generador, con
  ventana de solapamiento; la idempotencia del central (ya implementada) lo blinda.

### 5.2 Registro dinámico de tablas con verificación y auto-documentación
Cuando alguien quiera analizar una tabla nueva:

1. **Verificar que existe** consultando el catálogo de Firebird
   (`RDB$RELATIONS`) — si no existe, se rechaza con mensaje claro.
2. **Auto-documentar** sus columnas y tipos leyendo `RDB$RELATION_FIELDS` +
   `RDB$FIELDS` (nombre, tipo, longitud, nullable). Se devuelve un "diccionario de la tabla"
   que el usuario ve en pantalla: qué campos hay y de qué tipo.
3. Con ese diccionario, el usuario **arma reglas sobre esa tabla** desde la interfaz, eligiendo
   campo, operador y umbral — sin tocar código ni llamar a un ingeniero.

### 5.3 Motor de reglas genérico (fuera de CONTAC)
Hoy `ReglaPersonalizada` ya soporta expresiones. Se generaliza para que una regla apunte a una
**FuenteExtraccion** cualquiera y referencie sus campos por nombre. El evaluador corre la
expresión sobre cada registro de esa fuente y, si se cumple, genera alerta con su score. Así una
regla en una tabla X funciona igual que las de DCTO/TURN.

### 5.4 Lo que hay que cuidar (honestidad de ingeniería)
- **Seguridad:** todo es **SELECT** sobre Firebird en modo solo lectura; se valida el nombre de
  tabla/columna contra el catálogo (lista blanca) para evitar inyección; nunca se interpola
  texto libre del usuario en el SQL.
- **Rendimiento:** límites de filas por ciclo, watermark por fuente, e índices/where acotado.
- **Seguridad de expresiones:** el evaluador de reglas debe ser un intérprete acotado (sin
  ejecutar código arbitrario), con operadores permitidos y tipos validados.
- **Gobernanza:** solo Supervisor/Administrador pueden registrar fuentes y crear reglas; queda
  en el log de auditoría.

---

## 6. Plan por fases (propuesto)

1. **Watermark por ID + detector de fecha fuera de rango** (cierra la Fase 1 del watermark).
2. **Turno sin cerrar + despacho no facturado + cuadre de tanque** (alto valor, tablas ya
   accesibles, encajan en los detectores actuales).
3. **Fuentes de extracción configurables** (multi-tabla sin recompilar).
4. **Registro dinámico + auto-documentación** (introspección de Firebird).
5. **Creador de reglas genérico** sobre cualquier fuente + gobernanza/auditoría.
6. **Créditos no autorizados / sin garante / kiting** y **placa bloqueada / sobre cupo**.

Cada fase compila, prueba y commitea por separado, manteniendo el sistema siempre funcional.

---

## Fuentes (industria)
- goftx — Gas Station Fuel Theft: How to Prevent Loss: https://goftx.com/blog/gas-station-fuel-theft-prevention/
- goftx — Fuel Inventory Accuracy Playbook: https://goftx.com/blog/fuel-inventory-accuracy-playbook-gas-station/
- PDI Technologies — Enhancing Fuel Inventory Reconciliation: https://pditechnologies.com/blog/enhance-fuel-inventory-reconciliation-prevent-loss/
- Regions Bank — Types of POS Fraud and How to Prevent Them: https://www.regions.com/insights/small-business/article/types-of-pos-fraud-how-to-prevent-them
- Infosys BPM — POS Fraud Detection and Prevention: https://www.infosysbpm.com/blogs/bpm-analytics/solutions-to-pos-fraud-challenge.html
- Fraud.net — What Is Transaction Reversal Fraud: https://www.fraud.net/glossary/transaction-reversal-fraud
