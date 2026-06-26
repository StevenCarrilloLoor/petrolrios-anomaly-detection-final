# Propuesta de diseño — Frecuencia de ejecución POR REGLA

> **Estado: ANOTADO (no implementado).** Documento de alcance y diseño para decidir cuándo y cómo
> hacerlo. No cambia código todavía.
> Fecha: 26 de junio de 2026 · Versión del sistema al anotar: 2.4.0

## 1. Qué pidió el ingeniero

> "Un parámetro que permita poner cada qué tiempo deba ejecutarse una regla, en vez de que las reglas se
> ejecuten todas a la vez. Puede haber una regla que se ejecute una vez a la semana, así que algunas serían
> innecesarias que se ejecuten todo el rato; o una regla que analice las transacciones finales del mes no
> tiene sentido que se analice cada minuto todos los días."

**Objetivo:** que cada regla tenga su **propia cadencia** (cada ciclo / por hora / diaria / semanal /
mensual), en lugar de que todas corran en cada ciclo del job.

## 2. Cómo funciona hoy (para entender el alcance)

- Un **único job de Hangfire** (`anomaly-detection`) corre cada X minutos (configurable en Ajustes →
  Operación del sistema). Código: `src/PetrolRios.Infrastructure/Jobs/AnomalyDetectionJob.cs`.
- En cada ciclo, por estación:
  1. Lee el **staging NO procesado** de esa estación (`TransaccionesStaging` con `Procesada == false`) y
     construye un `DetectionContext`.
  2. Corre **los 4 detectores** (que internamente evalúan TODAS las reglas del motor + las personalizadas).
  3. Persiste alertas, notifica por SignalR, y **marca esas transacciones como `Procesada = true`**
     (`AnomalyDetectionJob.cs`, línea ~229).
- Reglas:
  - **Motor (25 reglas):** entidad `ReglaDeteccion` (umbral/`Activa`/ámbito). Las evalúa `RuleBasedDetector`
    + las clases en `src/PetrolRios.Detectors/Rules/**`.
  - **Personalizadas:** entidad `ReglaPersonalizada`. Las evalúa `CustomRuleDetector`.
  - Ya existe un gate **por regla**: si `Activa == false`, el detector NO la ejecuta. (Aquí engancharía el
    nuevo "¿le toca por frecuencia?").

**Punto clave:** el modelo es **incremental de una pasada** — una transacción la ven todas las reglas en un
ciclo y luego se marca `Procesada` (se "consume").

## 3. El matiz importante: frecuencia ≠ ventana de datos

Bajar la frecuencia de una regla **no basta** para los casos del ingeniero:

- Una regla **por transacción** (ej.: "factura > $X", "placa genérica AND galones > 5") con frecuencia baja
  solo significa "procesa el backlog acumulado cuando le toque". Fácil… **siempre que** las transacciones no
  se hayan marcado `Procesada` y desechado antes de que la regla lenta corra.
