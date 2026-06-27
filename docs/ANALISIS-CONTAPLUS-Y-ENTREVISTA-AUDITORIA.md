# Análisis: documentación ContaPlus + entrevista de auditoría (Juan Valdez)

> **Fecha:** 27-jun-2026. Insumos: entrevista con la auditora (audio transcrito), documentación
> técnica de ContaPlus (PDF + `.txt`), y capturas reales del catálogo **Forma de Pago** del POS.
> **Propósito:** dejar preparado (1) qué corregir en el diccionario de datos y las reglas, y (2) el
> backlog de mejoras que pidió auditoría, para ejecutarlo por etapas verificadas.

## 0. Confiabilidad de las fuentes (¡importante!)

| Fuente | Confiabilidad | Uso |
|---|---|---|
| **`docs/contac-schema.sql`** (schema real Firebird) | ✅ Fuente de verdad de **nombres** de tablas/columnas | Siempre verificar aquí |
| **Capturas "Forma de Pago"** (POS, recientes) | ✅ Confiable para el catálogo real de `COD_PAGO` | Catálogo de pagos |
| **Entrevista auditoría** (reciente) | ✅ Confiable para el negocio y el wishlist | Reglas/UX nuevas |
| **PDF `Documentacion_BD_ContaPlus`** | ⚠️ Algo viejo + nombres "idealizados" (`COD_DCTO`, `TOT_DCTO`, `DES_DCTO`) que **NO** existen así en el schema real (`SEC_DCTO`, `TNI_DCTO`, `DSC_DCTO`) | Solo conceptos/flujos, **no** nombres |
| **`DOCUMENTACION TECNICA DE TABLAS`** (`.txt`) | ⚠️ Algo viejo, pero más cercano al real | Conceptos; verificar campos |

**Regla de oro:** todo nombre de campo se confirma en el schema real; toda semántica de doc viejo se
marca "verificar contra datos" antes de cablearla en una regla.

---

## 1. 🚨 Hallazgo crítico: `FAC_DESP` es la FORMA DE PAGO, no "¿facturado?"

El campo `DESP.FAC_DESP` (que el agente envía como `Facturado` en `DetalleFacturaDto`) **no indica si
el despacho se facturó**. Es el **código de forma de pago** del despacho. Lo confirman **tres fuentes +
los datos reales**:

- **Datos reales** (consulta a `transacciones_staging`): `FAC_DESP` ∈ {`2`,`4`,`5`,`7`, vacío} — nunca
  `1`. Imposible para un indicador 0/1.
- **`.txt` técnico:** *"EN LA TABLA DESP EN EL CAMPO FAC_DESP, SE REGISTRA LA FORMA DE PAGO QUE SE UTILIZÓ
  EN EL DESPACHO"*, con la equivalencia:

  | `FAC_DESP` | Significado | → `DCTO.COD_PAGO` |
  |---|---|---|
  | 0 | Cheque (RUC) | |
  | 1 | Cheque (cédula) | |
  | 2 | **Contado (RUC)** | 001 |
  | 3 | Crédito | CRE |
  | 4 | **Tarjeta crédito (RUC)** | 002 |
  | 5 | **Contado (cédula)** | 001 |
  | 6 | Contado (pasaporte) | 001 |
  | 7 | **Tarjeta crédito (cédula)** | 002 |
  | 8 | Tarjeta crédito (RUC) | 004 |
  | 9 | Tarjeta débito (cédula) | 004 |

- **PDF** (sección 3.4): *"FAC_DESP — Forma de pago del despacho (mapeado a COD_PAGO en DCTO)"*.
- **Entrevista:** la auditora, viendo `Facturado=5`, dijo literal *"¿Qué es el facturado 5? No sé"* — no
  es un dato que ellos lean como facturación.

### Impacto en la regla "Despacho NO facturado" (CAMBIOS §81)

- La regla original marcaba "no facturado" todo `FAC_DESP != "1"` → disparaba en todos (bug ya
  identificado).
- **El fix §81** (marcar solo `vacío`/`"0"`) **frenó la inundación pero está sobre premisa equivocada**:
  `FAC_DESP="0"` = **cheque (RUC)**, un pago válido — marcarlo como "sin cobrar" sería un nuevo falso
  positivo. El campo no sirve para saber si se facturó.

### Cómo se sabe DE VERDAD si un despacho se facturó

Según el `.txt` técnico (⚠️ verificar contra datos reales): se cruza **`DESP.NUM_DESP` ↔ `DCTO.NDO_DCTO`**.
Un despacho está facturado si **existe un `DCTO` (FV) que lo referencia** en `NDO_DCTO`. Campos que
**sí existen** en el `DCTO` real y soportan el flujo EB→FV:

