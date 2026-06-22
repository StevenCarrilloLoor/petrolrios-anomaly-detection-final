# Guion de presentación — 15 min · Sesión de revisión TITA 2
**PetrolRíos — Sistema de Detección de Anomalías Transaccionales**
Steven Carrillo + Leonardo Andrade · Docente guía: Felipe Urquiza · TITA 2 — Eddy Mauricio Armas

> El profesor pidió **"evidenciar las funcionalidades que su producto de software tiene en este momento versus su planificación, así que necesito revisar su software en ejecución, no PowerPoints o documentos"**.
> Todo el guion está pensado para hablar con la pantalla compartida.

---

## ANTES DE LA REUNIÓN (5 min)

Tener corriendo **3 ventanas cmd visibles** y **3 pestañas de Chrome**:

| Ventana | Cómo arrancar | Qué se ve |
|---|---|---|
| PostgreSQL | `01_levantar_postgres.bat` | Container `Up (healthy)` |
| API .NET | `02_levantar_api.bat` | Logs de Hangfire, detectores |
| Frontend Vite | `03_levantar_frontend.bat` | "VITE ready" + URL |
| Firebird | `05_firebird_demo.bat` | Container `Up (healthy)` con CONTACONSTANZA |

| Pestaña Chrome | URL |
|---|---|
| Tab 1 | http://localhost:5173/dashboard (logueado como admin) |
| Tab 2 | http://localhost:5170/swagger |
| Tab 3 | http://localhost:5170/hangfire/recurring |

Dejar abierto el **explorador de archivos** en `_arranque/` para hacer doble clic en `96_simular_agente_demo.bat` en vivo.

> Justo antes del demo, correr **`97_limpiar_para_demo.bat`** para empezar con `alertas = 0`.

---

## ESTRUCTURA DEL GUION (15:00 total)

| Bloque | Tiempo | Quién | Tema |
|---|---|---|---|
| 1 | 0:00–1:30 | E1 | Saludo + problema + objetivo |
| 2 | 1:30–3:30 | E2 | Arquitectura: del problema al sistema |
| 3 | 3:30–6:00 | E1 | Demo: la BD real de Contaplus + el agente |
| 4 | 6:00–9:00 | E2 | Demo: ciclo end-to-end (push → detector → alerta) |
| 5 | 9:00–11:00 | E1 | Recorrido funcional: dashboard, alertas, detalle |
| 6 | 11:00–12:30 | E2 | Estado del avance vs planificación TITA 1 |
| 7 | 12:30–14:00 | ambos | Próximos pasos y cache 24h |
| 8 | 14:00–15:00 | ambos | Preguntas defensivas listas |

---

## BLOQUE 1 — Intro y problema (1:30) · E1
**Pantalla:** README.md o portada del repo.

> "Buenas tardes profesor Armas. Somos Steven Carrillo y Leonardo Andrade.
> Vamos a mostrarle el avance de nuestro proyecto Capstone: un sistema web
> de detección de anomalías transaccionales para PetrolRíos S.A.
>
> El problema que atacamos: PetrolRíos procesa entre 13.000 y 15.000
> transacciones diarias en sus 10 estaciones integradas. Su control actual
> sólo cuadra totales — no analiza las transacciones individuales — así que
> los fraudes que se ocultan en operaciones que numéricamente cuadran pasan
> desapercibidos. Casos como el gineteo, las ventas en efectivo registradas
> como tarjeta, o las violaciones a la regulación de ARCERNNR como la placa
> genérica ZZZ999949 con más de 5 galones, simplemente no se detectaban.
>
> Nuestro sistema cierra esa brecha: analiza el 100% de las transacciones
> cada 5 minutos y notifica al auditor en tiempo real."

---

## BLOQUE 2 — Arquitectura (2:00) · E2
**Pantalla:** abrir `docs/ARQUITECTURA.md` (o el diagrama C4 nivel 2).

> "La arquitectura sigue la Alternativa B de la tesis: un sistema en capas
> con modelo push. Cada estación tiene un **Station Agent** —un Worker
> Service en .NET 9— que se conecta en modo solo lectura a la base
> **Firebird** del Contaplus de la estación. Lee con marca de agua —no
> reprocesa— y empuja lotes JSON a nuestra **API ASP.NET Core 9** vía
> POST `/api/v1/ingesta` autenticado con **JWT**.
>
> En el servidor central, las transacciones llegan en crudo a una tabla
> de staging en **PostgreSQL 16**. Cada 5 minutos un job de **Hangfire**
> levanta esos datos y ejecuta los **4 detectores** en paralelo —Cash
> Fraud, Invoice Anomaly, Payment Fraud y Compliance Violation— bajo
> **Strategy Pattern**. Las anomalías reciben un score de 0 a 100 vía un
> motor de scoring, se persisten como alertas, y se notifican en tiempo
> real al frontend React con **SignalR**.
>
> Stack completo: ASP.NET Core 9, EF Core 9, Dapper, Hangfire, SignalR,
> JWT con RBAC, React 18 con TypeScript, Vite, Tailwind y Recharts.
> Todo siguiendo Clean Architecture, Repository, Unit of Work."

