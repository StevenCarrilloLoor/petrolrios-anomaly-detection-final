# Avance Capstone — Guion de video, documento y checklist
**PetrolRíos — Sistema de Detección de Anomalías Transaccionales**
TITA7912 · Steven Carrillo + Leonardo Andrade · Entrega: viernes 22 may 2026, 23:59

---

## 1. Guion del video (máx. 3 minutos · 2 personas · grabación de pantalla en vivo)

> **Reparto sugerido:** E1 = Steven, E2 = Leonardo (pueden intercambiarse).
> Hablen pausado y claro. El video se graba mostrando la pantalla del sistema YA levantado.
> Total hablado ≈ 390 palabras → entra en 3 min con las pausas de demo.

### SEGMENTO 1 — Introducción y problema · 0:00–0:30 · habla **E1**
**En pantalla:** portada/título del proyecto o el README con el diagrama de arquitectura.

> "Buenas tardes. Somos Steven Carrillo y Leonardo Andrade. Presentamos el avance de
> nuestro proyecto Capstone: un sistema web de detección de anomalías transaccionales
> para la empresa PetrolRíos S.A. El problema es claro: la empresa procesa entre trece y
> quince mil transacciones diarias en diez estaciones, pero su control actual solo verifica
> que los totales cuadren; no analiza las transacciones individuales. Eso deja sin detectar
> fraudes que se ocultan dentro de operaciones que numéricamente cuadran."

### SEGMENTO 2 — Solución y arquitectura · 0:30–0:55 · habla **E2**
**En pantalla:** diagrama de contenedores (ARQUITECTURA.md) o el README.

> "Nuestra solución es un sistema en capas con ASP.NET Core 9, React con TypeScript y
> PostgreSQL. Cada estación tiene un agente que extrae las transacciones de su base
> Firebird en modo solo lectura y las envía al servidor central. Un proceso programado con
> Hangfire ejecuta cuatro detectores cada cinco minutos y notifica las alertas en tiempo
> real mediante SignalR."

### SEGMENTO 3 — Demo: ingesta y procesamiento batch · 0:55–1:40 · habla **E1**
**En pantalla:** Swagger (`:5170/swagger`) → luego Hangfire (`:5170/hangfire`).

> "Veamos el sistema funcionando. Este es el API REST documentado en Swagger. El endpoint
> de ingesta recibe los lotes de transacciones que envían los agentes de estación,
> autenticados con JWT."
> *(mostrar el endpoint POST /api/v1/ingesta en Swagger)*
> "Las transacciones llegan en crudo a una tabla de staging en PostgreSQL. Y este es el
> panel de Hangfire: el trabajo 'anomaly-detection' se ejecuta automáticamente cada cinco
> minutos, y también lo podemos disparar manualmente."
> *(mostrar Recurring Jobs → Trigger now)*
> "Cada ejecución corre los cuatro detectores en paralelo sobre los datos nuevos."

### SEGMENTO 4 — Demo: panel del auditor y detectores · 1:40–2:40 · habla **E2**
**En pantalla:** frontend (`:5173`) → login → Dashboard → Alertas → Detalle de una alerta.

> "Este es el panel del auditor. Iniciamos sesión con control de acceso por roles."
> *(login con admin@petrolrios.com)*
> "El dashboard muestra los indicadores en tiempo real: total de alertas, alertas críticas,
> score promedio y estaciones activas."
> *(mostrar Dashboard)*
> "En la sección de alertas vemos las anomalías detectadas, clasificadas por nivel de
> riesgo. Tenemos los cuatro tipos de la tesis funcionando: fraude de efectivo, anomalía de
> factura, fraude de pago y violación normativa."
> *(mostrar lista de Alertas)*
> "Por ejemplo, esta alerta detecta una venta a la placa genérica ZZZ999949 con ocho
> galones, cuando la regulación de ARCERNNR permite máximo cinco. Cada alerta incluye su
> score de riesgo, la evidencia y el empleado involucrado."
> *(abrir el detalle de la alerta de Violación Normativa)*

### SEGMENTO 5 — Estado de avance y cierre · 2:40–3:00 · hablan **E1 y E2**
**En pantalla:** vuelta al Dashboard o pantalla de cierre.

> **E1:** "A la fecha tenemos el backend completo, los cuatro detectores, el procesamiento
> batch y el panel web funcionando de extremo a extremo."
> **E2:** "Los próximos pasos son ampliar las pruebas automatizadas y la validación con
> datos reales de Firebird. Gracias por su atención."