- `ANE_DCTO Char(1)` — `1` = egreso de bodega (EB) pendiente; `0` = ya consolidado en factura.
- `NUM_CONS` — número de consolidación que vincula los EB con su FV.
- `COL_DCTO` — secuencial de la factura que consolidó los EB.
- `NDO_DCTO Char(20)` — referencia al documento/despacho relacionado.
  (Ojo: en devoluciones `DV`, `NDO_DCTO` apunta al `SEC_DCTO` del documento original, no a un despacho.)

### Acción recomendada (a confirmar con Steven)

1. **Inmediato (seguro):** corregir el diccionario — `FAC_DESP` = *"Forma de pago del despacho"* (hecho en
   esta ronda). ✔️
2. **Corto plazo:** **desactivar por defecto** la regla "Despacho no facturado" (hoy no es fiable desde los
   datos que tenemos), o
3. **Reescritura correcta (recomendada):** que el agente envíe `NDO_DCTO`/`ANE_DCTO` del `DCTO` y un campo
   de enlace del `DESP`, y rehacer la regla como **cruce DESP↔DCTO** (despacho sin factura que lo
   referencie). Requiere verificar el enlace `NUM_DESP↔NDO_DCTO` contra datos reales primero.

---

## 2. Catálogo real de Formas de Pago (`COD_PAGO`) — de las capturas del POS

Tomado de las pantallas "Forma de Pago" (recientes). `Tipo`: FV=factura venta, DV=devolución venta,
PC/PV/PD/PL=pagos, BC/BV/FC=compras/contado, etc. `DCr`: D=débito / C=crédito.

**Ventas (las que más nos importan):**

| Código | Nombre | Tipo |
|---|---|---|
| `001` | CONTADO ISLAS (efectivo) | FV |
| `002` | TARJETA DE CRÉDITO | FV |
| `003` | TARJETA DE DÉBITO | FV |
| `004` | CHEQUE | FV |
| `020` | PAGO YA | FV |
| `021` | OTROS PAGOS | FV |
| `CRE` | CRÉDITO | FV |
| `CON` | CONTADO ISLAS | FV |
| `EFE` | EFECTIVO | PC |
| `101`–`104` | (variantes de devolución: contado/tarjetas/cheque) | DV |

(Hay decenas más para compras/devoluciones; el listado completo está en las capturas. Si se quiere, se
puede sembrar este catálogo en el diccionario para traducir `COD_PAGO`/`FAC_DESP` a su nombre legible.)

---

## 3. Diccionario de negocio (tablas reales que importan)

Confirmado contra `docs/contac-schema.sql`. Las **tres tablas que auditoría realmente usa** (entrevista):
**ventas (DCTO/FV)**, **devoluciones (DV / DCTO_DEV)** y **egresos**, además de **liquidaciones (LIQU)**.

- **`DCTO`** = cabecera del documento (factura `FV`, egreso de bodega `EB`, nota de crédito `DV`).
  Campos clave reales: `SEC_DCTO` (secuencial/PK), `TIP_DCTO` (FV/EB/DV), `NUM_DCTO` (nº factura),
  `FEC_DCTO`, `COD_CLIE`, `COD_VEND`, `COD_PAGO`, `TNI_DCTO` (total c/IVA), `TSI_DCTO` (base), `IVA_DCTO`,
  `DSC_DCTO` (descuento), `PLA_DCTO` (placa), `RUC_DCTO`, `NUM_TURN`, `COD_CHOF`, `NUM_CONS`, `COL_DCTO`,
  `ANE_DCTO`, `NDO_DCTO`, `PAG_DCTO` (=TSI+IVA, pero **0 si es venta a crédito**).
- **`DESP`** = detalle del despacho de combustible. `NUM_DESP` (PK), `CAN_DESP` (galones), `VTO_DESP`
  (valor $), `VUN_DESP` (precio/galón), `COD_PROD`/`NOM_PROD`, `COD_MANG`, `FIN_DESP` (fecha), `COD_CLIE`,
  **`FAC_DESP` (forma de pago)**, `EST_DESP` (estado), `NUM_LIQU`.
- **`TURN`/`MOLI`** (turnos): apertura/cierre, lecturas de contadores; `NUM_LIQU=0` = turno sin liquidar.
  Puntos de lubricantes: `COD_PUNT` 15/17/19.
- **`LIQU`** (liquidaciones): cierre de turno; totales por forma de pago; `NUM_TURN` indica qué turno se
  liquidó → permite cruzar **qué facturas (DCTO con ese `NUM_TURN`) entraron en la liquidación**.
- **`CLIE`** (clientes y **despachadores**): despachador = `CPR_CLIE='D'`; `EST_CLIE='02'` = cliente a
  crédito; `CRE_CLIE` = cupo disponible; `APE_CLIE=1` = siempre imprime factura aunque tenga crédito.
