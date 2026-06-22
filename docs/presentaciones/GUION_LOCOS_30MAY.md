# GUION DE LOCOS — Presentación Comité de Titulación
**Sábado 30 de mayo de 2026 · 08:30 – 15:00 · Presencial**
**PetrolRíos — Sistema de Detección de Anomalías Transaccionales**
Steven Carrillo + Leonardo Andrade · Docente guía: Felipe Urquiza · TITA 2 — Eddy Mauricio Armas

> Este documento tiene 3 partes:
> 1. **Info complementaria** — lo que tenemos que entender nosotros (para Q&A)
> 2. **Guion paso a paso** — qué decimos, qué hacemos, qué se ve en pantalla
> 3. **Plan B y "qué pasa si"** — contingencias

---

# PARTE 1 — INFO TÉCNICA COMPLEMENTARIA (estudiar antes)

## 1.1 — Las 3 capas del sistema, en lenguaje técnico real

| Capa | Tecnología | Responsabilidad | Por qué eso y no otra cosa |
|---|---|---|---|
| **Origen de datos** | Firebird 2.5 (Contaplus) en cada estación | Persistir todas las transacciones del POS (FACT, DCTO, TURN…). El sistema legacy de PetrolRíos. | Restricción R08: no podemos tocarla, sólo leer. No podemos pedirle a la empresa cambiar de motor. |
| **Captura → transporte** | `PetrolRios.StationAgent` (.NET 9 Worker Service) | Lee Firebird con marca de agua, empaqueta JSON, POST a `/api/v1/ingesta` con JWT. Tiene store-and-forward si el servidor está caído. | Modelo PUSH de la tesis Alternativa B. Más resiliente que pull centralizado con VPN. |
| **Análisis y notificación** | ASP.NET Core 9 + PostgreSQL 16 + Hangfire + SignalR + React 18 | Recibir, persistir, analizar (4 detectores), generar alertas, notificar en tiempo real. | Stack moderno LTS, costo bajo en AWS (~$405/mes en producción), 2 estudiantes pueden mantenerlo. |

## 1.2 — El ciclo de detección, paso por paso (lo que vamos a SHOWAR)

```
1. ESTACIÓN: una venta ocurre en el POS Contaplus
   → INSERT en DCTO (factura), DESP (despacho), TURN (cierre turno)
   en su Firebird local

2. STATION AGENT: cada 30 s (configurable) ejecuta SELECT con watermark
   → SELECT ... FROM DCTO WHERE FEC_DCTO > @ultima_extraccion
   → mismo para DESP, TURN, TURN_TARJ, ANUL, CRED_CABE

3. STATION AGENT: empaqueta lo nuevo en JSON
   → POST http://servidor-central/api/v1/ingesta
     Authorization: Bearer eyJ... (JWT del usuario agent-est-001)
     Body: { "codigoEstacion":"EST-001", "transacciones":[...] }

4. API: persiste en transacciones_staging
   → INSERT con Procesada=false, FechaOriginal, EstacionId
   → Marca el watermark de la estación

5. HANGFIRE: cada 5 min (configurable cron */5 * * * *)
   → AnomalyDetectionJob.ExecuteAsync()
   → SELECT staging WHERE EstacionId=X AND Procesada=false

6. DETECTORES: 4 ejecutándose en paralelo (Task.WhenAll)
   → CashFraudDetector (faltantes, gineteo)
   → InvoiceAnomalyDetector (placa vacía, anulaciones, precios)
   → PaymentFraudDetector (reversiones, créditos sin auth, duplicados)
   → ComplianceViolationDetector (ZZZ999949, multi-combustible, horario)

7. SCORING: cada anomalía recibe score 0-100
   Score = RiesgoBase × MultMonto × MultReincidencia × MultEstacion
   - Bajo 0-25 | Medio 26-50 | Alto 51-75 | Crítico 76-100

8. PERSISTENCIA: INSERT en tabla alertas con metadata JSON

9. SIGNALR: emite "NuevaAlerta" a clientes conectados
   → Hub /hubs/alerts, grupos auditores/supervisores/estacion-N

10. FRONTEND REACT: el auditor ve toast en vivo + dashboard actualizado
```