---

## 2. Esquema del documento de avance

El comité pide subir el documento "tal como está a la fecha". Es el documento de la tesis
(TITA 1) **actualizado** con la evidencia de implementación de TITA 2. Secciones a revisar/añadir:

**Mantener y pulir (de TITA 1):**
1. Identificación del problema o necesidad (síntomas, causas, impacto medible).
2. Requerimientos funcionales y no funcionales (en formato de lista clara — RF-01…, RNF-01…).
3. Alternativas de solución con diagramas C4 nivel 2.
4. Selección de la mejor solución con tabla comparativa ISO/IEC 25010:2023 (escala 1–5).
5. Impacto del proyecto con marco STEEP.
6. Objetivos general y específicos (OE1–OE5).
7. Alcance: diagrama de casos de uso + declaración de alcance.

**Reforzar / añadir (lo nuevo de TITA 2 — es lo que el comité quiere ver):**
8. Planificación: EDT o user story mapping + tablero (Trello/Jira) con accesos al guía.
9. Diseño de la solución con C4 **hasta nivel 4** (modelo entidad-relación y/o de clases).
10. Desarrollo: evidencia de la metodología Scrum — product backlog, sprints, tablero,
    capturas de avance por sprint, enlace al repositorio Git con commits descriptivos.
11. Pruebas y evaluación: estrategia de pruebas basada en ISO/IEC 29119 — niveles y tipos
    (unitarias, integración, sistema, aceptación), ambientes y datos de prueba, criterios.
12. Automatización / CI-CD: evidencia del pipeline (workflow, logs de ejecución, reportes
    de pruebas), indicando qué se automatiza y cuándo se ejecuta.

> **Atajo realista para hoy:** marquen en el documento, con resaltado o comentarios, qué
> partes ya están hechas y cuáles están en progreso. El comité pidió el avance "tal como
> está", no la versión final.

---

## 3. Checklist de grabación (dejar el sistema listo ANTES de grabar)

**Preparación del entorno:**
- [ ] Abrir **Docker Desktop** y esperar a que diga "running".
- [ ] Ejecutar `_arranque\ARRANQUE_DEMO.bat` (levanta PostgreSQL + API + Frontend).
      *Alternativa: correr 01, 02 y 03 en orden.*
- [ ] Esperar a que la ventana del API muestre "Ciclo completado" y la de Vite muestre
      "VITE … ready".
- [ ] Ejecutar `_arranque\04_seed_staging_demo.bat` para inyectar transacciones de demo.
- [ ] Abrir `:5170/hangfire/recurring` → seleccionar `anomaly-detection` → **Trigger now**.
- [ ] Confirmar en `:5173/alertas` que aparecen las alertas de los 4 detectores.

**Pestañas del navegador listas (en este orden, para no perder tiempo en el video):**
- [ ] Pestaña 1: `http://localhost:5173` (login del panel)
- [ ] Pestaña 2: `http://localhost:5170/swagger`
- [ ] Pestaña 3: `http://localhost:5170/hangfire/recurring`

**Antes de pulsar Grabar:**
- [ ] Cerrar notificaciones de Windows, WhatsApp, correo, etc. (modo "No molestar").
- [ ] Grabador de pantalla listo: **Xbox Game Bar** (tecla Win + G) o **OBS Studio**.
- [ ] Probar el micrófono; grabar en un lugar silencioso.
- [ ] Tener este guion abierto en una segunda pantalla o impreso.
- [ ] Hacer **un ensayo cronometrado** — el límite de 3 minutos es estricto.

**Después de grabar:**
- [ ] Revisar que el audio se escuche y que el video dure ≤ 3:00.
- [ ] Subir a YouTube (no listado) o Microsoft Stream y copiar el enlace.
- [ ] Subir el documento de avance + pegar el enlace del video en la actividad del aula.

---

## 4. Datos útiles para el video

| Recurso | URL / Valor |
|---|---|
| Panel del auditor (frontend) | http://localhost:5173 |
| Login | admin@petrolrios.com / Admin123! |
| Swagger (API REST) | http://localhost:5170/swagger |
| Hangfire (procesamiento batch) | http://localhost:5170/hangfire |
| Detectores | Cash Fraud · Invoice Anomaly · Payment Fraud · Compliance Violation |
| Niveles de riesgo | Bajo 0–25 · Medio 26–50 · Alto 51–75 · Crítico 76–100 |
| Stack | ASP.NET Core 9 · React + TypeScript · PostgreSQL 16 · Hangfire · SignalR |