- **`CRED`/`CRED_CABE`** (créditos): garante (`COD_GARA` vacío = sin garante), `COD_AUTO` (autorización),
  cupo de crédito.
- **`ANUL`** (anulaciones), **`DCTO_DEV`** (relación factura↔nota de crédito, creada 2023).

### Relaciones útiles (para enriquecer alertas / reglas con joins)

- `DCTO.COD_CLIE → CLIE` · `DCTO.COD_VEND → VEND` · `MOVI.SEC_DCTO → DCTO` (líneas de la factura).
- `DESP.NUM_DESP ↔ DCTO.NDO_DCTO` (despacho ↔ factura que lo consolidó) — ⚠️ **verificar**.
- `LIQU.NUM_TURN ↔ DCTO.NUM_TURN` (facturas de un turno liquidado).
- `DCTO_DEV.SEC_DCTO_DEV ↔ DCTO.SEC_DCTO` (FV de la que salió una N/C).

### Confirmaciones de negocio de la entrevista

- **Descuento:** *"no se aplica nunca"* → para esta operación, **cualquier `DSC_DCTO` > 0 es anómalo**
  (umbral efectivo 0; la regla de descuento excesivo aplica fuerte aquí).
- **Turno sin cerrar:** un mismo empleado **no debe estar > 24 h**; puede pasar de 8 h (doble turno),
  pero no 24 h → umbral 24 h correcto.
- **Placa obligatoria:** no se puede despachar sin placa; el problema real es la **reutilización** de
  placas (ver backlog #1).
- **`NUM_CONS` (consecutivo):** número rápido de búsqueda del POS (≈ número de despacho); en muchas FV
  llega en 0.

---

## 4. Backlog de auditoría (lo que pidió la auditora) — priorizado

### 🔴 Alta prioridad (reglas)

1. **Misma placa / mismo cliente facturado muchas veces en el día** (reutilización de placa). Caso real:
   **14 facturas en un día** a la misma placa. Riesgo: denuncia SRI + multas. Regla nueva: agrupar por
   placa (o cliente) en **ventana de día**; si > N veces → alerta de Auditoría. Umbral configurable (la
   auditora sugiere **2**). Complementa la actual "Despachos rápidos" (ventana de 10 min) con ventana
   diaria. *(El bloqueo en el surtidor es del POS, fuera de alcance; nosotros alertamos + el monitor de la
   estación lo muestra al administrador para autorizar/rechazar.)*
2. **Factura no incluida en ninguna liquidación** (cuadre de turno). Al cierre, todas las facturas del
   turno deben estar en su liquidación; una factura "colgada" (su `NUM_TURN` no aparece liquidado en
   `LIQU`) → alerta. Requiere que el agente envíe `LIQU` + el enlace `NUM_TURN`.
3. **Reescribir/depurar "Despacho no facturado"** (ver §1) — depende del cruce DESP↔DCTO.

### 🟠 Media prioridad (UX / utilidad — "que funcione como ERP")

4. **Hipervínculos en las alertas:** clic en cliente/RUC → ver **todas sus facturas** (reporte por rango
   de fechas); clic → ver la **factura completa**; abrir en **ventana nueva** (comparar lado a lado); clic
   en despachador → sus despachos + filtro de fecha + imprimir.
5. **Buscador en alertas por placa / RUC / nombre** (botón).
6. **Número de factura (`NUM_DCTO`) visible en la alerta** — no solo la referencia técnica larga.
7. **Dashboard filtrable por estación de servicio** (no mezclar estaciones; las alertas ya se filtran, el
   dashboard no).
8. **Reportería en la misma pantalla** (clic para reporte/imprimir/Excel sin navegar a otro lado).

### 🟡 Baja prioridad / pulido

9. **Botón "copiar al portapapeles"** en alertas (sobre todo hipervínculos).
10. **Donaciones/descuentos excesivos por vendedor** sobre el periodo (global o por vendedor).
11. Confirmadas y ya implementadas (la auditora las validó): nombre del despachador en alertas,
    notificación por correo + asignación a auditor, comentarios tipo foro, 2FA, logs de auditoría,
    programación por regla (ejecutar la regla de cierre de turno **justo después** del cierre).

---

## 5. Estado de esta ronda

- ✅ **Diccionario corregido:** `FAC_DESP` ahora se documenta como *"Forma de pago del despacho"* (antes
  decía erróneamente "¿Facturado?"), y `COD_PAGO` apunta al catálogo real (001/002/003/004/CRE/EFE…).
- ⏳ **Preparado (este documento):** el backlog de auditoría y el plan de corrección de "Despacho no
  facturado". Pendiente de confirmar con Steven cuál abordar primero (sugerencia: backlog #1 — placa
  reutilizada en el día — por su impacto y porque no depende de verificar enlaces nuevos).