- Una regla **de ventana/periodo** (ej.: "anulaciones > 5% del día", "totales de fin de mes", "mismo
  empleado con faltantes > 3 en 30 días") necesita ver **toda la ventana** (día/semana/mes). Hoy esos datos
  ya se marcaron `Procesada` en ciclos anteriores. Es decir, una regla lenta debe **consultar su ventana**
  (últimos 7/30 días) directo de la BD, no el "lote nuevo".

**Conclusión:** lo difícil no es el "campo de frecuencia + saltarse ciclos" (eso es trivial); es darle a las
reglas lentas su **ventana de datos** sin romper el pipeline incremental actual.

## 4. Maquinaria que YA existe y baja la dificultad

- **`IReglaBacktestService`** (`src/PetrolRios.Application/Interfaces/IReglaBacktestService.cs`): ya corre
  una regla **contra los datos de staging de los últimos N días**, sin marcar `Procesada` ni generar
  alertas (es la "vista previa" del creador de reglas). → Es **exactamente** el motor de "evaluar una regla
  sobre una ventana histórica". Para reglas programadas se reusaría esa evaluación, pero **sí** generando
  alertas.
- **Idempotencia** (huella SHA-256 en ingesta + dedupe de alertas): si una regla de ventana re-escanea su
  periodo, **no** crea alertas duplicadas. Esto elimina el principal riesgo de re-procesar.
- **El desplegable de frecuencia "a prueba de errores"** que ya se hizo para el cron global
  (`AjustesPage.tsx`, CAMBIOS §71) se **reutiliza** para el selector por regla.

## 5. Diseño propuesto

### 5.1 Modelo de datos
Agregar a **ambas** entidades de reglas (`ReglaDeteccion` y `ReglaPersonalizada`):

- `FrecuenciaEjecucion` (enum/string): `CadaCiclo` (default, = hoy), `Horaria`, `Diaria`, `Semanal`,
  `Mensual`. (Opcional avanzado: una expresión cron por regla, reutilizando la validación de cron del §71.)
- `UltimaEjecucion` (DateTime?, UTC): cuándo corrió por última vez.
- (Solo reglas de ventana) `DiasVentana` (int?): cuántos días atrás analizar cuando le toca. Para `Mensual`
  ≈ 31, `Semanal` ≈ 7, etc. Puede derivarse de la frecuencia por defecto.

Migración EF Code-First (una, con defaults: `CadaCiclo` y `UltimaEjecucion = null`) → no rompe nada
existente (todo sigue como hoy hasta que el usuario cambie una regla).

### 5.2 Lógica en el job (`AnomalyDetectionJob`)
Por cada regla, antes de evaluarla, calcular `¿LeToca(frecuencia, UltimaEjecucion, ahora)?`:

- **`CadaCiclo`:** se evalúa con el **lote incremental** de siempre (camino actual, sin cambios).
- **Frecuencias lentas (horaria/diaria/semanal/mensual):** solo cuando "le toca":
  - Obtener su **ventana** (`DiasVentana`) desde el staging persistido (reusando el evaluador del backtest,
    pero emitiendo alertas).
  - La **idempotencia** evita duplicados si la ventana solapa corridas previas.
  - Actualizar `UltimaEjecucion = ahora`.
- Las reglas lentas **no** dependen de `Procesada` (consultan su ventana directa), así que **no** hay que
  reescribir el marcado incremental actual.

### 5.3 UI
- Selector de **frecuencia por regla** (reusar el desplegable del §71) en:
  - Reglas del motor (`frontend/src/components/reglas/...` sección de reglas del motor).
  - Reglas personalizadas (`ReglasPersonalizadasSection.tsx`, junto a "ámbito"/"notificar correo").
- Mostrar `UltimaEjecucion` ("última vez: …") como ayuda.

### 5.4 Endpoints/DTOs
- Extender los DTOs de regla (motor y personalizada) y sus endpoints de guardar/editar para incluir
  `frecuenciaEjecucion`. Validar con la misma idea del cron del §71 (lista cerrada → imposible romper).

## 6. Fases (para que NO sea enorme)

- **Fase 1 — Throttle (pequeño-mediano):** campo de frecuencia + gate "¿le toca?" + UI, **para reglas por
  transacción**. Resuelve el 80% del pedido ("que no corra cada minuto"). Las reglas lentas, de momento,
  procesan el backlog acumulado.
  - Decisión de Fase 1: para que una regla lenta por-transacción no se pierda datos, **no** marcar
    `Procesada` hasta que la regla de menor frecuencia activa los haya visto, **o** que las reglas lentas
    consulten su ventana (ver Fase 2). Lo más simple y robusto es saltar directo al esquema de ventana.
- **Fase 2 — Ventana real (mediano):** reglas semanales/mensuales que consultan su periodo reusando el
  motor de backtest + idempotencia. Aquí está el grueso del diseño, pero acotado y con la pieza principal
  (backtest) ya hecha.

## 7. Esfuerzo estimado (orientativo)

- Fase 1: ~0.5–1 día (entidades + migración + gate + UI + tests).
- Fase 2: ~1–2 días (adaptar el evaluador de backtest para emitir alertas en el job + ventana por regla +
  pruebas de no-duplicación).
- **Total: mediano. No requiere reescribir el pipeline incremental.**

## 8. Riesgos y decisiones abiertas

- **Interacción con `Procesada`** (el punto crítico): recomendado **desacoplar** las reglas lentas del lote
  incremental (consultan su ventana directa). Así no se toca el camino rápido actual.
- **Duplicación de alertas** al re-escanear ventanas: cubierto por la idempotencia existente; igual conviene
  un test específico.
- **Carga**: una regla mensual sobre 31 días de staging puede ser pesada; correrla **una vez al mes** (no
  cada ciclo) es precisamente lo que la hace barata. Conviene escalonar (no todas las lentas el mismo
  minuto).
- **Zona horaria** de "fin de mes"/"fin de día": usar el reloj de la estación / TZ del central de forma
  consistente (ya hay precedente con el watermark por reloj de Firebird del §65).

## 9. Qué NO incluye esta propuesta (alcance acotado)

- No usa Machine Learning (excluido por la tesis, L02).
- No cambia el modelo incremental de las reglas rápidas (siguen igual).
- No crea un job de Hangfire por regla (se mantiene **un** job que internamente decide qué reglas tocan;
  más simple y suficiente).

## 10. Archivos que se tocarían (mapa)

- Dominio: `ReglaDeteccion`, `ReglaPersonalizada` (+ enum de frecuencia) y **una** migración EF.
- Job: `src/PetrolRios.Infrastructure/Jobs/AnomalyDetectionJob.cs` (gate "¿le toca?" + rama de ventana).
- Evaluación de ventana: reusar `IReglaBacktestService` / su implementación, parametrizado para emitir
  alertas.
- API/DTOs: controladores de reglas del motor y personalizadas (+ sus DTOs).
- Frontend: secciones de reglas del motor y personalizadas (selector de frecuencia, reusando el del §71).
- Tests: gate "¿le toca?" (unit) + no-duplicación al re-escanear (integración).

---

**Resumen para decidir:** factible, **mediano**, sin reescritura masiva, apoyado en piezas que ya existen
(backtest + idempotencia + selector de frecuencia). Cuando quieras avanzar, decidir si **Fase 1**, **ambas**,
o se queda anotado.