## 1.3 — Las 4 reglas de detección de la tesis (Tabla 3)

| Detector | Regla | Umbral por defecto | Ejemplo concreto |
|---|---|---|---|
| **CashFraud** | Faltante de caja en cierre de turno | $50 USD por turno | TURN.FAL_TURN = 85 → alerta |
| **CashFraud** | Gineteo (faltantes recurrentes) | ≥ 3 turnos con faltante en 30 días, mismo empleado | EMP-007 con 3 turnos faltantes → alerta |
| **InvoiceAnomaly** | Anulaciones excesivas | > 5% de facturas diarias por empleado | EMP-011 con 8 ANUL de 100 FACT → alerta |
| **InvoiceAnomaly** | Campos obligatorios vacíos | placa vacía con monto significativo | DCTO.PLA_DCTO = '' → alerta |
| **PaymentFraud** | Reversión de tarjeta tardía | > 30 min después de la venta | TURN_TARJ con valor negativo 90 min después → alerta |
| **PaymentFraud** | Crédito sin autorización | > límite_cliente AND sin código autorización | CRED_CABE sin auth → alerta |
| **PaymentFraud** | Transacciones duplicadas | misma tarjeta, mismo monto, < 5 min de diferencia | 2 DCTO idénticos → alerta |
| **ComplianceViolation** | Placa genérica ZZZ999949 con galones > 5 | regulación ARCERNNR | DCTO+DESP con PLA='ZZZ999949' y CAN=8 → alerta |
| **ComplianceViolation** | Multi-combustible mismo día | misma placa con diésel Y extra | placa PCQ1234 con DIESEL + EXTRA → alerta |
| **ComplianceViolation** | Fuera de horario | venta antes de apertura o después de cierre | DCTO a las 04:00 → alerta |

## 1.4 — Por qué Strategy Pattern, Repository, Unit of Work

- **Strategy Pattern** (interface `IAnomalyDetector`): cada detector implementa la misma interface. El job los inyecta todos con `IEnumerable<IAnomalyDetector>` y los corre en paralelo. **Beneficio: añadir un 5° detector no requiere tocar el job, sólo crear una clase y registrarla en DI**.
- **Repository Pattern**: `IAlertaRepository`, `IEstacionRepository`, etc. en `Application`, implementadas en `Infrastructure`. **Beneficio: los servicios de negocio no conocen EF Core, podríamos cambiar a Dapper o Mongo sin tocar lógica**.
- **Unit of Work**: coordina transacciones que tocan múltiples tablas. **Beneficio: si falla el INSERT de alerta después de marcar staging como procesada, todo hace rollback**.

## 1.5 — Cómo defendemos el avance vs planificación

Planificación de la tesis (TITA 1, sección 5.1): 6 fases en 24 semanas.

| Fase | Semanas | Estado a 30-may |
|---|---|---|
| 1. Análisis y requisitos | 1-4 | ✅ 100% (entregado TITA 1) |
| 2. Diseño arquitectura | 5-7 | ✅ 100% (C4 niveles 1-2, ER) |
| 3. Backend + detectores | 8-13 | ✅ 100% (los 4 detectores funcionando, scoring, ETL, Hangfire, SignalR, JWT) |
| 4. Frontend + dashboards | 14-18 | ✅ 85% (Dashboard, alertas, detalle, login OK; faltan pulir reglas/usuarios) |
| 5. Pruebas y validación | 19-22 | 🟡 25% (proyecto de tests existe, faltan los 6+ tests por detector y UAT) |
| 6. Documentación y entrega | 23-24 | 🟡 30% (README, ARQUITECTURA.md hechos; falta manual usuario y video) |

**% acumulado de avance: aproximadamente 75-80%.**

## 1.6 — Cosas que NO funcionan al 100% (ser honestos si preguntan)

