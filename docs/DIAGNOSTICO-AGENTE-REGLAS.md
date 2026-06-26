# Diagnóstico: el agente envía datos pero la regla nueva no se ejecuta (San Pío)

Fecha: 25/26-jun-2026. Análisis de código + evidencia en vivo de la base central. **Sin cambios de código aún.**

## Síntoma
- El agente de **San Pío (EST-12, v2.3.0)** está **en línea** y **sí envían datos nuevos** (1707 → 1712 → 3214 filas `Dcto` en staging, la última `FEC_DCTO = 2026-06-25 17:11` local).
- Pero la **regla nueva "Prueba factura San Pio"** (total de factura > $1) **no genera alertas**.

## Causa raíz (DEFINITIVA): la regla apunta a otra fuente y a otro nombre de campo que el de los datos que llegan

DCTO está entrando al central de **DOS formas distintas, con nombres de campo distintos**:

| Cómo llega | `TipoTransaccion` | Origen | Nombres de campo | Frescura (evidencia) |
|---|---|---|---|---|
| Extractor **built-in** | `Factura` | `FirebirdExtractor.GetFacturasSql` (siempre activo) | **amigables**: `TotalNeto`, `CodigoPago`, `Placa`… | **CONGELADA**: 13 filas, última `19:08 UTC` (=14:08 local) |
| Fuente **configurable** (selector) | `Dcto` | `FuenteDatos` id 4 (tabla DCTO, watermark `FEC_DCTO`) | **crudos de Firebird**: `TNI_DCTO`, `COD_PAGO`, `PLA_DCTO`… | **AL DÍA**: 3214 filas, última `00:08 UTC` (=19:08 local) |

La regla `#45 "Prueba factura San Pio"` quedó guardada así:
- `FuenteDatos = "Factura"` (la built-in)
- Condición = `TotalNeto > 1`

El detector hace `ObtenerFuente(context, "Factura")` → lee `context.Facturas` = el bucket **built-in**, que está **congelado** (ver abajo). Los datos frescos de San Pío llegan en el bucket **`Dcto`**, que la regla **ni mira**. Resultado: la regla evalúa 0 filas nuevas → **nunca dispara**. Hay un **doble desajuste**:

1. **Fuente equivocada:** la regla apunta a `Factura` (built-in) pero el dato vivo llega como `Dcto` (configurable). Son `TipoTransaccion` distintos → `ObtenerFuente` devuelve el bucket equivocado.
2. **Campo equivocado (latente):** aunque apuntara a `Dcto`, la condición usa `TotalNeto`, pero las filas `Dcto` traen el nombre **crudo** `TNI_DCTO`. Una condición `TotalNeto > 1` sobre `Dcto` tampoco encontraría el campo → tampoco dispararía.

## El "diccionario": el mismo total con DOS nombres (confirmado, esto te confundió)
- En `CatalogoReglasPersonalizadas.Fuentes["Factura"]` (fuente built-in): `TotalNeto` → **"Total de la factura ($)"**.
- En `DiccionarioCamposContaplus` (fuente configurable `Dcto`): `TNI_DCTO` → **"Total con IVA ($)"**.
- Son **la misma columna** (`TNI_DCTO AS TotalNeto`). Pero `CatalogoReglasPersonalizadas.GetValor`, para una fuente configurable (registro = diccionario), hace **match EXACTO** del nombre crudo (`dict.TryGetValue(campo)`). No hay puente amigable↔crudo. → una regla con `TotalNeto` no resuelve sobre filas `Dcto` (que traen `TNI_DCTO`), y una regla sobre `Factura` lee el bucket built-in (congelado).

**Fix propuesto (código):** en `GetValor`, para fuentes configurables, resolver el campo con tolerancia: (1) exacto, (2) sin distinguir mayúsculas/espacios, (3) puente amigable→crudo (`TotalNeto`→`TNI_DCTO`, `Subtotal`→`SUB_DCTO`, `Cantidad`→`CAN_DESP`, …). Así una regla escrita con nombres amigables funciona sobre la fuente configurable.

## Por qué la built-in `Factura` está congelada (bug real de zona horaria)
- El watermark **global** del agente avanza con `DateTime.UtcNow` (`CycleRunner` línea 130) y arranca con `DateTime.UtcNow.AddHours(-1)`.
- Pero `FEC_DCTO` en Contaplus se graba en **hora LOCAL** de la estación (Ecuador, UTC-5). La consulta built-in filtra `WHERE FEC_DCTO > @watermark`.
- Tras el primer envío, el watermark salta a `UtcNow` (hora de pared 5 h adelantada) → las facturas nuevas (hora local) caen "detrás" del watermark → **no se extraen** hasta reiniciar el agente.
- **Evidencia:** la built-in `Factura` quedó clavada justo en `19:08 UTC` (=14:08 local), el instante en que el watermark saltó a UTC. La fuente configurable `Dcto` NO tiene este bug (su cursor es el valor crudo de Firebird, mismo reloj que `FEC_DCTO`) y por eso sigue al día.