---

## BLOQUE 3 — La BD real de Contaplus + el agente (2:30) · E1
**Pantalla:** ventana cmd con `docker ps`, luego `docs/contac-schema.sql`.

> "Para validar que esto no es maqueta, restauramos el backup real de
> Contaplus —el archivo CONTACONSTANZA-20250609.FBK que nos pasó la
> empresa— dentro de un contenedor Docker de Firebird 2.5."

*(mostrar `docker ps` con `petrolrios-firebird Up (healthy) :3050`)*

> "El agente conecta a `localhost:3050` exactamente como conectaría al
> Firebird de una estación a través de la VPN. Lee de las tablas reales:
> DCTO para facturas, DESP para despachos, TURN y TURN_TARJ para turnos
> y tarjetas, ANUL para anulaciones, CRED_CABE para créditos."

*(mostrar `docs/contac-schema.sql` con CREATE TABLE DCTO y FACT)*

> "Las consultas son SQL real contra el esquema real de Contaplus, no
> nombres inventados. Eso es lo que asegura que cuando lo despleguemos
> contra una estación de verdad, no necesitemos cambiar el código."

---

## BLOQUE 4 — Ciclo end-to-end en vivo (3:00) · E2
**Pantalla:** alternar entre Hangfire, frontend `/alertas`, y la cmd que ejecuta el script.

> "Veamos el ciclo completo en tiempo real. Empecemos con la BD limpia."

*(en cmd correr `97_limpiar_para_demo.bat`, mostrar `alertas = 0`)*

> "Ahora simulemos lo que pasa cuando llegan tres transacciones nuevas en
> la estación EST-001."

*(doble clic en `96_simular_agente_demo.bat`)*

> "El script hace login JWT como `agent-est-001@petrolrios.com`, prepara
> un lote con 3 transacciones (una factura con placa ZZZ999949 con 9
> galones, su detalle de despacho, y un cierre de turno con faltante de
> 95 dólares) y los POSTea al endpoint `/api/v1/ingesta`."

*(cuando el script imprime "Lote empujado", cambiar a Hangfire)*

> "El job recurrente 'anomaly-detection' se dispara —pueden ver la
> ejecución en Hangfire— corre los 4 detectores en paralelo, y el motor
> de scoring asigna riesgo Alto a la violación ARCERNNR y Medio al
> faltante de efectivo."

*(cambiar al frontend `/alertas` y refrescar)*

> "En el panel del auditor aparecen las 2 alertas: una de Violación
> Normativa por la placa genérica, y una de Fraude de Efectivo por el
> faltante en el turno. Cada una con su score y su evidencia."

---

## BLOQUE 5 — Recorrido funcional (2:00) · E1
**Pantalla:** frontend, recorrer Dashboard → Alertas → Detalle → Swagger.

> "El dashboard del auditor muestra KPIs en vivo: total de alertas,
> alertas nuevas, críticas, en revisión, score promedio."

*(clic en Dashboard, leer los números)*

> "La lista de alertas tiene filtros por tipo, nivel y estado. Cada
> alerta lleva su score, fecha, estación y empleado involucrado."

*(clic en Alertas, mostrar filtros)*

> "Al abrir el detalle vemos el metadata JSON completo con la evidencia:
> número de documento, galones, umbral aplicado. El auditor puede tomar
> la alerta en revisión, marcarla como confirmada o falso positivo, y
> asignarla a otro auditor."

*(abrir el detalle de la alerta de ZZZ999949)*

> "La API REST está documentada en Swagger con 25 endpoints versionados
> en v1, todos con autorización RBAC por 3 roles: Auditor, Supervisor,
> Administrador."

*(cambiar a Tab Swagger, mostrar la lista de endpoints)*

---

## BLOQUE 6 — Avance vs planificación TITA 1 (1:30) · E2
**Pantalla:** abrir `docs/Tesis.md` y saltar al cronograma (sección 5.1).