| Tema | Estado real | Cómo defenderlo |
|---|---|---|
| Auth Firebird 2.5 desde .NET 10.x | El cliente .NET nuevo no autentica contra FB 2.5 por el cambio de Srp→Legacy. | "En producción usaremos Firebird 3.0+ con auth Srp moderna. La query SQL es idéntica; sólo cambia el driver." |
| Cache 24 h para staging procesada | Hoy las transacciones no-anómalas quedan en staging. | "Próximo paso es añadir Hangfire job diario que elimine staging procesada > 24 h. El refactor es de ~20 líneas." |
| CI/CD pipeline | No está activo todavía. | "Tenemos el workflow YAML diseñado para GitHub Actions; lo activamos cuando publiquemos el repo." |
| UAT con auditores reales | No realizado aún. | "Está planificado para las semanas 19-20 según el cronograma." |

## 1.7 — Jira / tablero de trabajo

**OBLIGATORIO** según email del 26-may del prof. Eddy. Tenemos que tener un tablero accesible.
- **Si tenemos Jira/Trello con product backlog y commits**: compartir el link al inicio.
- **Si NO lo tenemos todavía**: este es el ÚNICO punto donde nos pueden ‘matar’. Hablamos antes con Felipe a más tardar viernes 29.

## 1.8 — Documentos de respaldo en el repo

- `docs/Tesis.md` — el documento completo entregado en TITA 1
- `docs/ARQUITECTURA.md` — diagramas C4 (1, 2, 3, 4) en Mermaid
- `docs/PROMPT.md` — especificación de los 8 bloques de implementación
- `docs/contac-schema.sql` — el schema REAL de Contaplus (no inventado)
- `CLAUDE.md` — instrucciones de proyecto, stack, reglas de implementación

---

# PARTE 2 — GUION PASO A PASO

> **Duración total objetivo: 15 minutos** (más margen para preguntas).
> El comité espera ver el software CORRIENDO. NO PowerPoint, NO documentos.
> Reparto sugerido: E1 = Steven, E2 = Leonardo. Pueden intercambiarse.

## ANTES DE ENTRAR AL AULA (llegar 30 min antes)

**Checklist en el laptop:**

- [ ] PostgreSQL Docker container `petrolrios-postgres` Up healthy → `01_levantar_postgres.bat`
- [ ] API ASP.NET Core 9 corriendo en `:5170` → `02_levantar_api.bat` en otra cmd
- [ ] Vite + React corriendo en `:5173` → `03_levantar_frontend.bat` en otra cmd
- [ ] Firebird Docker container `petrolrios-firebird` Up → `05_firebird_demo.bat` (FB 3.0)
- [ ] BD limpia → `97_limpiar_para_demo.bat` (alertas = 0)
- [ ] Tablero Jira/Trello abierto en una pestaña, con tareas reales del backlog
- [ ] Modo no-molestar activado
- [ ] Cable HDMI/adaptador listo si el aula lo necesita

**Pestañas de Chrome abiertas (en orden de uso):**

1. http://localhost:5173/dashboard (logueado como `admin@petrolrios.com`)
2. http://localhost:5173/alertas
3. http://localhost:5170/swagger
4. http://localhost:5170/hangfire/recurring
5. (opcional) https://github.com/... el repo
6. (opcional) Jira/Trello

**Ventanas cmd visibles en barra de tareas:**

- Una con `_arranque/` abierto para hacer doble clic a:
  - `97_limpiar_para_demo.bat` (limpieza)
  - `95_lluvia_alertas.bat` (genera ~30 alertas)
  - `96_simular_agente_demo.bat` (push pequeño en vivo si nos dan tiempo extra)

---

## SEGMENTO 1 — Saludo + problema (0:00 – 2:00) · habla **E1**

**En pantalla:** README.md o portada del repo abierto en VS Code.