> Por eso "antes funcionaba": el demo corría en un contenedor Docker con reloj **UTC**, así que watermark (UTC) y `FEC_DCTO` (UTC del contenedor) coincidían. En San Pío el reloj es local (UTC-5) → 5 h de desfase.

## Estructura real de DCTO (confirmada en `docs/contac-schema.sql` = backup real de Contaplus = San Pío)
Los nombres **crudos** que llegan en la fuente `Dcto` (y que una regla sobre `Dcto` debe usar):

| Campo crudo (Firebird) | Tipo | Significado | Alias "amigable" del built-in `Factura` |
|---|---|---|---|
| **`TNI_DCTO`** | Double precision | **Total neto de la factura** | `TotalNeto` |
| `TSI_DCTO` | Double precision | Total sin IVA | `TotalSinIva` |
| `SUB_DCTO` | Double precision | Subtotal | `Subtotal` |
| `DSC_DCTO` | Double precision | Descuento | `Descuento` |
| `IVA_DCTO` | Double precision | IVA | `Iva` |
| `FEC_DCTO` | Timestamp | Fecha del documento (watermark) | `FechaDocumento` |
| `COD_PAGO` | Char(3) | Forma de pago (**San Pío = `001`**, no `EF`) | `CodigoPago` |
| `COD_VEND` | Char(9) | Vendedor (San Pío = `DD0000013`) | `CodigoVendedor` |
| `COD_CLIE` | Char(9) | Cliente (San Pío = `CZ0061065`) | `CodigoCliente` |
| `PLA_DCTO` | Char(20) | Placa | `Placa` |
| `RUC_DCTO` | Char(13) | RUC | `RucCliente` |
| `NUM_TURN` | Integer | Turno | `NumeroTurno` |

→ Para que tu regla "total de factura > $1" dispare sobre lo que realmente llega: **Fuente `Dcto`, condición `TNI_DCTO > 1`**.

## Detalle extra que tampoco cuadra (datos reales ≠ demo)
En las filas reales de San Pío: `COD_PAGO = "001"` (no `"EF"`), `COD_VEND = "DD0000013"`, `COD_CLIE = "CZ0061065"`. Las reglas/detectores que comparan contra códigos del **demo** (`COD_PAGO='EF'`, vendedores de 3 caracteres) **no coincidirán** con San Pío. Cualquier regla debe usar los **valores reales** de San Pío.

## Solución DEFINITIVA

### Inmediata (sin tocar código) — para que tu regla dispare ya
Recrear/editar la regla para que apunte a la fuente que **sí** fluye y al **campo real**:
- **Fuente:** `Dcto` (la configurable del selector), no `Factura`.
- **Condición:** `TNI_DCTO > 1` (nombre crudo), no `TotalNeto > 1`.
- (Si filtras por forma de pago, usar el código real de San Pío, p. ej. `COD_PAGO = '001'`, no `'EF'`.)

### De raíz (código, para después — NO hacer aún)
1. **Arreglar el watermark del built-in:** anclar el watermark global al **reloj de Firebird** (`CURRENT_TIMESTAMP` del servidor) o al **máximo timestamp de las filas leídas**, en vez de `DateTime.UtcNow`. Así los buckets built-in (`Factura`, `DetalleFactura`, `CierreTurno`…) funcionan en estaciones de cualquier zona horaria y dejan de congelarse.
2. **Resolver la duplicación de DCTO:** que la misma tabla no entre a la vez como built-in `Factura` (campos amigables) y como configurable `Dcto` (campos crudos). Opciones: (a) si hay una fuente configurable que cubre una tabla built-in, **omitir** la built-in para esa estación; o (b) en el creador de reglas, **mapear** nombres amigables ↔ crudos para que una regla sobre `Dcto` pueda usar `TotalNeto` y el motor lo traduzca a `TNI_DCTO`.
3. **Coherencia de nombres en el creador de reglas:** que el builder, al elegir una fuente, muestre y guarde el nombre de campo **que realmente llega en el staging de esa fuente** (evitar que se mezcle el catálogo amigable de la built-in con las columnas crudas de la configurable).

## Checks en vivo que faltan (cuando vuelva la conexión)
- Confirmar `MAX(FEC_DCTO)` real en la Firebird de San Pío vs el cursor de la fuente `Dcto` (descartar overshoot).
- Confirmar que la venta recién despachada aparece en `DCTO` de Contaplus (no solo en el POS / "otro sistema").