> "En la planificación de TITA 1 dividimos el proyecto en 6 fases sobre
> 24 semanas. A la fecha tenemos:
>
> - **Fase 3 - Backend** (semanas 8-13, 6 semanas): completa. Los 4
>   detectores con Strategy Pattern, motor de scoring, ETL con
>   watermark, Hangfire + SignalR, JWT con RBAC.
> - **Fase 4 - Frontend** (semanas 14-18, 5 semanas): completa al 80%.
>   Dashboard, lista de alertas, detalle, login JWT, integración SignalR.
> - **Fase 5 - Pruebas** (semanas 19-22): iniciada. Pruebas unitarias
>   de los 4 detectores con xUnit + FluentAssertions + Moq apuntando al
>   >80% de cobertura que pide el OE5.
> - **Próximas semanas**: ampliar pruebas, automatizar pipeline CI/CD
>   en GitHub Actions, validación con datos reales, manuales."

---

## BLOQUE 7 — Próximos pasos y cache 24h (1:30) · ambos
**E2:** "Lo que sigue inmediatamente:"
> 1. **Cache de 24 horas**: hoy las transacciones que NO producen alerta
>    quedan en staging. Vamos a añadir un Hangfire job diario que
>    elimine staging procesada más vieja que 24 h. Las alertas confirmadas
>    quedan permanentes; los datos sin anomalía se purgan automáticamente.
> 2. **Pipeline CI/CD**: workflow en GitHub Actions con build + test
>    automático en cada push.
> 3. **UAT con auditores**: 2 semanas de operación paralela con datos
>    reales para validar la tasa de alertas válidas > 90% que pide la
>    tesis.

**E1:** "Y en producción, el único cambio respecto a lo que ves hoy es
sustituir el Firebird de Docker por las 10 conexiones VPN a las
estaciones reales — el código es idéntico."

---

## BLOQUE 8 — Preguntas defensivas (1:00)

Respuestas listas si el profesor pregunta:

**P: ¿Cómo evidencias la metodología?**
> R: Repositorio Git con commits por bloque (Bloque 1 a 7 según
> `docs/PROMPT.md`). Tablero Trello/Jira con product backlog y user
> stories. Demos cada 2 semanas con el guía Felipe Urquiza (3 reuniones
> registradas: 9-abr, 27-abr, 5-may en la planilla Capstones_202020).

**P: ¿Y las pruebas?**
> R: xUnit + FluentAssertions + Moq. Proyecto `PetrolRios.Detectors.Tests`
> apuntando a >80% de cobertura — requisito OE5 de la tesis. Estrategia
> basada en ISO/IEC 29119: unitarias, integración y E2E con
> WebApplicationFactory + Testcontainers.

**P: ¿Por qué reglas y no Machine Learning?**
> R: La tesis explícitamente excluye ML (limitación L02) porque no
> existen datos etiquetados de fraudes en PetrolRíos. ML quedó como
> trabajo futuro cuando haya 12+ meses de datos etiquetados.

**P: ¿Por qué Hangfire y no eventos en tiempo real?**
> R: Latencia de minutos es aceptable para auditoría —no es trading—.
> Análisis comparativo en sección 6.4 de la tesis. Hangfire + SignalR
> combina batch para el análisis + push para la notificación.

**P: ¿Cómo escala a las 90 estaciones?**
> R: Cada estación corre su propio Worker Agent independiente con
> store-and-forward; si el servidor central no responde, guarda local
> y reintenta. El servidor central escala horizontalmente porque la
> ingesta es stateless y Hangfire usa storage en PostgreSQL.

**P: ¿Y la seguridad?**
> R: JWT con refresh tokens (60 min/7 días), RBAC con 3 roles, BCrypt
> para passwords, HTTPS en producción, CORS restringido al frontend,
> rate limiting, logs de auditoría. Las BDs Firebird siempre con
> `ReadOnly=true` por restricción R08 de la tesis.

---

## CIERRE (10 s)

> **E1:** "Eso es el avance a la fecha. Estamos a tiempo para la
> entrega final."
>
> **E2:** "Quedamos atentos a sus observaciones. Muchas gracias."

---

## CHECKLIST FINAL ANTES DE CONECTAR AL TEAMS

- [ ] Las 3 cmds (Postgres, API, Frontend) corren sin errores
- [ ] Firebird container `petrolrios-firebird` Up (healthy)
- [ ] Frontend en http://localhost:5173 muestra el dashboard logueado
- [ ] Swagger y Hangfire abiertos en pestañas
- [ ] Corrí `97_limpiar_para_demo.bat` (alertas = 0)
- [ ] `96_simular_agente_demo.bat` está en File Explorer listo para
      doble clic
- [ ] Modo No-Molestar activado (no notificaciones de WhatsApp, mail)
- [ ] Cronómetro a la vista (3 min por bloque promedio)