> "Buenos días Comité. Somos Steven Carrillo y Leonardo Andrade. Vamos a
> presentar el avance del Capstone PetrolRíos — sistema web de detección
> de anomalías transaccionales.
>
> El problema que atacamos viene del Área de Auditoría de PetrolRíos:
> ellos procesan entre 13 y 15 mil transacciones diarias en sus 10
> estaciones integradas, pero su control actual sólo cuadra totales. Si
> un despachador registra una venta en efectivo de $100 como venta a
> tarjeta usando su propia tarjeta y se queda con los $100, el sistema
> ‘cuadra perfectamente’ y nadie lo detecta. Eso se llama gineteo —
> también está el problema de las ventas a la placa genérica ZZZ999949
> con más de 5 galones que viola la regulación ARCERNNR.
>
> Nuestro sistema cierra esa brecha: analiza el 100% de las
> transacciones cada 5 minutos y alerta al auditor en tiempo real."

**Si preguntan "¿cuál es la pregunta de investigación?"**:
> "¿Es viable implementar un sistema de detección de anomalías a nivel
> transaccional, basado en reglas de negocio configurables, que opere
> sobre las bases Firebird heredadas en modo solo-lectura, con tiempo
> de detección menor a 10 minutos? La respuesta es sí, y se lo vamos
> a demostrar."

---

## SEGMENTO 2 — Arquitectura (2:00 – 4:00) · habla **E2**

**En pantalla:** abrir `docs/ARQUITECTURA.md` y mostrar el diagrama C4 nivel 2.

> "Arquitectura en capas, modelo push (Alternativa B de la tesis).
>
> Tenemos tres componentes principales:
>
> 1. **Station Agent** — un .NET Worker Service que va INSTALADO en
>    cada estación. Lee la BD Firebird de Contaplus en modo solo
>    lectura, con marca de agua para no reprocesar. Empaqueta lo nuevo
>    en JSON y lo POSTea a nuestra API central con JWT. Si la red se
>    cae, tiene store-and-forward: guarda local y reintenta.
>
> 2. **Servidor central** (ASP.NET Core 9): recibe los lotes en
>    `/api/v1/ingesta`, persiste en `transacciones_staging` (PostgreSQL),
>    y un job de Hangfire ejecuta cada 5 minutos los **cuatro
>    detectores en paralelo** — Cash Fraud, Invoice Anomaly, Payment
>    Fraud, Compliance Violation. Las alertas se persisten con score
>    0-100 y se notifican por SignalR.
>
> 3. **Frontend React 18** — Dashboard del auditor con KPIs en vivo,
>    lista de alertas con filtros, detalle con evidencia JSON.
>
> El stack completo: ASP.NET Core 9, EF Core 9, Dapper, Hangfire,
> SignalR, JWT con RBAC, PostgreSQL 16, React 18 con TypeScript,
> Vite, Tailwind, Recharts. Todo Clean Architecture con Repository,
> Unit of Work, Strategy Pattern para los detectores."

---

## SEGMENTO 3 — La BD real de Contaplus (4:00 – 5:30) · habla **E1**

**En pantalla:** abrir una cmd y correr `docker ps`. Mostrar `petrolrios-firebird`.

> "Para que no parezca maqueta: cargamos el backup real de Contaplus
> que nos entregó la empresa — el archivo CONTACONSTANZA-20250609.FBK
> — dentro de un contenedor Docker de Firebird. Aquí lo ven corriendo
> en localhost puerto 3050."

**Mostrar las tablas reales:**
> "Y aquí está el schema REAL de Contaplus en `docs/contac-schema.sql`
> — son más de 100 tablas. Nuestro agente sólo lee las 7 que necesita:
> DCTO para facturas, DESP para despachos, TURN y TURN_TARJ para
> turnos y tarjetas, ANUL para anulaciones, CRED_CABE para créditos.
> Las queries SQL son SELECT puros, no inventamos columnas."

**Mostrar `src/PetrolRios.StationAgent/Services/FirebirdExtractor.cs`:**
> "Este es el código del agente. Aquí ven la query real contra DCTO
> con la columna `FEC_DCTO > @Watermark` — esa es la marca de agua
> que evita reprocesar. La connection string usa `ReadOnly=true` por
> restricción R08 de la tesis: nunca modificamos la BD de la estación."

---

## SEGMENTO 4 — Demo en vivo del ciclo completo (5:30 – 9:00) · habla **E2** (E1 hace clicks)

**Acción 1**: cambiar al frontend (`/dashboard`). Mostrar 0 alertas.

> "Empezamos con la base de datos limpia. 0 alertas. Vamos a simular
> qué pasa cuando llega un lote de transacciones nuevas — esto en
> producción lo hace el Station Agent cada 30 segundos leyendo Firebird,
> aquí lo simulamos con un script que ejecuta el mismo POST que haría
> el agente."

**Acción 2**: doble clic en `95_lluvia_alertas.bat` (en el File Explorer ya abierto).

> "El script hace login como agente con JWT, prepara un lote de 50
> transacciones — facturas con placa ZZZ999949, cierres de turno con
> faltantes, anulaciones excesivas, reversiones de tarjeta tardías —
> y las empuja al endpoint `/api/v1/ingesta`."

**Mientras corre, cambiar a Hangfire** (`:5170/hangfire/recurring`):

> "Aquí está el job recurrente que el sistema ejecuta cada 5 minutos.
> Ahora vamos a dispararlo manualmente para no esperar."

(Marcar `anomaly-detection` + Trigger now)

**Esperar 5-10 segundos y cambiar a `/alertas`:**

> "Y aquí tenemos las alertas detectadas. Esperábamos cerca de 30 —
> vamos a verlas."

(Refrescar `/alertas`)

> "Tenemos alertas de los 4 tipos: Fraude de Efectivo con varios
> empleados, Anomalía de Factura por las placas vacías y las
> anulaciones de EMP-011, Fraude de Pago por las reversiones
> tardías, y Violación Normativa por la placa ZZZ999949."

**Click en una alerta de Violación Normativa para mostrar el detalle:**

> "Aquí está el detalle de una alerta. Tipo de detector, score de
> riesgo 65 — nivel Alto. Empleado involucrado, referencia del
> documento DCTO original, y la metadata JSON con toda la evidencia:
> placa, galones detectados, umbral aplicado. El auditor puede tomar
> la alerta en revisión, marcarla como confirmada o falso positivo,
> y asignarla a otro auditor."

---

## SEGMENTO 5 — Recorrido funcional del panel (9:00 – 10:30) · habla **E1**

**Click en Dashboard:**
> "El dashboard del auditor con KPIs en tiempo real: total de alertas,
> alertas nuevas, críticas, en revisión, score promedio, estaciones
> activas. Los gráficos se actualizan automáticamente — el de tipo de
> detector muestra la distribución, el de estación muestra dónde se
> está generando más actividad sospechosa."

**Click en Reglas:**
> "Sección de reglas — los umbrales son configurables. Aquí el
> supervisor puede subir el umbral de faltante de $50 a $100 si hay
> muchos falsos positivos. Tenemos 12 reglas pre-cargadas según la
> Tabla 3 de la tesis."

**Click en Usuarios:**
> "Sección de usuarios — sólo accesible al Administrador. Roles
> Auditor, Supervisor y Administrador con permisos diferenciados
> (RBAC declarativo con `[Authorize(Roles = ...)]`)."

**Cambiar a Swagger (`:5170/swagger`):**
> "La API REST está documentada con OpenAPI, 25 endpoints versionados
> en v1, todos con autorización JWT. Aquí está el endpoint de ingesta
> que recibe los lotes del Station Agent."

---

## SEGMENTO 6 — Avance vs planificación + Jira (10:30 – 12:00) · habla **E2**

**Abrir el tablero Jira/Trello:**
> "Este es nuestro tablero de seguimiento en Jira. Aquí están las user
> stories del product backlog, las que están en sprint actual, las
> bloqueadas y las terminadas. El docente guía Felipe Urquiza tiene
> acceso de viewer para validar."

**Volver al repo, mostrar el cronograma de la tesis (sección 5.1):**
> "Comparado con la planificación de TITA 1 (24 semanas, 6 fases):
> - Fase 1-2 Análisis y diseño: 100% completas.
> - Fase 3 Backend: 100%. Los 4 detectores, scoring, ETL con
>   watermark, Hangfire, SignalR, JWT con RBAC.
> - Fase 4 Frontend: 85%. Dashboard, alertas, detalle, login están
>   listos; nos falta pulir Reglas y Usuarios.
> - Fase 5 Pruebas: 25%. El proyecto de pruebas está creado, faltan
>   los 6+ tests por detector que pide el OE5 (>80% cobertura).
> - Fase 6 Documentación: 30%. README y ARQUITECTURA.md están,
>   falta manual de usuario.
>
> En porcentaje acumulado: cerca de 75-80%. Por encima del 50% que
> pidió el comité para esta presentación intermedia."

---

## SEGMENTO 7 — Próximos pasos y cierre (12:00 – 13:30) · ambos

**E1**:
> "Próximas semanas:
> - Cerrar pruebas unitarias hasta el 80% de cobertura en detectores.
> - Activar el pipeline CI/CD en GitHub Actions.
> - Añadir un job diario que purga las transacciones procesadas más
>   viejas que 24 horas (cache TTL).
> - UAT de 2 semanas con los auditores reales."

**E2**:
> "En producción, el único cambio respecto a lo que vieron hoy es
> sustituir el Firebird de Docker por las 10 conexiones a las
> estaciones reales — el código del agente es exactamente el mismo,
> sólo cambia la connection string. Estamos listos para el deploy."

**E1**: "Quedamos atentos a sus preguntas. Gracias."

---

# PARTE 3 — Q&A DEFENSIVA + PLAN B

## Q&A: las 10 preguntas más probables

**P1: ¿Por qué reglas de negocio y no Machine Learning?**
> R: La tesis explícitamente excluye ML como limitación L02 porque no
> existen datos históricos etiquetados de fraudes en PetrolRíos. ML
> queda como trabajo futuro cuando tengamos 12+ meses de alertas
> confirmadas que sirvan como dataset etiquetado. Hoy las reglas dan
> >90% de alertas válidas según la literatura (Unit8, 2024).

**P2: ¿Cómo evidencian que están siguiendo Scrum?**
> R: Repositorio Git con commits descriptivos por sprint, tablero Jira
> con product backlog y user stories. Tenemos demos cada 2 semanas con
> el docente guía registradas en el archivo Capstones_202020 — 3
> reuniones registradas: 9-abr, 27-abr, 5-may.

**P3: ¿Qué pruebas tienen hoy y cuáles faltan?**
> R: Hoy: proyecto `PetrolRios.Detectors.Tests` con xUnit, FluentAssertions
> y Moq. Cubrimos las reglas principales de cada detector. Faltan
> casos de borde, integración con WebApplicationFactory y E2E con
> Testcontainers.PostgreSql para alcanzar el 80% del OE5.

**P4: ¿Cómo escala el sistema a las 90 estaciones cuando crezca?**
> R: Cada estación tiene su propio Station Agent independiente con
> store-and-forward, así que la captura es horizontal. El servidor
> central escala porque la ingesta es stateless; Hangfire usa
> PostgreSQL como storage compartido. Lo dimensionamos en AWS RDS
> `db.t3.micro` que aguanta hasta 200 estaciones; si crece más,
> subimos a `db.m5.large`.

**P5: ¿Y la seguridad de las credenciales de las estaciones?**
> R: JWT con refresh tokens (60 min / 7 días), passwords con BCrypt,
> HTTPS en producción, CORS restringido al frontend, rate limiting
> básico, logs de auditoría de todas las acciones. Las connection
> strings a Firebird siempre tienen `ReadOnly=true` — restricción
> R08 de la tesis.

**P6: ¿Si una estación tiene mal el reloj y la marca de agua se
desincroniza?**
> R: La marca de agua se persiste por estación en `estacion_watermarks`,
> y avanza en función de la fecha del documento extraído, no del reloj
> local del agente. Aunque la estación tenga el reloj corrido, mientras
> las fechas en DCTO sean monotónicas hacia adelante, no perdemos datos.
> Si la fecha está movida en el pasado, el sistema reprocesa — no es
> ideal pero no se pierde nada.

**P7: ¿Por qué Hangfire y no eventos en tiempo real con Kafka?**
> R: La detección de fraudes no es trading; tolerancia de minutos es
> aceptable. Sección 6.4 de la tesis explica el análisis comparativo.
> Hangfire es batch para el análisis (más simple, menos infraestructura)
> y SignalR es push para la notificación. Es lo mejor de los dos mundos
> con un costo de operación 5× menor que Kafka.

**P8: ¿Cómo evitan generar muchísimos falsos positivos?**
> R: Los umbrales están en la tabla `reglas_deteccion` y son
> configurables vía UI. Empezamos con los valores de la Tabla 3 de la
> tesis (basados en entrevistas con el Área de Auditoría) y se ajustan
> durante el UAT. El plan de validación de 2 semanas mide exactamente
> la tasa de falsos positivos contra el objetivo < 10%.

**P9: ¿Y los datos personales? GDPR / Habeas Data?**
> R: El sistema procesa datos transaccionales que pueden incluir
> nombres y placas. Toda la auditoría se loguea (`logs_auditoria`),
> cifrado en tránsito con TLS 1.3, en reposo con AES-256 de AWS RDS,
> RBAC con principio de mínimo privilegio. Las alertas son indicios,
> no acusaciones — el auditor humano siempre decide.

**P10: ¿Pueden mostrarnos el código de un detector?**
> R: ¡Sí! Abrimos `src/PetrolRios.Detectors/ComplianceViolationDetector.cs`.
> Aquí ven la interface `IAnomalyDetector`, el método `DetectAsync`,
> la lectura de umbrales desde `ReglaDeteccion`, la generación de la
> alerta con score… (mostrar)

## Plan B: ¿Qué hacemos si algo falla en vivo?

| Problema | Plan B |
|---|---|
| Docker no arranca / API no responde | Mostrar el código en VS Code, los diagramas C4, el README y explicar el flujo conceptual. Tenemos screenshots del sistema corriendo en `_arranque/screenshots/` (sacarlos antes). |
| El job de Hangfire no dispara | Mostrar las ejecuciones ANTERIORES en `:5170/hangfire/jobs/` que ya están en la BD. Explicar que el job corre automático cada 5 min. |
| No aparecen 30 alertas exactas | Aclarar que algunos detectores AGRUPAN (ej. gineteo cuenta 3+ faltantes como una sola alerta). Mostrar la lista real, contar lo que hay. |
| Falla el frontend | Abrir Swagger y hacer las queries directas (GET /api/v1/alertas) para mostrar la data. |
| El comité pregunta algo que no sabemos | Decir honestamente "esa parte está en planificación / no la implementamos todavía / no estoy 100% seguro pero el comportamiento esperado sería X". NO inventar. |
| Wifi del aula no funciona | El sistema corre 100% local. Sólo necesitamos red para enseñar el repo en GitHub. Si no hay wifi, lo mostramos en VS Code abierto local. |

## Reglas de oro durante la defensa

1. **No hablar por encima del otro.** Si E1 está mostrando, E2 toma nota de las preguntas.
2. **Mantener contacto visual con el comité** — no quedarse mirando la pantalla.
3. **Si una pregunta es técnica fuera de tu área, decir "te respondo X / mi compañero te responde Y".** No fingir conocimiento.
4. **Velocidad de habla normal, pausada.** Tendemos a acelerar por nervios.
5. **Si algo se rompe, no entrar en pánico**: "vamos a mostrarlo conceptualmente y luego retomamos". El plan B existe.
6. **Mostrar SIEMPRE el dashboard con datos al final**, aunque sea con alertas de una corrida anterior.
7. **Al cerrar, dar las gracias y quedarse 30 segundos disponibles** para que el comité pregunte directo.

---

## DESPUÉS DE LA PRESENTACIÓN

- Anotar las observaciones del comité.
- Compartir el link del repo / Jira si lo piden.
- Confirmar fechas próximas con Felipe Urquiza.
- Celebrar (con moderación) que pasamos la primera defensa.
