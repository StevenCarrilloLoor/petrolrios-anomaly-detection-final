# Registro de mejoras post-revisión del jurado

Fecha: junio 2026. Versión del sistema: 2.0.

Este documento resume todo lo agregado, modificado y eliminado tras la observación del
jurado de que el aplicativo debía ser más completo y la interfaz sustancialmente mejor.
Cada cambio está vinculado a la sección de la tesis que lo respalda.

---

## 1. Reglas de detección

### 1.1 Reglas eliminadas / desactivadas

| Regla | Decisión | Justificación |
|---|---|---|
| Operación fuera de horario | **Desactivada por defecto** (`Activa = false`, umbral 0) | Las estaciones de PetrolRíos operan 24/7, por lo que generaba falsos positivos sin valor. Se conserva como regla configurable por estación (no contradice la Tabla 2 de la tesis) y es defendible ante el jurado: "configurable, apagada porque operamos 24/7". |

Además se corrigió un defecto: **los detectores ahora respetan el flag `Activa`** de cada
regla. Antes, una regla desactivada en la base de datos seguía ejecutándose con su valor
por defecto.

### 1.2 Reglas nuevas (7)

Todas configurables desde la pantalla de Reglas (umbral editable, activable/desactivable).

**Cash Fraud (+2):**

1. **Venta a crédito sin cliente identificado** (`CreditoSinClienteHabilitado`).
   Un crédito sin deudor identificable es incobrable: sugiere venta en efectivo registrada
   como crédito para retener el dinero. *Fuente: Tesis Tabla 2 — "Ventas en efectivo
   registradas como crédito".*
2. **Proporción atípica de efectivo corporativo** (`EfectivoCorporativoPorcentajeUmbral`,
   defecto 30%). Vendedor con % de ventas en efectivo sobre clientes corporativos por
   encima del umbral. *Fuente: caso de estudio documentado (enero 2026, Tablas 4–5 de la
   tesis): el cliente investigado pagaba 88.4% con tarjeta y el despachador con 79.5% de
   contado resultó el de nivel crítico.*

**Invoice Anomaly (+2):**

3. **Descuento excesivo fuera de política** (`DescuentoPorcentajeMaximo`, defecto 10%).
   *Fuente: Tesis Tabla 3 — "descuento_aplicado > descuento_máximo_permitido". Estaba en
   la tesis pero NO estaba implementada.*
4. **Total de factura inconsistente** (`TotalInconsistenteHabilitado`). Verifica que
   `total = subtotal − descuento + IVA` (tolerancia $0.05). Indicador clásico de
   manipulación documental en técnicas de interrogación de archivos. *Fuente: Tesis
   sección 6 (interrogación de archivos).*

**Payment Fraud (+1):**

5. **Despachos rápidos sucesivos** (`DespachosRapidosMinutosUmbral`, defecto 10 min,
   mínimo 3 transacciones consecutivas del mismo cliente). *Fuente: caso de estudio
   documentado: 33.9% de las transacciones investigadas ocurrieron con < 10 minutos entre
   despachos — patrón físicamente improbable que sugiere facturación ficticia.*

**Compliance Violation (+1):**

6. **Venta sin placa en monto mayor** (`VentaSinPlacaMontoMinimo`, defecto $200).
   *Fuente: Tesis Tabla 2 — "Ventas sin placa en montos mayores". Estaba en la tesis pero
   NO estaba implementada.*

(7.ª: la regla de horario reconfigurada como opt-in cuenta como cambio de regla.)

### 1.3 Corrección de bug en filtros

El filtro por tipo de detector en la lista de alertas no funcionaba: el frontend enviaba
`tipoDetector` y el backend esperaba `tipo`. Corregido en `alertas.service.ts`.

---

## 2. Funcionalidad nueva del aplicativo

| Funcionalidad | Caso de uso | Detalle |
|---|---|---|
| **Comentarios de auditoría** | CU-07 | Nueva entidad `ComentarioAlerta` (fecha, hora, autor), endpoints `GET/POST /api/v1/alertas/{id}/comentarios`, hilo de comentarios en el detalle de alerta. |
| **Reportes PDF y Excel** | CU-12 | Nuevo `ReportesController` + `ReporteService` (QuestPDF y ClosedXML). PDF apaisado con resumen por nivel y paginación; Excel con autofiltro, hoja de resumen y filas críticas resaltadas. Página nueva "Reportes" para Supervisor/Administrador. |
| **Métricas y KPIs ejecutivos** | CU-13 | Nuevos endpoints: tendencia diaria (serie continua), alertas por nivel, top empleados con más alertas, métricas de resolución (tiempo medio, tasa de falsos positivos, **tasa de alertas válidas — el indicador del OE2 > 90%**). |
| **Asignación de alertas usable** | CU-11 | Antes había que escribir el ID del auditor a mano. Ahora: endpoint `GET /usuarios/auditores` (accesible a Supervisor) y selector con nombres en la UI. Se excluyen los usuarios-agente de estación. |
| **Fecha de resolución** | CU-13 | Nueva columna `FechaResolucion` en `Alerta`; se registra al confirmar/cerrar/marcar falso positivo. Permite medir el tiempo de resolución real. |

Requiere una migración EF nueva: `AgregarComentariosYFechaResolucion`
(la genera `scripts/verificar-mejoras.bat`).

---

## 3. Interfaz (rediseño completo)

**Dashboard → "Centro de Monitoreo":**
indicador "Monitoreo activo" con pulso, 8 KPIs con subtítulos contextuales y acento de
color para críticas, 3 tarjetas de métricas de efectividad (tiempo medio de resolución,
tasa de alertas válidas vs meta OE2, pendientes/resueltas), gráfico de **tendencia de 14
días** (área con gradiente, total vs críticas), donut por tipo de detector, barras
horizontales por nivel de riesgo con colores semánticos, barras por estación y **tabla de
ranking de empleados** con píldoras de score.

**Alertas:** filtros completos (tipo, nivel, estado, **estación**, **rango de fechas**) con
botón "limpiar", contador de resultados, barra visual de score por fila, columna de
empleado, skeleton loaders y estado vacío ilustrado.

**Detalle de alerta:** gauge circular de score, **la metadata JSON cruda se reemplazó por
una grilla "Evidencia de la detección"** con etiquetas en español, selector de auditor con
nombres (antes input numérico), hilo de comentarios con avatares e historial, y acciones de
clasificación.

**Login:** pantalla dividida con panel institucional (cobertura 100%, SignalR en tiempo
real, detección < 10 min) — presenta el proyecto ante el jurado desde el primer segundo.

**Navegación:** sidebar reorganizado en secciones (Monitoreo / Gestión / Administración),
nueva entrada "Reportes", branding renovado.

**Componentes nuevos:** `Card`, `Skeleton`, `EmptyState`.

---

## 4. Pruebas

- Nuevo archivo `NuevasReglasDetectorTests.cs`: 15 tests que cubren las 6 reglas nuevas
  (caso positivo y negativo), el respeto al flag `Activa` (incluida la regla de horario
  desactivada — el caso que mencionó el jurado) y la robustez ante lotes duplicados.
- `TestHelpers` ampliado: facturas con cliente/descuento/subtotal/IVA configurables y
  helper de reglas inactivas.
- Resultado verificado: **build 0 errores / 0 warnings; 76+ tests en verde**
  (Domain 4, Detectors 60+, Api 12; los 13 de integración con BD siguen marcados skip).
- La cobertura de `PetrolRios.Detectors` se mantiene > 80% (requisito OE5).

---

## 4.1 Correcciones encontradas durante la prueba en vivo (implementación real)

La prueba end-to-end se hizo con el pipeline REAL: backup `CONTACONSTANZA-20250609.FBK`
(40,021 facturas reales de Contaplus) restaurado en Firebird 3.0 (Docker, puerto **3051**)
→ Station Agent con watermark → JWT → `POST /api/v1/ingesta` → Hangfire → detectores →
dashboard. Durante esa prueba se encontraron y corrigieron 5 defectos reales:

1. **Auth de Firebird:** el cliente .NET fallaba porque el Firebird 2.5 local de Windows
   ocupa el puerto 3050; el contenedor se movió al 3051 y se creó el usuario SYSDBA (Srp)
   en `security3.fdb`. Scripts: `05_firebird_demo.bat`, `fix_firebird_auth.bat`.
2. **Ingesta 500:** las fechas de Firebird llegan con `Kind=Unspecified` y Npgsql exige
   UTC. Corregido en `IngestaService` (conversión explícita a UTC).
3. **Lotes duplicados:** si un envío falla a mitad de camino, el store-and-forward
   reenvía el lote y se duplican filas en staging. El `ToDictionary` del
   `PaymentFraudDetector` lanzaba excepción y tumbaba el ciclo completo. Corregido:
   dedup por (`TipoTransaccion`,`DataJson`) en el job + `GroupBy` tolerante en el detector
   + test de regresión.
4. **Staging consumido por ciclos fallidos:** un ciclo que fallaba dejaba las filas
   marcadas como procesadas. Mitigado con el dedup y el manejo del punto 3.
5. **Job con reglas pre-filtradas:** el job cargaba solo reglas activas
   (`GetActivasAsync`), por lo que una regla desactivada volvía a ejecutarse con su valor
   por defecto. Ahora carga todas (`GetAllAsync`) y los detectores deciden por `Activa`.

**Resultado final verificado en vivo:** 12 ventas anómalas insertadas en la BD Firebird
real (`96_insertar_anomalias_firebird.bat`) → el agente las detectó automáticamente →
**10 alertas generadas** cubriendo las 6 reglas nuevas y las clásicas → visibles en el
dashboard con evidencia, comentario de auditoría guardado (CU-07), selector de auditores
(CU-11) y reportes PDF/Excel descargados (CU-12).

**Seed idempotente:** las reglas nuevas y los usuarios demo
(`auditor@petrolrios.com` / `Auditor123!`, `supervisor@petrolrios.com` / `Supervisor123!`)
se agregan también a bases ya sembradas, y la regla de horario se desactiva
automáticamente en bases existentes.

---

## 5. Secciones de la tesis que conviene actualizar

1. **Tabla 3 (sección 1.1.3):** agregar las 6 reglas nuevas con su fuente de umbral
   (caso documentado de enero 2026 y Tabla 2). Indicar que "Operación fuera de horario"
   queda deshabilitada por defecto por operación 24/7.
2. **Sección 4.1.6 (módulos funcionales):** mencionar el módulo de reportes (CU-12) y las
   métricas ejecutivas (CU-13) como implementados.
3. **Sección 7.2 (desarrollo):** describir el respeto al flag `Activa`, la entidad
   `ComentarioAlerta` y la columna `FechaResolucion`.
4. **Sección 7.3 (pruebas):** actualizar el número de pruebas unitarias y la cobertura.
5. **Capturas de pantalla (4.1.3 / anexos):** regenerar con la interfaz nueva.

---

## 6. Archivos principales tocados

**Backend:** 4 detectores, `SeedData`, `Alerta`, `ComentarioAlerta` (nueva),
`PetrolRiosDbContext`, `AlertaService`, `DashboardService`, `ReporteService` (nuevo),
`AlertasController`, `DashboardController`, `ReportesController` (nuevo),
`UsuariosController`, DTOs nuevos, `DependencyInjection`, csproj (+QuestPDF, +ClosedXML).

**Frontend:** `DashboardPage`, `AlertasPage`, `DetalleAlertaPage`, `LoginPage`,
`ReportesPage` (nueva), `Sidebar`, `App`, servicios y tipos, componentes UI nuevos.

**Tests:** `NuevasReglasDetectorTests` (nuevo), `TestHelpers`.

**Scripts:** `scripts/verificar-mejoras.bat` (restore + migración EF + build + tests +
build de frontend, con log en `verificacion.log`).

---

# Segunda ronda de mejoras (observaciones del usuario, junio 2026)

## 7. Logs de auditoría funcionales (CU-17)

La tabla `logs_auditoria` existía pero **nada escribía en ella**. Ahora se registra con
usuario e IP de origen: inicio/cierre de sesión, cambios de estado de alertas,
asignaciones, comentarios, actualización de reglas y creación/edición/desactivación de
usuarios (`AuditoriaExtensions` + llamadas en todos los controllers de escritura).

## 8. Monitoreo de Conexiones (pestaña nueva)

- **Endpoint `GET /api/v1/monitoreo/conexiones`:** estado por estación — Conectada /
  Sin conexión / Nunca conectada (ventana de 10 min), última ingesta, transacciones de
  las últimas 24 h, históricas y pendientes de análisis.
- **Endpoint `GET /api/v1/monitoreo/sistema`:** API (versión, uptime, entorno), BD
  (conectada + latencia en ms), SignalR (clientes conectados en vivo, con contador real
  en el hub), motor de detección (último ciclo: estado, alertas, duración).
- **Página "Conexiones"** en el menú Monitoreo con auto-refresco cada 10 s, tarjetas de
  estado con indicador pulsante y tabla de agentes.
- **KPI corregido en el dashboard:** "Estaciones Activas: 10" (mentira estática) →
  **"Estaciones Conectadas X/10"** calculado de verdad con la ingesta de los últimos
  10 minutos; en rojo si no hay ningún agente conectado.

## 9. Reglas honestas y alertas autoexplicativas

- **Pantalla de Reglas rediseñada:** se eliminó el formulario "Nueva Regla" (era
  engañoso — un parámetro inventado no ejecuta ninguna lógica). Ahora es la
  "Configuración del Motor de Detección": reglas agrupadas por detector con icono y
  descripción, umbral editable inline (Enter para guardar) y switch activar/desactivar.
  Un aviso explica que la lógica vive en el motor (Strategy Pattern, OE2) y aquí solo se
  parametriza. Los endpoints POST/DELETE de reglas se retiraron de la API.
- **Lista de alertas:** cada fila ahora muestra la descripción de la anomalía debajo del
  tipo (con tooltip del texto completo) — ya no hay que entrar a la alerta para saber de
  qué se trata.

## 10. Station Agent v2 — panel de control propio

El agente dejó de ser una consola ciega; ahora levanta un **panel web local en
`http://localhost:5180`** (solo accesible desde la máquina de la estación):

- Estado en vivo: último ciclo y resultado, transacciones enviadas, lotes pendientes
  de store-and-forward, latencia y última conexión/desconexión con el servidor.
- Configuración activa visible (estación, servidor, Firebird con password oculto,
  intervalo, watermark).
- **Modo automático / manual** (switch): en manual el agente no sincroniza solo y el
  operador usa el botón **"Sincronizar ahora"**.
- Botones **"Probar conexión Firebird"** (cuenta documentos en DCTO) y **"Probar
  conexión al servidor"** (login JWT + latencia).
- Bitácora de actividad de los últimos 60 eventos.
- Internamente: `AgentState` (estado observable), `CycleRunner` (ciclo reutilizable por
  el worker y el panel), `Worker` que respeta el modo manual.

## 11. Plug and play y distribución

- **`ejecutables/`** — todos los .bat organizados por propósito y orden de ejecución
  (ver `ejecutables/LEEME.md`): `1-INICIO` (INICIAR_TODO / DETENER_TODO), `2-DEMO`,
  `3-DIAGNOSTICO`, `4-PUBLICACION`, `5-DESARROLLO`.
- **`INICIAR_TODO.bat`:** un doble clic arranca Docker (lo abre si está cerrado),
  PostgreSQL, Firebird, la API, el frontend y el agente, espera a que cada servicio
  responda y abre el navegador con la app y el panel del agente.
- **La API sirve el frontend compilado** (`wwwroot` + SPA fallback): en producción un
  solo ejecutable entrega todo.
- **`publicar.bat`:** genera `dist\PetrolRios-Servidor\PetrolRios.Api.exe` y
  `dist\PetrolRios-Agente\PetrolRios.StationAgent.exe` (single-file, self-contained,
  no requieren .NET instalado) y, si Inno Setup 6 está presente, compila los
  instaladores `PetrolRios-Servidor-Setup.exe` y `PetrolRios-Agente-Setup.exe`
  (scripts `.iss` incluidos).

## 12. Secciones de la tesis a actualizar (segunda ronda)

1. **4.1.6 módulos:** agregar el módulo de Monitoreo de Conexiones y el panel local del
   Station Agent (modo manual/automático como control operativo).
2. **CU-17:** describir las acciones efectivamente auditadas (login, alertas, reglas,
   usuarios) con IP y usuario.
3. **7.2 despliegue:** describir la distribución como ejecutables self-contained e
   instaladores Inno Setup, y la API sirviendo la SPA (un contenedor de despliegue).
4. **Capturas:** regenerar Reglas, Conexiones y el panel del agente.

---

# Tercera ronda de mejoras (observaciones del usuario, junio 2026)

## 13. Heartbeat del agente y estado de conexión real

El agente ahora envía un **heartbeat** (`POST /api/v1/ingesta/heartbeat`) en cada ciclo,
también en modo manual. Una estación se considera **"En línea"** si su agente latió en
los últimos 3 minutos, **aunque no haya transacciones nuevas** — antes una estación con
agente activo pero sin datos aparecía falsamente como "Sin conexión". La página de
Conexiones distingue ahora "Señal de vida" (heartbeat) de "Última ingesta de datos", y
muestra la versión del agente. Verificado en vivo: EST-001 marca "En línea · hace
segundos · agente v2.0".

## 14. Estaciones dinámicas (auto-registro + CRUD)

- **Auto-registro:** las estaciones se crean solas la primera vez que su agente se
  conecta (`Estacion.CreateDesdeAgente`), en lugar de quemar 10 estaciones fijas. El
  dashboard muestra "Estaciones en Línea: X de N" (N = registradas reales, escalable).
- **Editar / eliminar** (`EstacionesController`): el usuario corrige nombre y zona de
  cada estación desde la página de Conexiones, y puede eliminar una estación que ya no
  es parte del sistema. Si la estación tiene historial (alertas/transacciones) se
  **desactiva** en lugar de borrarse, para conservar la trazabilidad de auditoría.

## 15. Motor de reglas personalizadas (escalabilidad sin tocar código)

El gran pedido de escalabilidad de la tesis: los usuarios **crean sus propias reglas de
negocio** desde la interfaz, y funcionan para cualquier escenario.

- **`CustomRuleDetector`** (5.º detector, Strategy Pattern): evalúa reglas definidas por
  el usuario. Cada regla elige una **fuente de datos** (Facturas, Cierres de turno,
  Despachos, Créditos, Tarjetas), aplica N **condiciones** combinadas con AND (operadores
  numéricos y de texto, incluidos "contiene", "vacío", etc.) y, opcionalmente, **agrupa y
  compara un agregado** (Conteo, Suma, Promedio) contra un umbral. Una regla mal definida
  se ignora y se registra, sin tumbar el ciclo.
- **`CatalogoReglasPersonalizadas`:** única fuente de verdad de fuentes/campos/operadores;
  la usan el builder de la UI, la validación al guardar y el detector al evaluar (no hay
  inyección arbitraria — todo se valida contra el catálogo).
- **`ReglasPersonalizadasController`** (CRUD + `/catalogo`) con validación estricta y
  auditoría. **Builder visual** en la página de Reglas: el usuario arma condiciones y
  agregaciones con menús desplegables, sin escribir código ni SQL.
- **Verificado en vivo end-to-end:** se creó "Facturas en efectivo mayores a $400" desde
  la UI → se guardó y persistió → se editó el umbral a $300 → el ciclo del motor la
  evaluó y generó la **alerta #83 "Regla Personalizada" (Crítico, score 96)** con
  descripción autoexplicativa. 8 tests unitarios nuevos cubren el detector
  (condiciones AND, agregación Conteo/Suma, regla inactiva, JSON inválido, campos vacíos).

## 16. Branding profesional y tiempo real más dinámico

- **Sin menciones de tesis en la UI:** se quitaron los "CU-XX", "OE2", "Proyecto de
  titulación · UDLA" y el "10 estaciones" quemado — la interfaz se presenta como producto
  de PetrolRíos S.A., no como entregable académico.
- **Tiempo real:** la llegada de una alerta por SignalR ahora **invalida y refresca
  automáticamente** las vistas de alertas, dashboard y conexiones (antes había que
  recargar). Los logs de auditoría y la lista de alertas además sondean cada 10–15 s.
- **Ciclos más cortos:** el job de detección pasó de cada 5 min a **cada minuto**; el
  agente de 60 s a **30 s**. El sistema es mucho más dinámico sin colapsar.

## 17. Resultado de verificación (ronda 3)

Build sin warnings; **85 pruebas unitarias en verde** (Domain 4, Detectors 69 — los 8
nuevos de reglas personalizadas incluidos —, Api 12 + 13 de integración que pasan con
Docker arriba). Frontend compila y empaqueta limpio. Migración EF nueva
`AgregarHeartbeatYReglasPersonalizadas` (columnas de heartbeat + tabla
`reglas_personalizadas`). Verificado en vivo: heartbeat "En línea", edición de estación,
y la regla personalizada generando alertas reales.

## 18. El agente como aplicación individual (instalable por estación)

El agente dejó de depender de un `appsettings` fijo: ahora es una **aplicación autónoma**
que se instala en la computadora de cada estación (la que tiene la base Firebird, pero
**no** la base central). El flujo en campo es: copiar el `.exe`, ejecutarlo, se abre el
panel local en `http://localhost:5180`, se escribe el **nombre de la estación** y los datos
de conexión, se guarda, y el agente empieza a enviar — sin tocar archivos ni reiniciar nada.

- **Configuración editable y persistente en disco** (`AgentConfigStore` →
  `config/agent-config.json`, junto al `.exe`). Reemplaza a `IOptions`: cualquier cambio
  (URL del servidor, ruta Firebird, intervalo, nombre) se aplica **en caliente**, sin
  recompilar ni reiniciar el servicio. `ServerClient` arma la URL del servidor en cada
  petición, por eso cambiar la URL surte efecto al instante.
- **Nombre de estación desde el propio agente.** Lo que se escribe en el panel se envía en
  cada ingesta/heartbeat y **se refleja en la página de Conexiones del servidor central**.
  Si la estación aún no existía, se **auto-registra** con ese nombre en su primer contacto;
  si ya existía con nombre por defecto (`Estación {código}`), se actualiza; si el supervisor
  ya le puso un nombre manual, no se sobrescribe.
- **Panel con dos pestañas (Monitoreo / Configuración)** y un **banner de bienvenida**
  cuando el agente aún no está configurado. El formulario está dividido en secciones:
  *Identidad* (código, nombre, zona), *Servidor central* (URL, usuario, contraseña,
  timeout), *Base Firebird* y *Operación* (intervalo, modo automático).
- **Opciones avanzadas de conexión "por si algo falla en campo".** La sección Firebird
  expone host, puerto, ruta del `CONTAC.FDB`, usuario, contraseña, **charset**, **dialect**
  y **WireCrypt** (con la pista *"Disabled para Firebird 2.5 con Legacy_Auth"*, que es la
  causa #1 de fallo de conexión a Contaplus). Botones **Probar Firebird** y **Probar
  servidor** diagnostican cada extremo por separado, y los errores se traducen a mensajes
  entendibles (archivo no encontrado, WireCrypt incompatible, servidor inalcanzable).
- **Arranque seguro:** mientras no esté configurado, el agente queda en modo manual (no
  intenta sincronizar contra una base inexistente); el heartbeat se envía siempre para que
  el panel central lo vea aunque todavía no mande transacciones.
- **Publicación self-contained:** un solo `.exe` (sin instalar .NET) vía `publicar.bat`, e
  instalador opcional con Inno Setup (`agente.iss`) para dejarlo como servicio de Windows.

## 19. Generador de reglas avanzado (modo expresión)

Al "modo básico" visual se le sumó un **modo avanzado** donde el usuario escribe la regla
como una **expresión lógica de programación**, mucho más abierta y combinable.

- **Motor de expresiones propio y seguro** (`Tokenizer` → `Parser` descendente recursivo →
  `EvaluadorExpresion`). **No** ejecuta código arbitrario: solo evalúa la expresión contra
  los campos del registro, con precedencia correcta (`||`, `&&`, comparaciones, `+ -`,
  `* /`, unario, paréntesis).
- **Operadores y funciones:** `> >= < <= == != && || ! + - * /`, y funciones útiles —
  `vacio()`, `contiene(,)`, `empieza(,)`, `termina(,)`, `longitud()`, `minusculas()`,
  `mayusculas()`, `abs()`, `redondear()`. Permite cosas imposibles en el modo básico, como
  **aritmética entre campos** (`Descuento / Subtotal > 0.1`) o condiciones compuestas con
  paréntesis.
- **Editor en la UI** con paletas de **campos / operadores / funciones** que se insertan al
  hacer clic, botón **"Validar expresión"** que comprueba sintaxis y que todos los campos
  referenciados existan en el catálogo de la fuente (validación en vivo, en verde/rojo).
- **Integración con el detector:** `CustomRuleDetector` compila la expresión una sola vez y
  filtra los registros; si la expresión está rota, **no tumba el ciclo** (se ignora la regla
  y se sigue). Las reglas avanzadas se muestran en la lista con el prefijo ⚡.
- Persistencia: columna `ExpresionAvanzada` en `reglas_personalizadas` (migración EF
  `ReglasPersonalizadasAvanzadas`).

## 20. Resultado de verificación (ronda 4)

Build sin warnings; **113 pruebas unitarias en verde** (Domain 4, Detectors 84 — incluidos
los nuevos `ExpresionAvanzadaTests` del motor de expresiones —, Api 25). Frontend compila y
empaqueta limpio. Verificado en vivo de punta a punta:

1. **Agente individual:** se configuró el nombre *"Estación Santo Domingo Centro"* en el
   panel del agente (`localhost:5180`), se guardó (persistió a disco), y la página de
   **Conexiones del servidor central mostró `EST-001 · Estación Santo Domingo Centro ·
   zona Centro · agente v2.1 · En línea`** — exactamente el flujo pedido (ejecutar el exe →
   poner el nombre → aparece conectado en el central).
2. **Regla avanzada:** se creó *"Efectivo alto con descuento agresivo"* con la expresión
   `TotalNeto > 400 && CodigoPago == 'EF' && Descuento / Subtotal > 0.1`; el validador la
   marcó **"Expresión válida"**, se guardó y quedó listada con el indicador ⚡, lista para
   evaluarse en el siguiente ciclo del motor.

## 21. Empaque del agente dentro del repositorio (monorepo)

El agente vive **dentro del proyecto** (`src/PetrolRios.StationAgent/`), así que su código
ya se versiona con cada commit — no hay un repositorio aparte. Para que implementar en una
estación sea "copiar una carpeta y ejecutar", se agregó un empaque limpio:

- **Plantilla de configuración versionada y sin secretos:** `agent-config.example.json`
  (junto al proyecto) documenta el formato y trae los valores por defecto (puertos, charset,
  dialect, rutas) pero **sin contraseñas**. Viaja con el repo y se copia a la salida en cada
  publicación. La configuración real de cada estación —con contraseñas— se crea localmente al
  guardar desde el panel (`config/agent-config.json`) y está **git-ignorada**, cumpliendo la
  regla 14 de seguridad (nunca subir secretos al repositorio).
- **Script de publicación `scripts/publicar_agente.bat`:** genera con `dotnet publish` un
  **ejecutable autocontenido de un solo archivo** (`win-x64`, no requiere instalar .NET) en
  `dist/agente/`, junto con la plantilla de config y un `LEEME.txt` con los pasos de
  instalación. La carpeta es el paquete listo para llevar a cada estación; al estar en
  `dist/` (git-ignorada) no engorda el repo y se regenera cuando cambia el código.
- **Flujo de implementación en campo:** correr `publicar_agente.bat` → copiar `dist/agente`
  a la computadora de la estación → ejecutar `PetrolRios.StationAgent.exe` → abrir
  `localhost:5180` → poner nombre y datos de conexión → guardar. La estación aparece "En
  línea" en el panel central.
- **Verificado:** la publicación corrió en Windows con **build correcto**; la carpeta
  `dist/agente` quedó con un único `PetrolRios.StationAgent.exe` autocontenido más
  `agent-config.example.json`, `appsettings.json`, `web.config` y `LEEME.txt`.

---

## 16. Despliegue sin fricción: panel del agente con login opcional (opt-in)

**Problema detectado en pruebas de campo.** Al endurecer la seguridad del panel se introdujo
un fallo de despliegue: en una máquina nueva el panel exigía iniciar sesión contra el servidor
central *antes* de poder configurarlo, pero el agente aún no tenía la conexión al central
configurada. Resultado: imposible configurar la conexión en la misma interfaz (problema del
huevo y la gallina).

**Solución (login opt-in por agente).** El panel del agente ahora arranca **abierto por
defecto** (`AgentSettings.RequiereLoginPanel = false`) — solo accesible desde `localhost`, así
que sigue protegido por la máquina. El administrador configura la conexión y, una vez enlazado
con el central, **activa el inicio de sesión para ese agente en concreto** desde
Configuración → Seguridad del panel. A partir de ese momento el panel exige login normal (RBAC
contra el central, con respaldo local PBKDF2 para acceso offline).

Cambios técnicos:

- `AgentSettings.RequiereLoginPanel` (bool, default `false`): bandera persistida en
  `agent-config.json`.
- `Program.cs`: el middleware de autenticación solo exige sesión cuando
  `RequiereLoginPanel = true`; `/api/sesion` devuelve `requiereLogin`; el GET/POST de
  configuración y `GuardarConfigRequest` exponen y mapean la bandera.
- `PanelHtml.cs`: `verificarSesion()` usa `!s.requiereLogin || s.autenticado`; nuevo selector
  "Requerir inicio de sesión para administrar este agente" en Seguridad del panel.

### 16.1 Quality of life: re-sincronizar desde una fecha

Utilidad de mantenimiento solicitada para el panel: **"Re-sincronizar desde"** (pestaña
Monitoreo). Permite fijar manualmente la marca de agua a una fecha/hora para volver a extraer y
reenviar las transacciones desde ese momento (p. ej. tras corregir un problema de conexión o de
datos en el central).

- `CycleRunner.ReiniciarWatermark(DateTime)`: fija el watermark, lo persiste y registra el evento.
- `Program.cs`: endpoint `/api/reiniciar-watermark` con `ReiniciarWatermarkRequest(string? Fecha)`.
- `PanelHtml.cs`: control de fecha/hora + botón "Re-enviar datos" + función `reiniciarWatermark()`.

**Verificado en vivo (Chrome, junio 2026):** build correcto con 126 pruebas en verde; el panel
abre sin overlay de login por defecto; el selector "Requerir inicio de sesión" aparece en
Seguridad del panel (en "No — panel abierto"); el control "Re-sincronizar desde" funciona en
Monitoreo.

---

## 17. Agente multiplataforma: publicación para Windows, Linux y macOS

El agente de estación es ASP.NET Core 9 (multiplataforma) y `AddWindowsService()` es
no-op fuera de Windows, así que el binario corre igual en Linux y macOS sin cambiar la
lógica. Lo que faltaba era el **empaque** para cada sistema. Se amplió la publicación:

- `scripts/publicar_agente.bat` ahora genera, desde la máquina Windows del desarrollador
  (cross-publish con `dotnet publish -r <RID>`), cuatro paquetes autocontenidos de un solo
  ejecutable (sin instalar .NET en la estación):
  - `dist/agente-windows` (win-x64) — `PetrolRios.StationAgent.exe`
  - `dist/agente-linux` (linux-x64, p. ej. Ubuntu) — `PetrolRios.StationAgent`
  - `dist/agente-macos-intel` (osx-x64) — `PetrolRios.StationAgent`
  - `dist/agente-macos-arm` (osx-arm64, Apple Silicon) — `PetrolRios.StationAgent`
- **Instaladores de arranque automático por sistema:**
  - Windows: `instalar_agente_servicio.bat` (servicio `sc create`).
  - Linux: `instalar_agente_servicio.sh` (unidad **systemd**, `systemctl enable`).
  - macOS: `instalar_agente_servicio_macos.sh` (**launchd**, `~/Library/LaunchAgents`).
- **LEEME específico** por sistema (`agente-LEEME-windows/linux/macos.txt`) con los pasos
  propios de cada uno: en Linux/macOS recuerda `chmod +x` y, en macOS, quitar la cuarentena
  (`xattr`); rutas típicas del `CONTAC.FDB` y comandos de servicio.

**Verificado (junio 2026):** la publicación corrió en Windows y generó los cuatro paquetes;
`file` confirma el formato correcto de cada binario (PE32+ x86-64 para Windows, ELF x86-64
para Linux, Mach-O x86_64 y arm64 para macOS), cada uno con su LEEME e instalador de servicio.
uario se **envía un correo** con botón "Verificar correo electrónico" (enlace a
  `/verificar-correo?token=...`); endpoints `/auth/verificar-email` y `/auth/reenviar-verificacion`.
- **Obligatoria:** no se permite iniciar sesión (ni por contraseña ni por QR) hasta confirmar
  el correo; mensaje claro + botón "Reenviar correo de verificación".
- **SMTP real (Gmail)** configurado en `appsettings.Secrets.json` **git-ignoreado** (la App
  Password nunca se sube al repo). Migración `VerificacionEmail`.
- **Probado en vivo:** cuenta creada → correo enviado por Gmail SMTP → llegó a la bandeja real
  → el enlace verificó la cuenta.

## 30. Login con autenticador y recuperación de contraseña

- **Entrar con el código del autenticador** (sin contraseña): `POST /auth/login-totp` para
  cuentas con 2FA activo; opción en la pantalla de login.
- **¿Olvidaste tu contraseña?**: envía un enlace al correo (`/auth/olvide-password`), y la
  página `/restablecer-password?token=...` permite fijar una nueva (`/auth/restablecer-password`).
  Tokens de un solo uso en memoria (`PasswordResetService`), válidos 1 hora.

## 31. Guía de instalación (INSTALACION.md)

- Documento `INSTALACION.md` con requisitos, publicación de ejecutables, configuración sin
  secretos quemados, seguridad del panel del agente, notificaciones por correo, actualización
  remota y solución de problemas, para el **servidor central** y el **agente**.

## 32b. Correcciones de bugs y pulido (ronda 7)

- **Login ya no revienta (500) con 2FA:** el controlador devolvía NRE al pedir el código;
  ahora retorna `Requiere2Fa` sin tocar el usuario nulo.
- **Agente "Probar servidor":** se quitó la mutación de `HttpClient.Timeout` tras usar el
  cliente (causaba *"instance has already started a request"*); el timeout se aplica por
  petición con `CancellationToken`.
- **Editar usuario:** `UpdateAsync` ahora actualiza nombre y rol (`Usuario.ActualizarPerfil`);
  antes solo cambiaba el estado.
- **Alta de usuarios:** campo **"Confirmar contraseña"** con validación y mensajes de error;
  baja con confirmación y aviso de resultado.
- **Pantalla de login** rediseñada (gradiente y decoración, más profesional) y se quitó el
  número fijo de estaciones (es incremental).
- **Hash de contraseñas:** se corrigió un hash `$2b$` (generado fuera de la app) que la
  librería del proyecto no procesaba; los hashes deben ser compatibles con BCrypt.Net.
- Verificado en vivo: admin login 200, editar rol persiste, borrado 204, agente probar
  servidor OK.

## 32. Resultado de verificación (rondas 5–6)

Build sin warnings; **126 pruebas unitarias en verde** (Domain 8, Detectors 89 — incluidos los
nuevos de expresiones, TOTP y QR —, Api 29 con las de integración). Frontend compila y empaqueta
limpio. Verificado en vivo: dashboard, conexiones (API v2.2.0, SignalR activo), detección
end-to-end con Firebird real, panel del agente con login RBAC, 2FA con QR, **verificación de
correo real contra Gmail**, y recuperación/login por autenticador. Migraciones EF nuevas:
`SeguridadUsuario` y `VerificacionEmail`.

---

## 18. Idempotencia de ingesta (anti reenvío duplicado) + login QR opcional

**Problema observado en pruebas:** al insertar en Firebird un registro fechado al futuro,
el agente lo re-extraía en cada ciclo (su fecha siempre quedaba por encima del watermark) y
el central lo volvía a guardar y a alertar **una y otra vez**. Además, el central insertaba
en `transacciones_staging` sin deduplicar.

**Solución — idempotencia por huella de contenido:**

- `TransaccionStaging` ahora calcula un `HashContenido` = SHA-256 de
  `estación | tipo | datos`. Índice **único** `(EstacionId, HashContenido)` como red de
  seguridad a nivel de base de datos.
- `IngestaService.RecibirLoteAsync` descarta los reenvíos: consulta los hashes ya existentes
  de la estación y los duplicados dentro del mismo lote, e inserta solo lo nuevo. La respuesta
  reporta cuántas se insertaron vs. cuántas se descartaron por duplicado. Así un registro
  fechado al futuro se guarda y alerta **una sola vez**, aunque el agente lo reenvíe en cada
  ciclo. Si el registro de origen cambia, su hash cambia y se trata como nuevo (deseable:
  permite detectar modificaciones/backdating).
- Migración `IdempotenciaStaging` (rellena las filas previas con su Id para no chocar con el
  índice único; las nuevas usan el SHA-256 real).

**Nota sobre puntos ciegos del watermark (pendiente para fase de detectores):** el watermark
por fecha tiene puntos ciegos conocidos (registros con fecha anterior a la marca, o reloj
desfasado, pueden saltarse). Quedan como mejora: watermark por ID monotónico (las tablas de
Contaplus tienen generadores como `GEN_FACT_ID`, `GEN_DCTO_ID`) + ventana de solapamiento, y
un detector nuevo de "fecha fuera de rango plausible" (futuro/backdating) que convierte ese
mismo escenario en una anomalía detectable.

**Correo de verificación:** el enlace ya era configurable (`App:FrontendUrl`, cae a
`localhost:5173`). Para que funcione fuera de la máquina del central, basta apuntar esa clave a
la IP de red/dominio (variable de entorno `App__FrontendUrl`) y servir el frontend en la red.

**Login por QR oculto por defecto:** como requiere que el teléfono alcance al central por la
red (sin dominio público aparece "caído"), se oculta tras `VITE_QR_HABILITADO`. El login móvil
queda cubierto por el autenticador (TOTP), que funciona offline. El QR se reactiva cuando haya
una URL pública.

---

## 19. Investigación profunda del esquema Firebird + detector de fecha fuera de rango

Se documentó el análisis tabla por tabla de `CONTAC.FDB` (~200 tablas) y el diseño de la
plataforma de detección configurable en `docs/investigacion-deteccion-anomalias.md`: tablas
útiles no aprovechadas (TURN.EST_TURN para turnos sin cerrar, DESP.FAC_DESP para despachos no
facturados, TANQ_REPO.DIFERENCIA para cuadre de tanque, CRED_CABE para créditos no autorizados,
ANUL para kiting/cancelar-reingresar, PLACA_BLOQ/PLACA_CUPO para regulación), el hallazgo de la
dualidad `FEC_*` (fecha negocio) vs `FUL_*` (inserción real) para detectar backdating, y el plan
de 6 fases hacia extracción multi-tabla + registro dinámico con auto-documentación + reglas sin
código. Validado con literatura de prevención de pérdidas en estaciones (fuentes en el doc).

**Primer detector nuevo entregado — "fecha fuera de rango plausible" (backdating):** regla 6 del
`InvoiceAnomalyDetector`. Marca facturas (DCTO) y créditos (CRED_CABE) fechados en el futuro más
allá de una tolerancia configurable (`FechaFuturaToleranciaHoras`, por defecto 24 h) respecto al
momento de procesamiento — convierte el experimento de la inserción fechada al futuro en una
anomalía detectable. Sembrada en `SeedData` (editable desde la pantalla de Reglas) y cubierta
con 3 pruebas unitarias nuevas (92 en total, todas en verde).

---

## 20. Subsistema de alertas por ámbito: Operativa (estación) vs Auditoría (fraude)

Primer bloque del subsistema que separa las alertas en dos carriles, para que el administrador
de cada estación reciba en tiempo real solo SUS problemas operativos y el central conserve la
visión completa (incluido el carril de fraude).

- **Enum `AmbitoAlerta`** (`Operativa` / `Auditoria`), ortogonal al `NivelRiesgo`.
- **`Alerta.Ambito`** persistido (default `Auditoria`) + índices para filtrar por ámbito;
  migración `AmbitoAlerta` (las alertas previas quedan como Auditoría).
- **`DetectedAnomaly.Ambito`**: los detectores marcan el carril. Primer ejemplo: "campos
  obligatorios vacíos" pasa a **Operativa** (error honesto del cajero); el backdating sigue en
  **Auditoría**.
- **SignalR**: el central (auditores/supervisores/admins) recibe todo; el grupo de la estación
  recibe en tiempo real **solo** las operativas, por un evento aparte `ProblemaEstacion` (el
  admin de estación no ve el carril de fraude).
- Cubierto con 2 pruebas nuevas de ámbito (94 en Detectors, verde).

Pendiente del subsistema: pestaña "Problemas de estación" (agrupada por estación, expandible),
endpoint de agregación, asociación usuario↔estación y aviso por correo al contacto de la estación.

---

## 21. Pestaña "Problemas de estación" (carril Operativa)

Segunda parte del subsistema de ámbito: la vista para el central de los problemas operativos
agrupados por estación.

- **Endpoint** `GET /api/v1/alertas/problemas-estacion?estacionId&dias`: alertas de ámbito
  Operativa agrupadas por estación y día con su conteo; cada grupo trae la lista de problemas
  para documentación. `AlertaResponse` ahora expone `Ambito`.
- **Frontend**: nueva página y entrada de menú "Problemas de estación" (Monitoreo). Tabla por
  estación/día con el conteo; clic en el grupo despliega la lista. Selector de ventana (1/7/30
  días) y refresco automático cada 30 s. Separada de la bandeja principal de auditoría.

Pendiente del subsistema: asociación usuario↔estación y correo de contacto por estación (para
que el administrador de la estación reciba y vea solo lo suyo).

---

## 22. Asociación usuario↔estación + correo de contacto + aviso de problemas operativos

Cierre del subsistema de ámbito: ahora el administrador de una estación recibe los problemas
operativos de SU estación.

- **`Usuario.EstacionId`** (nullable): adscribe un usuario a una estación. Expuesto en crear/editar
  usuario (`CrearUsuarioRequest`/`ActualizarUsuarioRequest`) y en `UsuarioResponse`. Índice en BD.
- **`Estacion.CorreoContacto`** (nullable): correo del contacto/administrador de la estación.
- **Aviso por correo (digest):** al final de cada ciclo de detección, si una estación tuvo
  problemas operativos (carril Operativa), se envía **un solo correo** con el resumen a su
  `CorreoContacto` y a los usuarios adscritos a esa estación (`EstacionId`). No satura: un correo
  por ciclo, solo si hay problemas y destinatarios.
- Migración `UsuarioEstacionYContacto` (columnas nullable, sin backfill). Build verde, 94 pruebas
  en Detectors.

Con esto el subsistema de ámbito queda funcional de punta a punta (clasificación → pestaña →
notificación in-app/SignalR + correo al admin de estación). Falta solo el detalle de UI para
elegir estación/correo desde el formulario (hoy se setea por API).

---

## 23. Plataforma flexible (1/3): explorador de tablas Firebird con documentación automática

Primer bloque de la plataforma de detección configurable. El agente ahora puede **introspeccionar
el esquema real de Firebird** y auto-documentar cualquier tabla — la base para registrar nuevas
fuentes y crear reglas sin tocar código.

- **`FirebirdExtractor.ListarTablasAsync`**: lista las tablas de usuario (consulta `RDB$RELATIONS`,
  excluye vistas y tablas del sistema).
- **`FirebirdExtractor.DescribirTablaAsync`**: **verifica que la tabla exista** (validación contra
  la lista real = lista blanca anti-inyección) y devuelve sus columnas con **tipo legible, longitud
  y nulabilidad** (mapea `RDB$FIELD_TYPE` a SMALLINT/INTEGER/VARCHAR/TIMESTAMP/…), más el conteo de
  filas. Todo en **solo lectura**.
- **Endpoints del agente**: `GET /api/firebird/tablas` y `GET /api/firebird/tabla/{nombre}`.
- **Panel del agente**: nueva tarjeta "Explorador de tablas (documentación automática)" en
  Monitoreo: botón "Cargar tablas", selector de tabla y tabla de campos/tipos/nulabilidad.

Próximos bloques: registrar fuentes de extracción configurables (multi-tabla) y el creador de
reglas genérico sobre cualquier tabla.

---

## 24. Plataforma flexible (2/3): fuentes de extracción configurables (multi-tabla)

Ahora el agente puede enviar al central datos de **cualquier tabla que elijas**, no solo las 7
fijas — configurable desde el panel, sin recompilar.

- **`AgentSettings.FuentesExtraccion`**: lista de fuentes (nombre lógico, tabla, columna de
  watermark opcional, activa), persistida en `agent-config.json`. `Clonar()` hace copia profunda.
- **`FirebirdExtractor.ExtractFuenteAsync`**: extrae una fuente validando **tabla y columna de
  watermark contra el catálogo real** (lista blanca anti-inyección); si hay columna de fecha,
  filtra `> watermark`; si no, toma un tope de 500 filas (la idempotencia del central descarta
  reenvíos). Cada fila se serializa a JSON y se envía con el nombre de la fuente como tipo.
- **`ExtractSinceAsync`** incluye las fuentes activas tras las fijas, con tolerancia a fallos
  (una fuente mal configurada no rompe el ciclo).
- **Endpoints del agente** `GET/POST /api/fuentes` (el POST valida que las tablas existan).
- **Panel del agente**: tarjeta "Fuentes de extracción adicionales" para agregar/quitar tablas,
  apoyándose en el explorador para ver campos y elegir la columna de watermark.

Falta el bloque 3/3: el creador de reglas genérico que opere sobre estas fuentes en el central.

---

## 25. Plataforma flexible (3/3, motor): reglas sobre cualquier fuente (genéricas)

Mejora directa del creador de reglas (forma básica y avanzada) para que opere sobre **cualquier
tabla**, incluidas las fuentes configurables, sin tocar el código base. Antes el motor estaba
anclado a un catálogo cableado de 5 fuentes; ahora es genérico.

- **`DetectionContext.FuentesGenericas`**: las tablas configurables (tipos de staging no conocidos)
  se exponen como registros diccionario campo→valor.
- **`AnomalyDetectionJob`**: deserializa esas filas a diccionarios y las pasa al contexto.
- **`CustomRuleDetector`**: `ObtenerFuente` cae a las fuentes genéricas; condiciones simples y
  **expresiones avanzadas** funcionan igual, **infiriendo el tipo (número/texto) del valor**
  cuando no hay catálogo. Una expresión rebuscada como `DIFERENCIA > 500 && VENTAS_TANQ >= 100`
  ya corre sobre una tabla arbitraria.
- **`CatalogoReglasPersonalizadas`**: `GetValor` resuelve diccionarios genéricos; `ValidarDefinicion`
  acepta fuentes configurables (valida sus campos en runtime, no contra el catálogo estático).
- 2 pruebas nuevas (condición básica y expresión avanzada sobre fuente genérica); 96 en Detectors.

Falta solo la UI: que el formulario de reglas liste las fuentes configurables y sus campos
(auto-documentación) para elegirlos sin escribir a mano. El motor ya los soporta.

---

## 26. Plataforma flexible (3/3, UI): el builder de reglas auto-descubre las fuentes configurables

Remate de la plataforma: el formulario de Reglas ya ofrece automáticamente las tablas
configurables y sus campos, sin escribir nada a mano.

- **`GET /reglas-personalizadas/catalogo`** ahora es asíncrono y, además de las 5 fuentes del
  catálogo, **descubre las fuentes configurables** desde el staging (tipos no conocidos) y
  **auto-documenta sus campos** infiriendo el tipo (número/texto) de una fila de muestra. El
  frontend, que ya consume este catálogo, las muestra en el selector sin cambios.
- **`EvaluadorExpresion.Validar`**: en fuentes configurables valida solo la sintaxis (los campos
  se resuelven en runtime); en las del catálogo sigue validando que los campos existan.
- **Controlador de reglas**: la validación al guardar acepta fuentes configurables (antes
  rechazaba toda fuente fuera del catálogo).

Con esto el ciclo completo de la plataforma queda cerrado: explorar una tabla → registrarla como
fuente (multi-tabla) → el agente la envía → y crear reglas básicas o avanzadas sobre sus campos
desde la interfaz, **sin tocar código**. Build verde (96 pruebas en Detectors).

---

## 27. Cobertura de pruebas para lo nuevo

Pruebas unitarias para las funcionalidades agregadas en esta ronda:

- **Idempotencia** (`TransaccionStagingTests`): la huella `CalcularHash` es determinista, tiene
  longitud SHA-256 (64 hex) y cambia si cambia cualquier componente (estación/tipo/datos);
  `Create` asigna el hash del contenido.
- **Ámbito de alerta** (`AlertaAmbitoTests`): `Create` por defecto es Auditoría y conserva
  Operativa cuando se indica.
- **Motor de reglas genérico** (`CustomRuleDetectorTests`, casos nuevos): agregación (Suma por
  grupo) sobre fuente configurable, condición de texto sobre fuente genérica, y expresión que no
  cumple → sin alerta.
- **Backdating sobre créditos** (`InvoiceAnomalyDetectorTests`): un crédito (CRED_CABE) fechado al
  futuro también dispara la alerta.

Totales: Domain 16, Detectors 100, Api 29 — todo en verde.

---

## 28. Detectores nuevos del ingeniero: turno sin cerrar y crédito sin garante

Dos patrones que mencionó el ingeniero, integrados a los detectores existentes y sembrados como
reglas editables.

- **Turno sin cerrar** (CashFraudDetector, carril **Operativa**): se añadió `EST_TURN` al DTO y a
  la extracción (`WHERE FFI_TURN > watermark OR EST_TURN = '0'`, trae también turnos abiertos).
  La regla `TurnoSinCerrarHorasUmbral` (default 18 h) alerta cuando un turno sigue abierto desde
  hace más horas que el umbral. Es el descuido operativo ("se olvidan de cerrar turno") y va al
  carril de la estación, no al de auditoría.
- **Crédito sin garante** (PaymentFraudDetector): la regla `CreditoSinGaranteHabilitado` alerta
  cuando un crédito (CRED_CABE) se otorga con `COD_GARA` vacío — señal de autorización indebida
  ("autorizan créditos a quien no debe").
- 4 pruebas nuevas (turno sin cerrar dispara/no dispara según horas; crédito con/sin garante).
  Totales: Domain 16, Detectors 104, Api 29 — todo verde. Agente compila (EXITCODE 0).

---

## 29. Detector nuevo del ingeniero: despacho no facturado

- **Despacho no facturado** (InvoiceAnomalyDetector, carril **Operativa**): se añadió `FAC_DESP`
  al DTO de despacho y a la extracción. La regla `DespachoNoFacturadoHabilitado` alerta cuando un
  despacho con galones servidos (`CAN_DESP > 0`) no está marcado como facturado (`FAC_DESP` ≠ '1')
  — combustible que salió sin cobrarse ("no lo colgó bien / no cerró el despacho"). Si el indicador
  viene vacío no se asume nada (evita falsos positivos). Sembrada como regla editable.
- 2 pruebas nuevas (despacho no facturado dispara alerta operativa; despacho facturado no).
  Totales: Domain 16, Detectors 106, Api 29 — verdes. Agente compila (EXITCODE 0).

> Nota: el **cuadre de tanque** (TANQ_REPO.DIFERENCIA) ya es alcanzable sin detector nuevo:
> se registra TANQ_REPO como fuente configurable y se crea una regla `DIFERENCIA > X` desde la UI.

---

## 30. Detector del ingeniero: anulaciones recurrentes (kiting)

- **Anulaciones recurrentes / kiting** (InvoiceAnomalyDetector, carril Auditoría): la regla
  `AnulacionRecurrenteDiasMinimo` (default 3) alerta cuando un mismo punto de emisión
  (establecimiento + punto) acumula anulaciones en varios días distintos — el patrón de
  "cancelar y reingresar al día siguiente, y así sucesivamente" que mencionó el ingeniero, usado
  para rodar la deuda o mover el período. Sembrada como regla editable.
- 2 pruebas nuevas (anulaciones en 3 días distintos disparan; mismo día no). Domain 16,
  Detectors 108, Api 29 — verdes.

**Con esto quedan implementados los cuatro patrones del ingeniero:** turno sin cerrar (operativa),
crédito sin garante, despacho no facturado (operativa) y kiting. El cuadre de tanque se cubre con
la plataforma de fuentes configurables + reglas, sin detector dedicado.

---

## 31. Sincronización observable de tablas dinámicas y prueba real de alertas

Se cerró la diferencia entre “la tabla está documentada” y “el agente realmente la está
extrayendo”. La marca de agua ahora se presenta y usa como **cursor incremental de extracción**,
no como parte de la autodocumentación.

### Estado por fuente y estación

- Nueva entidad y migración `FuenteDatosEstacionEstado`: guarda por fuente/estación si la tabla
  existe, si el cursor es válido, estado del ciclo, filas leídas/enviadas, total enviado, versión
  de configuración, último éxito y error accionable.
- Nuevo endpoint `POST /api/v1/fuentes-datos/estado-agente`; los agentes reportan estado incluso
  cuando no hay filas o la tabla/columna está mal configurada.
- `GET /api/v1/fuentes-datos` incluye la matriz de sincronización y detecta agentes que todavía
  no recibieron la versión vigente de la fuente.

### Watermark/cursor seguro

- El backend y el agente rechazan como cursor columnas que no sean `DATE`, `TIME` o `TIMESTAMP`.
- La UI ya no permite escribir cualquier nombre: ofrece únicamente las columnas temporales
  descubiertas en el esquema. Sin cursor queda explícito el “modo muestra” (máximo 500 filas).
- Cada fuente central tiene su propio cursor persistente (`source-watermarks.json`); una fuente
  recién registrada no hereda el watermark global y carga primero sus 500 filas más recientes.
- Se corrigió un defecto de zona horaria: Firebird `TIMESTAMP` no tiene zona; el cursor conserva
  el reloj nativo de Firebird y solo la fecha enviada al central se convierte a UTC.

### Doble check en las interfaces

- En Reglas → Fuentes de datos, cada tabla muestra el estado por estación, agente en línea,
  versión aplicada, filas del último ciclo, total y error.
- El panel local del Station Agent incorpora “Fuentes dinámicas recibidas del sistema central”
  con tabla, cursor, estado, filas leídas/enviadas y hora del reporte.
- Nuevo diagnóstico `ejecutables/3-DIAGNOSTICO/verificar_fuentes_dinamicas.bat`, que compara el
  estado del agente con el registrado en el central.

### Notificaciones y carriles

- El usuario autenticado transmite su `EstacionId` a SignalR y escucha `NuevaAlerta` y
  `ProblemaEstacion`.
- Las suscripciones sobreviven al montaje estricto de React y a recreaciones/reconexiones de la
  conexión SignalR.
- Cada aviso lleva `NotificationId`; así se deduplica el mismo problema recibido por grupo de rol
  y grupo de estación sin confundir dos alertas nuevas que aún tienen `Id = 0`.
- Las alertas operativas invalidan en tiempo real la consulta de “Problemas de estación”.
- Se corrigió el desfase de un día al mostrar agrupaciones `DateOnly` en esa pantalla.

### Verificación

- Prueba E2E automatizada: una fila de fuente genérica crea una alerta de Auditoría y otra
  Operativa, persiste ambas y emite `NuevaAlerta`/`ProblemaEstacion`.
- Prueba real con `TANQ_REPO`, cursor `FEC_FIN_REPO` y condición `DIFERENCIA > 500`: el agente
  reportó 1 fila leída/1 enviada; Hangfire creó ambos ámbitos; Chrome mostró simultáneamente
  “Nueva alerta: Personalizada” y “Nuevo problema de estación”; la operativa apareció en su
  pestaña. Los datos temporales de la prueba se limpiaron al finalizar.
- Totales verdes: **Domain 34, Detectors 110, API 40**. Frontend: lint sin errores y build
  de producción correcto.
- Se restauró `scripts/verificar-mejoras.bat` para ejecutar restore, build Release, pruebas,
  comprobación de migraciones, lint y build frontend con log en `verificacion.log`.

---

## 32. Tercer subsistema: Monitor local de problemas operativos por estación

Se completó la interfaz que faltaba para las estaciones. No es otra vista dentro del central:
es un ejecutable independiente, desplegable como el Station Agent, pero con flujo inverso y
estrictamente de solo lectura.

### Nuevo `PetrolRios.StationMonitor`

- Proyecto ASP.NET Core local en `src/PetrolRios.StationMonitor`, panel en
  `http://localhost:5190`.
- Se autentica contra el servidor central con una cuenta vinculada a estación y rechaza cuentas
  sin `EstacionId` o que pertenezcan a otro código.
- Consulta periódicamente únicamente problemas con ámbito `Operativa`, estado activo y estación
  propia. No accede a Firebird, no envía transacciones y no expone acciones de escritura.
- Panel con estado de conexión, conteos, prioridad, referencias, actividad local, botón de
  consulta manual, configuración editable y avisos del navegador/sonido para problemas nuevos.
- Se añadió publicación self-contained, instalador Inno Setup, instalación como servicio Windows
  y scripts de inicio/diagnóstico. `INICIAR_TODO.bat` ya levanta también el monitor.

### Aislamiento de seguridad

- El JWT incorpora el claim firmado `estacion_id`; el login devuelve además código y nombre de
  estación.
- `GET /alertas/problemas-estacion` fuerza la estación del JWT aunque el cliente intente enviar
  otro `estacionId`; `soloActivos=true` excluye alertas cerradas/resueltas.
- Las cuentas de estación reciben `403` al intentar abrir dashboard, listado general de alertas
  u otros módulos centrales.
- El detalle solo permite una alerta operativa propia y devuelve `404` para otra estación o para
  el carril Auditoría.
- Ingesta, heartbeat, estado de fuentes y reporte de esquema validan que el código enviado
  coincida con la estación firmada.
- SignalR dejó de confiar en `rol`/`estacionId` de la query string. Una cuenta de estación entra
  exclusivamente a `estacion-{id}`; una cuenta central entra al grupo de su rol.
- El hub ahora exige autenticación explícita; conexiones anónimas tampoco pueden aparecer como
  usuarios conectados ficticios.
- El cliente SignalR ignora limpiamente la negociación obsoleta del primer montaje de React
  Strict Mode y reintenta fallos reales sin dejar errores rojos falsos en la consola.
- Las cuentas técnicas `agent-est-001`…`agent-est-010` se vinculan idempotentemente a sus
  estaciones durante el seed.

### Gestión central

- La tabla de Usuarios muestra `Sistema central` o el código de estación.
- Crear y editar usuario permite asignar o retirar la estación desde la UI.
- Se valida que la estación exista y esté activa.

### Pruebas y evidencia conservada

- Build completo sin advertencias; frontend de producción correcto.
- Pruebas: **Domain 34, Detectors 110, API 47, Monitor 2**, todas verdes; la API usa PostgreSQL Testcontainers.
- Nuevas pruebas comprueban JWT, aislamiento de alertas, bloqueo de heartbeat cruzado y resolución
  de grupos SignalR desde claims.
- Chrome: central, agente y monitor recorridos. Firebird se recuperó y TANQ_REPO quedó
  `Sincronizada`; el agente reportó 239 tablas.
- Evidencias permanentes en PostgreSQL:
  - `145` turno sin cerrar EST-001 (Operativa),
  - `146` despacho no facturado EST-001 (Operativa),
  - `147` problema EST-002 usado para demostrar aislamiento,
  - `148` alerta Auditoría EST-001 que el monitor no muestra,
  - `149` operativa cerrada que `soloActivos` excluye,
  - `150` operativa creada con el monitor abierto; apareció automáticamente y generó el evento
    local `ALERTA`.

---

# Ronda de mejoras — sesión junio 2026 (5 etapas)

Se trabajó en 5 etapas, cada una probada (build Release + 194 pruebas en verde + lint/build de
frontend), verificada en vivo en el navegador y commiteada por separado.

## 33. Etapa 1 — Inicio de sesión (commit `2704ae8`)
- **Problema:** toda cuenta creada por el Administrador quedaba sin verificar y el login la
  bloqueaba; además el enlace de verificación apuntaba a `localhost`, imposible de abrir desde un
  móvil. Resultado: usuarios nuevos (p. ej. un compañero) no podían entrar.
- **Solución:** las cuentas creadas por el Admin quedan **verificadas en el alta** (sistema interno
  con altas avaladas). La verificación por correo pasa a ser **configurable**
  (`Seguridad:RequerirVerificacionEmail`, **desactivada por defecto**) en `AuthService`,
  `LoginConTotp` y `QrEstado`. Seed idempotente `EnsureCuentasAccesoAsync`: marca verificadas las
  cuentas activas sin verificar, asegura al Administrador activo y sin bloqueo por intentos, y
  permite recuperación "break-glass" del admin vía `Seguridad:AdminPasswordInicial`.
- Test de integración nuevo: una cuenta creada por el Admin se auto-verifica y entra de inmediato.
- **Correo verificado en vivo:** la recuperación de contraseña envía y **llega a la bandeja real**
  (SMTP de Gmail operativo). El login ya no depende del correo.

## 34. Etapa 2 — Alertas operativas fuera de la bandeja de auditoría (commit `4919509`)
- Las alertas de ámbito **Operativa** (turno sin cerrar, despacho no facturado, campos faltantes)
  ya no aparecen en la bandeja de **Alertas** (auditores) ni en sus notificaciones; quedan solo en
  **"Problemas de estación"** y en el Monitor de estación.
- `AlertaRepository.ApplyFilters` filtra `Ambito=Auditoria`; `DashboardService` excluye las
  operativas de los KPIs; `AnomalyDetectionJob` emite las operativas como `ProblemaEstacion` (no
  `NuevaAlerta`); el `NotificationProvider` del frontend separa los handlers (la operativa no toca
  el contador del auditor). Verificado en vivo: la bandeja pasó de **141 a 135** y las 6 operativas
  siguen en su pestaña.

## 35. Etapa 3 — Reglas y Fuentes de datos (commit `438952f`)
- El registro de tablas (**Fuentes de datos**) se separó a su **propia página** (`/fuentes-datos`,
  con entrada de menú): registrar tablas y configurar reglas son tareas distintas.
- La pantalla de **Reglas** se rediseñó con cabecera + **resumen** (reglas del motor, activas,
  inactivas, detectores) y **pestañas** "Motor de detección" y "Reglas personalizadas".

## 36. Etapa 4 — Ajustes del central (QOL) (commit `afd6795`)
- Nueva página **Ajustes** (`/ajustes`) con funciones de calidad de vida: **tema
  Sistema/Claro/Oscuro** (real, en toda la app, vía clases en `<html>` + variables CSS), **sonido
  en alertas críticas** (tono Web Audio, sin archivo) y **avisos emergentes (toasts)** activables.
  Preferencias persistidas en el navegador (`SettingsContext`).

## 37. Etapa 5 — Interfaz del Monitor de estación (commit `07dd4ed`)
- El Monitor (`:5190`) usaba un tema verde con gradientes que se veía poco profesional e
  inconsistente. Se rediseñó a la **paleta slate + azul del panel central** (conservando TODAS las
  clases CSS, sin tocar la lógica/JS): logo y botón principal en azul, tarjetas y bordes neutros.
  Ahora se ve como parte del mismo producto.

## 38. Secciones de la tesis a actualizar (esta ronda)
- **4.1.6 / módulos:** página de Fuentes de datos separada, pantalla de Ajustes (QOL) y Monitor
  rediseñado.
- **Seguridad:** verificación de correo configurable + auto-verificación de altas + recuperación
  break-glass del admin.
- **Capturas:** regenerar Reglas (pestañas), Fuentes de datos, Ajustes y el Monitor.

## 39. Desbloqueo de cuenta por autoservicio desde el login

**Motivación.** El sistema bloquea la cuenta 15 min tras 5 intentos fallidos (anti fuerza bruta).
Antes, la única salida era esperar o restablecer la contraseña. Si el usuario **recuerda** su
contraseña y solo fue víctima del bloqueo (propio o de un tercero tecleando mal), forzar un cambio
de contraseña es fricción innecesaria. Se añade un desbloqueo dedicado, sin cambiar la contraseña.

**Diseño (espejo de la recuperación de contraseña, sin tocar el esquema).**
- `AccountUnlockService` (Infrastructure): tokens en memoria, de un solo uso, caducan en 1 h. Mismo
  patrón que `PasswordResetService`; registrado como singleton en DI. **No** añade columnas ni
  migraciones (la verificación EF sigue limpia).
- `AuthService.SolicitarDesbloqueoAsync(email)`: respuesta neutra; **solo** envía correo si la
  cuenta existe, está activa y `EstaBloqueado()`. El enlace apunta a
  `/desbloquear-cuenta?token=…` (1 h de validez).
- `AuthService.DesbloquearCuentaAsync(token)`: valida el token, llama a `Usuario.ResetearFallos()`
  (levanta `LockoutEnd` y limpia `AccessFailedCount`), guarda y consume el token. Conserva la
  contraseña. Queda registrado en logs de auditoría.
- Endpoints `AllowAnonymous`: `POST /api/v1/auth/solicitar-desbloqueo` y
  `POST /api/v1/auth/desbloquear-cuenta`.
- Frontend: en el login, enlace **"¿Cuenta bloqueada?"** junto a "¿Olvidaste tu contraseña?"; abre
  un formulario que pide el correo y envía el enlace. Página nueva `DesbloquearCuentaPage`
  (ruta pública) que consume el token al abrirse y muestra éxito o error. `auth.service.ts` expone
  `solicitarDesbloqueo` y `desbloquearCuenta`.

**Seguridad.** El desbloqueo exige recibir el correo (control del buzón); por sí solo no da acceso
(sigue hace falta la contraseña), así que no debilita la protección anti fuerza bruta frente a
quien no controla el correo. Mensajes neutros para no revelar si un correo existe o está bloqueado.

**Verificación.**
- Build Release **sin advertencias ni errores**; `tsc -b && vite build` correcto; lint del frontend
  limpio; EF **sin cambios de modelo pendientes**.
- Pruebas: **201 en verde, 0 fallidas** (Domain 34, StationMonitor 2, Detectors 110, **API 55**).
  7 pruebas nuevas en `AccountUnlockServiceTests`: el servicio de tokens (validar/expirar/un solo
  uso/unicidad) y el cableado real de `AuthService` con EF InMemory (desbloquea una cuenta
  bloqueada y consume el token; token inválido → `false`; el correo de desbloqueo se envía **solo**
  si la cuenta está bloqueada).
- Interfaz (Chrome): el login muestra el enlace "¿Cuenta bloqueada?", abre el formulario de correo
  y la página `/desbloquear-cuenta` renderiza y maneja el token inválido. (El API de desarrollo en
  ejecución debe reiniciarse para servir los endpoints nuevos: no recarga en caliente.)

**Archivos:** `AccountUnlockService.cs` (nuevo), `AuthService.cs`, `IAuthService.cs`,
`AuthController.cs`, `DependencyInjection.cs`, `DTOs/Auth/LoginResponse.cs`,
`frontend/.../LoginPage.tsx`, `DesbloquearCuentaPage.tsx` (nuevo), `auth.service.ts`, `App.tsx`,
`tests/PetrolRios.Api.Tests/AccountUnlockServiceTests.cs` (nuevo).

## 40. Arquitectura de los detectores: una clase por regla (Strategy + auto-registro)

**Motivación.** La tesis declara el **Strategy Pattern** para los detectores, pero cada uno de los
4 detectores era un monolito con toda su lógica de reglas en línea. Eso contradecía el patrón
declarado y hacía que agregar una regla obligara a editar un archivo grande y delicado. Se
refactorizó para que **cada regla sea su propia estrategia**.

**Diseño.**
- `IDetectionRule` / `DetectionRuleBase`: contrato de una regla individual (a qué detector
  pertenece, su parámetro/umbral configurable, su ámbito por defecto y su método `Evaluar`).
- `RuleBasedDetector` (base abstracta): orquestador delgado. Recibe por **inyección de
  dependencias** todas las `IDetectionRule`, filtra las de su `Type`, **respeta el flag `Activa`**
  de cada `ReglaDeteccion` y acumula las anomalías. Cada detector concreto queda en ~10 líneas
  (constructor + `Type`).
- **Auto-registro por reflexión:** la capa de DI escanea el ensamblado y registra cada
  `IDetectionRule` automáticamente. **Agregar una regla = agregar un archivo** (una clase nueva);
  no hay que tocar el detector, ni la DI, ni un `switch`. Escalable y modular.
- 25 reglas repartidas en `Rules/<Detector>/` (8 Invoice, 6 Compliance, 5 Cash + helper, 5 Payment).

**Vínculo con la tesis.** Materializa el patrón Strategy declarado en el diseño y refuerza los
atributos de **mantenibilidad y escalabilidad** del sistema (OE de calidad). El comportamiento de
detección es idéntico al previo: es una refactorización estructural, no funcional.

**Verificación.** Build Release **0 advertencias / 0 errores**; **119 pruebas de Detectors en
verde**; interfaz de Reglas sin cambios para el usuario (mismas 25 reglas, mismos umbrales).

**Archivos:** `RuleBasedDetector.cs` (nuevo), `Rules/DetectionRuleBase.cs`,
`Rules/<Detector>/*.cs` (25 clases, una por regla), `DependencyInjection.cs` (auto-registro),
los 4 detectores reducidos a orquestadores. *(commits `25a9e30`, `b37d216`)*

## 41. Motor de reglas personalizadas 2.0

La pantalla de **Reglas personalizadas** (que permite a Supervisor/Admin definir reglas sin tocar
código — CU de configuración del motor) se llevó a un nivel de producto. Cuatro mejoras + una de
robustez:

**41.1 Vista previa / backtest antes de guardar (dry-run).** Inspirado en motores de reglas reales
(estilo *Stripe Radar*): antes de guardar una regla se la prueba contra los **últimos N días** de
datos de staging y se ve cuántas transacciones habría marcado y con qué nivel de riesgo, **sin
persistir nada**. Reusa el `CustomRuleDetector` real (mismo resultado que en producción), lee
staging en **solo lectura** por ventana de fecha. Endpoint `POST /api/v1/reglas-personalizadas/backtest`;
panel de resultados en la UI (registros evaluados, coincidencias por nivel Bajo/Medio/Alto/Crítico
y muestra de ejemplos). Evita publicar reglas ruidosas o inertes.

**41.2 Combinador lógico Y/O en el modo básico.** Las condiciones del modo básico podían combinarse
solo de forma implícita. Ahora el usuario elige **"Cumplir TODAS (Y)"** o **"CUALQUIERA (O)"**.
Se persiste como JSON `{"combinador","condiciones"}`, **retrocompatible** con el array plano legado
(se interpreta como "Y"). *Verificado en vivo por backtest:* con **Y** y dos condiciones (una casi
imposible) → 0/24 coincidencias; con **O** y las mismas condiciones → 16/24. El round-trip
serializar→persistir→leer→evaluar es correcto.

**41.3 Funciones nuevas del DSL avanzado.** El modo avanzado (mini-lenguaje de expresiones seguro,
sin `eval`, con tokenizer/parser/AST propios) ganó funciones: `min`, `max`, `piso`, `techo`,
`modulo` (división segura), `raiz`, `potencia`, `en(x, …)` (pertenencia a lista), `siVacio`
(coalescencia), `redondear`, `mayusculas`, `termina`. Da expresividad sin sacrificar la seguridad
de no ejecutar código arbitrario.

**41.4 Galería de plantillas de arranque.** 5 plantillas pre-armadas (un clic las carga en el
formulario) para que el usuario parta de un ejemplo en vez de una hoja en blanco.

**41.5 (robustez) Modal de confirmación con estilo** en lugar del `confirm()` nativo del navegador.
Además de verse acorde al producto, elimina un cuelgue del hilo de render que el `confirm()`
bloqueante provocaba al borrar reglas.

**Vínculo con la tesis.** Refuerza los casos de uso de **configuración de reglas/umbrales** del
Supervisor y el Administrador y el principio de un **motor de detección configurable** sin
redepliegue.

**Verificación.** Build Release 0/0; `tsc -b && vite build` limpio; **119 pruebas de Detectors**
(3 nuevas: combinador "O", funciones matemáticas, `en`/`siVacio`); E2E del backtest con 4 casos
(coincidencia, sin coincidencia, regla inválida → error de validación, expresión avanzada).
*(commits `9746bd0`, `33392d2`, `da3c34f`, `e763c8f`)*

**Archivos:** `ReglaBacktestService.cs` (nuevo), `Jobs/StagingJson.cs` (nuevo, compartido),
`ReglasPersonalizadas/Expresiones/*` (AST + evaluador), `CatalogoReglasPersonalizadas.cs`,
`CustomRuleDetector.cs`, `ReglasPersonalizadasController.cs`, DTOs de backtest,
`ui/ConfirmDialog.tsx` (nuevo), `reglas/ReglasPersonalizadasSection.tsx`, tipos y servicio del
frontend.

## 42. Evidencia de cobertura de pruebas (OE5 ≥ 80%)

Tras la refactorización a 25 reglas se **midió** la cobertura real del ensamblado crítico, no solo
se contó pruebas. Herramienta: `coverlet` + `reportgenerator` (`scripts/coverage.ps1`).

**Resultado — `PetrolRios.Detectors`:**

| Métrica | Valor |
|---|---|
| Cobertura de **líneas** | **96.3%** (1169 / 1213) |
| Cobertura de ramas | 84.9% (339 / 399) |
| Cobertura de métodos | 88.9% (145 / 163) |
| Clases medidas | 33 |
| Pruebas | 119, todas en verde |

Los 4 detectores y el orquestador quedan al **100%**; cada una de las 25 reglas individuales entre
**91% y 100%**. **Supera con holgura el umbral del 80% exigido por OE5.** Reproducible en cualquier
momento con `scripts/coverage.ps1` (o `dotnet test … --collect:"XPlat Code Coverage"`); el reporte
HTML por clase queda en `coverage-report/` (ignorado por git como artefacto de build).

**Vínculo con la tesis.** Es la evidencia directa del **OE5** ("cobertura de pruebas unitarias > 80%
en el módulo de detección").

## 43. Nombre del empleado en las alertas (no solo el código)

Para poder **actuar de inmediato**, las alertas que involucran a un despachador ahora muestran su
**nombre** junto al código (antes solo se veía `COD_VEND`, p. ej. `V001`).

**El reto:** el nombre no existe en el central — a PostgreSQL solo llega el código dentro de cada
factura/turno; el nombre vive solo en Firebird (`VEND.NOM_VEND`, llave `COD_VEND`; respaldo
`EMPL.NOM_EMPL` vía `VEND.NUM_EMPL`). Hubo que traerlo.

**Diseño (catálogo + resolución, sin tocar las reglas):**
- **Catálogo central `Empleado`** (`EstacionId`, `Codigo`, `Nombre`) con índice único
  `(EstacionId, Codigo)` y migración `AgregarCatalogoEmpleados`. El **agente lo sincroniza** desde
  Firebird cada ~6 h (`FirebirdExtractor.ObtenerEmpleadosAsync`: `TRIM(COD_VEND)` →
  `COALESCE(NULLIF(NOM_VEND,''), NOM_EMPL)`) y lo envía a `POST /api/v1/empleados/sync` (mismo
  aislamiento por estación que la ingesta).
- **`IEmpleadoDirectorio`** resuelve `(estación, código) → nombre` (cruce por TRIM + mayúsculas). Se
  usa al construir `AlertaResponse` (lista, detalle, cambio de estado, problemas de estación), en el
  **top-empleados del dashboard** y en los **reportes PDF/Excel** ("Nombre (código)").
- **No toca ninguna de las 25 reglas** ni el pipeline de detección: el código sigue siendo la llave
  inmutable guardada en la alerta; el nombre es la resolución legible y se aplica **también a las
  alertas ya existentes** (resolución al leer). Sin match, se muestra solo el código.
- **Frontend:** Alertas, Detalle y Dashboard muestran el nombre con el código debajo; tipos actualizados.

**Verificación.** Build Release 0/0; migración `AgregarCatalogoEmpleados` (tabla `empleados` + índice
único + FK a estaciones); **tests en verde** (Domain 40, Detectors 119, Monitor 2, API 53 + 16 de
integración saltadas sin Docker); frontend (`tsc -b && vite build`) limpio. 6 pruebas nuevas
(`EmpleadoTests`, `EmpleadoDirectorioTests`).

## 44. Datos demo coherentes para que el nombre del empleado aparezca

Al probar en vivo, las alertas seguían mostrando **solo el código**. La causa **no era el código**
(la resolución de §43 funciona): era un desfase en los **datos de prueba**. Las ventas anómalas de
`inserciones_anomalias.sql` estaban atribuidas a despachadores `EMP-001…EMP-009`, **códigos que no
existen en `VEND`** — la tabla real usa `COD_VEND CHAR(3)` y en la BD demo solo había 4 despachadores
(`001 Almacén`, `002 Oficina`, `003 Luis Sotomayor`, `11 San Jacinto`). El agente sincronizaba bien
ese catálogo, pero ningún `EMP-xxx` cruzaba → no había nombre que mostrar.

**Diagnóstico (en la máquina):** se consultó el catálogo `empleados` (PostgreSQL), los `EmpleadoCodigo`
de las alertas y la estructura/contenido real de `VEND` (Firebird) — confirmando el desfase de códigos.

**Arreglo (coherencia de datos demo, sin tocar el motor):**
- Nuevo `_arranque/inserciones_vendedores_demo.sql`: da de alta en `VEND` despachadores con **códigos
  reales de 3 caracteres** (`004…010`) y nombres realistas (`UPDATE OR INSERT`, idempotente). Se
  enganchó en `96_insertar_anomalias_firebird.bat` para que cada demo siembre los despachadores.
- `inserciones_anomalias.sql`: cada escenario ahora apunta a un despachador real (`003…010`) en vez de
  `EMP-xxx`, así cada anomalía queda atribuida a un nombre distinto.

**Verificación en Chrome (E2E real).** Tras regenerar las alertas, la lista y el detalle muestran
`Nombre (código)`: *Luis Sotomayor (003)* en la alerta de placa genérica, *María Quiñónez (005)* en
despachos rápidos, *Washington Bravo (008)* en venta sin placa, etc. — las 11 alertas con su nombre.
En producción los `COD_VEND` reales ya coinciden con `VEND`, por lo que esto solo corrige el set de
demostración.

---

## 45. Escalabilidad real: alta de estaciones (>10) + rol "Agente" propio (seguridad)

**Motivación.** El sistema no debe quedar amarrado a 10 estaciones fijas: PetrolRíos puede crecer.
Además se detectó un riesgo de seguridad: los usuarios-agente de estación usaban el rol **Auditor**,
lo que les habría permitido entrar a la aplicación central. Un agente es una **cuenta de servicio**,
no una persona — debe tener su propio rol sin acceso al sistema de auditoría.

**Alta de estación de dos formas (ambas pedidas por el usuario):**
- **Botón "Nueva estación"** (solo Administrador) en la página de Conexiones: un modal crea la
  estación **y** su usuario-agente de una sola vez y muestra las credenciales **una única vez** para
  configurar el agente de esa estación. Endpoint `POST /api/v1/estaciones`
  (`CrearEstacionRequest` → `ProvisionarEstacionResponse` con email y contraseña del agente). Si no se
  indica contraseña, se genera una aleatoria legible (`GenerarPassword`, prefijo `Ag-`).
- **Desde "Nuevo Usuario":** el formulario acepta un **código de estación nuevo**
  (`CodigoEstacionNueva`); `UsuarioService.CreateAsync` hace *find-or-create* de la estación por
  código (`ResolverEstacionAsync`). Así se crean el agente y su estación en un solo paso.

**Rol "Agente" separado (seguridad):**
- Nuevo rol **`Agente`** sembrado en `SeedData` (`SeedRolesAsync`); los usuarios `agent-*` se crean
  con este rol, no con Auditor.
- **Repunte idempotente** (`EnsureRolAgenteAsync`): en bases ya existentes crea el rol si falta y
  **reapunta todos los `agent-*` de Auditor→Agente** (`ActualizarPerfil(null, agenteRol.Id)`).
  Verificado en vivo: 10 agentes migrados, rol Agente creado (Id 4).
- **La política "Central"** (la que protege la app de auditoría) ahora excluye al rol Agente además
  de a quien tenga claim `EstacionId`: `!HasClaim(EstacionId) && !IsInRole("Agente")`. Un agente puede
  autenticarse para enviar datos, pero **no** puede entrar al dashboard ni a la bandeja central.

**Verificación.** Build Release 0/0; gate completo en verde (Domain 40, Detectors 119, Monitor 2,
Api 53 + 16 de integración saltadas sin Docker; EF sin cambios pendientes; lint y build de frontend
limpios). E2E en Chrome: se creó una estación con su agente, las credenciales se mostraron una sola
vez, y el rol del agente quedó como `Agente`. *(commits `2d3d12a`, `bf8782a`)*

---

## 46. Robustez del agente y del gate de pruebas

Tres correcciones encontradas al preparar producción:

- **El contador "transacciones enviadas" no contaba los reenvíos.** Al reenviar lotes pendientes del
  store-and-forward, el total del panel no se incrementaba. `CycleRunner`: tras
  `RetrySendPendingBatchesAsync`, si se enviaron pendientes se suman a `TotalTransaccionesEnviadas`.
  (No era un bug de duplicación de alertas — se confirmó que las filas eran genuinamente distintas;
  solo el contador estaba mal.)
- **Guardado del panel del agente endurecido.** `guardarFuentes` apuntaba al endpoint equivocado;
  ahora usa `/api/fuentes`. El guardado de configuración mueve la construcción del payload **dentro**
  del `try` y muestra mensajes de error visibles si algo falla (antes "no parecía guardar" sin avisar).
- **Gate de verificación arreglado** (`scripts/verificar-mejoras.bat`): (1) detiene los servicios
  antes de compilar (`taskkill` de Api/Agent/Monitor) — sin esto el build fallaba con `MSB3027`;
  (2) asegura la herramienta `dotnet-ef` (`|| dotnet tool install`) — en frío el chequeo de migración
  fallaba; (3) reescrito para mostrar el **progreso en vivo, paso a paso**, con `pause` al final, en
  vez de redirigir todo a un log y dejar la ventana "en negro" hasta el final.

**Verificación.** Gate ejecutado de punta a punta en verde tras los cambios; panel del agente
guardando fuentes y configuración con confirmación visible; dist del agente republicado (v2.3.0).
*(commits `a50a79f`, `e4146c7`)*

---

## 47. Validación de contraseña del agente al crear estación + prueba E2E (EST-777)

**Bug detectado por el usuario.** Al crear una estación con contraseña `1234`, el sistema la
**aceptaba sin avisar**, pero el login exige mínimo 6 caracteres (`LoginRequestValidator`), así que el
usuario-agente quedaba **inservible**: creado, pero incapaz de autenticarse.

**Arreglo (frontend + backend, misma política que el login):**
- **Backend** (`EstacionesController.Crear`): rechaza con 400 y mensaje claro si `PasswordAgente`
  tiene menos de 6 caracteres, antes de crear nada.
- **Frontend** (`CrearEstacionModal` en `ConexionesPage`): valida en el `onClick` y muestra el error
  en rojo sin llamar a la API.

**Prueba E2E en Chrome (EST-777).** Se intentó crear `EST-777` con `1234` → el modal lo **rechazó**
con "La contraseña del agente debe tener al menos 6 caracteres…" (bug confirmado y corregido). Se creó
con `123456` → credenciales mostradas (`agent-est-777@petrolrios.com`) → login OK (rol **Agente**) →
se configuró el agente a EST-777 → **Autenticado en 182 ms**, **2 transacciones enviadas**,
0 pendientes, fuentes ANUL/TANQ_REPO **Sincronizada**. Flujo Firebird→agente→central probado de punta
a punta con una estación nueva. *(commit `c66fb56`)*

---

## 48. Verificación E2E del motor de reglas personalizadas con una regla creada por el usuario (despacho ≥400 gal)

Prueba de punta a punta de que una regla **definida por el usuario desde la interfaz** (no una de las 25
integradas) dispara a través del pipeline completo Firebird→agente→central.

- **Regla del usuario:** "Despacho execivo" (carril Auditoría, riesgo base 57), fuente **Despachos de
  combustible (DESP / `DetalleFactura`)**, condición **`Cantidad >= 400`** galones. El campo `Cantidad`
  del builder mapea a `DESP.CAN_DESP`.
- **Inserción en Firebird (nunca en PostgreSQL):** un despacho de **450 galones** en la tabla `DESP`
  (`FIN_DESP = CURRENT_TIMESTAMP`) vía isql en el contenedor `petrolrios-firebird`. Se respeta la regla
  de oro: los datos de prueba siempre entran por Firebird, para demostrar que el agente funciona.
- **Resultado (verificado en Chrome):** el agente de **EST-777** extrajo el despacho por watermark, lo
  empujó al central, y el `CustomRuleDetector` generó la **alerta #30 — Crítico, score 86** con evidencia
  legible: *Fuente DetalleFactura · Cantidad 450 · Condición `Cantidad >= 400` · Monto $1260 · Regla
  "Despacho execivo"*, atribuida a la estación Estacion Prueba 777.

Confirma la plataforma de detección configurable de extremo a extremo: el usuario crea una regla de
negocio sin tocar código y el sistema la evalúa sobre datos reales que entran por el agente. *(prueba de
verificación; sin cambios de código)*

---

## 49. Fix de producción: el .exe no arrancaba en Windows Server con CET / Shadow Stack

**Síntoma (en producción).** En el servidor de la estación, `PetrolRios.StationAgent.exe` abortaba al
arrancar con: `CLR: Assert failure ... !AreShadowStacksEnabled() ... threads.cpp`.

**Causa.** No es Windows Server en sí, sino que el CPU del servidor tiene **CET (Control-flow Enforcement
Technology)** y Windows tiene activada la **"Protección de pila reforzada por hardware" (Shadow Stack)**.
El runtime de .NET 9 tiene un defecto conocido en esa ruta y aborta el proceso. Es un problema
documentado de .NET 9 en equipos/SO con CET (frecuente en servidores y CPUs recientes Intel/AMD).

**Solución.** Compilar los ejecutables sin la marca CET, con **`<CETCompat>false</CETCompat>`** en el
`PropertyGroup` de los **3 proyectos que generan `.exe`**: `PetrolRios.StationAgent`,
`PetrolRios.StationMonitor` y `PetrolRios.Api`. Así Windows no fuerza shadow stacks sobre estos binarios
y arrancan en cualquier equipo. (El binario solo deja de usar esa protección concreta; conserva el resto
del endurecimiento.)

**Verificación.** Republicado el agente win-x64 self-contained single-file con `PUBLISH_EXIT=0`; el nuevo
`.exe` (~50 MB) arranca en local y retoma EST-777 (panel v2.3.0, configuración intacta). La validación
definitiva es en el servidor con CET: redeploy del `.exe` y arranque sin el assert.

**Workaround inmediato (sin recompilar), por si hay que destrabar el servidor al instante:** en el
servidor, en PowerShell como administrador —
`Set-ProcessMitigation -Name PetrolRios.StationAgent.exe -Disable UserShadowStack,UserShadowStackStrictMode`
(desactiva el shadow stack solo para ese proceso). El fix permanente es el `.exe` recompilado.

---

## 50. Creador de reglas más usable (1/2): documentación automática de campos

**Motivación (pedido del ingeniero).** En el creador de reglas, los campos se mostraban con su código
crudo de Contaplus (`AUG_DCTO`, `FEC_DCTO`, `CAN_DESP`…), así que un usuario no técnico no sabía qué
era cada uno ni cuál era fecha, monto o cantidad. En las reglas predefinidas no hay problema (el código
ya sabe qué es cada campo), pero el creador no daba esa ayuda.

**Solución — diccionario de datos + glosario de negocio, automático.** Cada campo ahora se documenta
solo, combinando tres fuentes en orden de prioridad (todo por datos, nada cableado por campo):
1. **Glosario curado** (`DiccionarioCamposContaplus`): los campos comunes y de más valor de Contaplus
   con etiqueta legible, rol y descripción exactos (DCTO, DESP, TURN, CRED, ANUL, CLIE/VEND, TANQ_REPO).
2. **Comentario del campo en Firebird** (`RDB$DESCRIPTION`) si la base lo trae (plomería lista en
   `ColumnaEsquemaRaw`; el agente puede enviarlo a futuro).
3. **Inferencia por prefijo** de Contaplus (`FEC_`/`FIN_` = fecha, `COD_` = código, `VAL_`/`VTO_`/`TNI_`
   = monto, `CAN_` = cantidad, `NOM_` = nombre, `PLA_` = placa, `RUC_` = identificación…).

Cada campo expone ahora **rol semántico** (Fecha, Monto, Cantidad, Código, Placa, Identificación,
Nombre, Estado, Número, Texto), una **descripción en español** y un **ícono** (📅 fecha, 💲 monto,
⛽ cantidad, 🏷️ código, 🚗 placa…). El endpoint `/reglas-personalizadas/catalogo` los devuelve por campo,
para las 5 fuentes conocidas (campos lógicos, rol inferido por palabras clave) y para cualquier **tabla
configurable** (campos crudos vía glosario + inferencia).

**Frontend.** En el builder, el selector de campos del modo básico muestra `📅 Fecha del documento`
(ícono + nombre legible) con la descripción en el tooltip; la paleta del modo avanzado muestra el ícono
junto al código y la descripción al pasar el mouse. El usuario ya no ve códigos crípticos.

**Escalable:** cubrir un campo nuevo = una entrada de glosario o un prefijo; el motor no cambia.

**Verificación.** Build Release **0/0**; **tests en verde** (Domain 40, Detectors 119, Monitor 2,
Api 69); `tsc -b && vite build` limpio. Sin migración (no toca la BD).

*(Parte 2 — juntar tablas y enriquecer la alerta con campos relacionados — va aparte.)*

---

## 51. Creador de reglas más usable (2/2): juntar tablas + enriquecer la alerta

**Motivación (pedido del ingeniero).** Una regla sobre despachos mostraba el dato del despacho pero no
"quién, qué placa, qué cliente, qué factura" — eso vive en la Factura, y el creador de reglas no podía
cruzar tablas (las reglas predefinidas sí, pero por código). Ahora el usuario trae campos de tablas
relacionadas y elige cuáles aparecen en la alerta, **sin tocar código**.

**Relaciones entre tablas (estilo lookup/linked fields).** Nueva entidad `RelacionTabla`
(origen→destino por un par de campos + etiqueta), migración `EnriquecimientoReglasYRelaciones`, y un
seed de la relación clave **Despacho (DetalleFactura) → Factura por código de cliente** (y la inversa).
El Admin gestiona más vía `GET/POST/PUT/DELETE /api/v1/relaciones-tabla` (`RelacionesTablaController`).

**Campos a mostrar en la alerta.** Nueva columna `CamposMostrarJson` en `ReglaPersonalizada` (lista de
"Campo" propio o "Fuente.Campo" relacionado). El `/catalogo` adjunta a cada fuente sus
`CamposRelacionados` (con el rol/ícono/descripción de la Parte 1). En la UI, un selector
**"Información a mostrar en la alerta"** (chips toggle) deja elegir placa, vendedor, cliente, N° de
factura, etc., además de los campos propios.

**Enriquecimiento en el detector.** `CustomRuleDetector`, al generar la alerta por registro, resuelve
los campos elegidos: para los relacionados busca la relación, **cruza en memoria** (el ciclo ya tiene
todas las tablas cargadas, igual que las reglas predefinidas) por la llave, y agrega el valor a la
evidencia con etiqueta legible (p. ej. "Placa (Factura)"). Tolerante: lo que no resuelve se omite y
nunca tumba el ciclo. El job carga las relaciones activas una vez por ciclo (`DetectionContext.Relaciones`).

**Escalable:** una relación nueva = una fila (seed o, a futuro, pantalla de Admin); el motor no cambia.

**Verificación.** Backend: build Debug+Release **0/0**, **migración generada sin cambios pendientes**,
**tests en verde** (Domain 40, Detectors 119, Monitor 2, Api 69). Frontend: `tsc -b && vite build`
limpio. *(commit backend `74b84f9`; frontend en este commit.)*

*(Mejora opcional pendiente: pantalla de Admin para el CRUD de relaciones desde la UI; la API ya existe
y las relaciones clave vienen sembradas, así que el feature funciona de punta a punta sin ella.)*

---

## 52. Creador de reglas: pulido del diccionario y filtros (feedback del ingeniero)

Tres ajustes sobre el creador de reglas:

- **Tooltip con el nombre técnico.** Los chips de "Información a mostrar en la alerta" y el selector de
  campos del modo básico muestran el nombre natural y ahora incluyen el **código real de la tabla**
  (p. ej. `FEC_DCTO`) —en el tooltip y, en el dropdown, también inline— para ver a la vez la palabra
  natural y la técnica. Así los campos sin nombre natural igual son entendibles.
- **Filtro por tipo en el modo avanzado.** La paleta de campos del modo avanzado tiene botones para
  filtrar por rol (📅 Fecha, 💲 Monto, 🏷️ Código, ⛽ Cantidad…), para encontrar rápido los campos de un
  tipo. Su tooltip muestra código + etiqueta + descripción.
- **Glosario ampliado.** Se agregaron los campos de DCTO con significado claro/estándar (importe, serie,
  guía, orden, dirección, bodega, grupo, voucher, liquidación, fecha de modificación, usuario que
  registró…) y prefijos nuevos (AUT/GUI/ORD/LIQ/VAU/SER). **No se inventan** los códigos internos opacos
  de Contaplus (ANE/AUG/BPA…): el esquema es propietario y no está documentado públicamente (verificado
  por búsqueda web), y un nombre incorrecto sería peor que el código; para esos, el tooltip técnico
  cubre la necesidad, y se agregan al glosario en cuanto se conozca su significado real.

**Verificación.** Build Release **0/0**; tests en verde; `tsc -b && vite build` limpio.

---

## 53. Autodescubridor de relaciones entre tablas (lo automático de verdad)

**Motivación (pedido del ingeniero).** Las relaciones entre fuentes estaban sembradas a mano (solo
Despacho↔Factura), así que al registrar una **tabla nueva no aparecían campos relacionados** para
enriquecer las alertas. El descubrimiento debía ser automático.

**Método — el estándar de *data profiling* para detectar claves foráneas** (verificado por investigación):
combina las tres señales que usa la literatura — **similitud de nombre de columna + inclusión/solapamiento
de valores + columna llave**:
- **`ConceptosRelacion`** (Application): catálogo de conceptos de llave (cliente, vendedor, turno,
  producto, banco, manguera, documento, tanque, placa, estación…) con sus variantes **lógicas y crudas**
  (`CodigoCliente` ↔ `COD_CLIE`), para cruzar fuentes que le llaman distinto a la misma llave.
- **`DescubridorRelacionesService`** (Infrastructure): arma candidatos (a) por **concepto compartido** y
  (b) por **nombre de columna llave idéntico** —lo que cubre tablas nuevas con códigos propios—, y los
  **valida con el solapamiento de valores reales en staging**: si dos columnas no comparten ningún valor,
  descarta el falso positivo. Crea las relaciones nuevas marcadas como **automáticas**
  (`RelacionTabla.EsAutomatica`); idempotente (no duplica). Migración `RelacionAutomatica`.
- **Disparadores:** corre **al arrancar el central** (best-effort, no bloquea el arranque) y por endpoint
  `POST /api/v1/relaciones-tabla/descubrir` (Admin) para re-ejecutar cuando lleguen datos nuevos.

**Resultado.** Una tabla nueva que comparte una llave con otra (cliente, turno, etc.) obtiene su relación
**sola**, y el creador de reglas ofrece sus campos relacionados sin que nadie defina nada. (Tanques sigue
sin empleado/factura porque no tiene esos campos — es correcto, no todo se relaciona con todo.)

**Verificación.** Build Debug+Release **0/0**; **tests en verde** (Domain 40, Detectors 119, Monitor 2,
Api 69); migración `RelacionAutomatica` sin cambios pendientes. *Base sublime:* el método de
inclusión de valores es el que recomienda la literatura de detección de FKs (SPIDER/Metanome).

---

## 54. Creador de reglas: buscador y filtro en el selector + gestión de relaciones

Tras el autodescubrimiento, el selector **"Información a mostrar en la alerta"** puede tener **cientos**
de campos (todos los relacionados de varias tablas). Para que siga siendo usable:

- **Buscador + filtro por tipo** en el selector: una caja de **búsqueda por texto**
  (nombre/etiqueta/descripción) y los botones de **rol** (📅 Fecha, 💲 Monto, 🏷️ Código, ⛽ Cantidad…),
  con la lista en un contenedor con **scroll** y un contador de "elegidos". Aparecen cuando hay más de 8
  campos, para no estorbar en fuentes chicas.
- **Panel "Relaciones entre tablas"** (colapsable, arriba de la sección) con un botón **"Descubrir
  relaciones"** que ejecuta el autodescubridor **on-demand** (sin reiniciar el central) y una lista de
  las relaciones (origen→destino, etiqueta, badge **auto**) con un botón para **podar** las que sobren.
  Usa `relacionesService` (`GET` / `POST descubrir` / `DELETE` de `/api/v1/relaciones-tabla`).

Con esto el usuario controla el ruido del autodescubridor: ve todas las relaciones, quita las que no
quiera, y encuentra cualquier campo en el selector aunque haya cientos.

**Verificación.** `tsc -b && vite build` limpio.

---

## 55. Problemas de estación: nombre del empleado + detalle clicable

Dos mejoras a la pestaña **"Problemas de estación"** (carril Operativa):
- **Nombre del empleado** (no solo el código): la lista ahora muestra **`👤 Nombre (código)`** resolviendo
  el nombre con el catálogo de empleados. El backend ya lo traía (`AlertaResponse.EmpleadoNombre`, resuelto
  por `IEmpleadoDirectorio` en `GetProblemasEstacionAsync`); faltaba mostrarlo en la UI (mostraba solo el
  código). Si no hay match, cae al código como antes.
- **Detalle clicable:** cada problema ahora abre su **detalle completo** (la misma vista de alerta,
  `/alertas/{id}`) al hacer clic, con un "Ver detalle →" al pasar el mouse.

*Nota técnica (la variable del empleado):* el código es **`Alerta.EmpleadoCodigo`**, que para un turno sin
cerrar viene del vendedor del turno (`CierreTurnoDto.CodigoVendedor` ← `TURN.COD_VEND` en Firebird). El
**nombre** se resuelve con `IEmpleadoDirectorio.Nombre(estación, código)` contra el catálogo central
`Empleado` (sincronizado por el agente desde `VEND.NOM_VEND`, con respaldo `EMPL.NOM_EMPL`), y se expone
como `EmpleadoNombre`.

**Verificación.** `tsc -b && vite build` limpio.

---

## 56. Lote de 6 mejoras (revisión del ingeniero, 25-jun) — etapa 1: alertas legibles + navegación

Ronda de **6 mejoras** pedidas en la revisión en vivo (capturas del 25-jun), a implementar en etapas
verificadas (código + Chrome + commit):

- **A** — Notificación por correo **por regla** (motor y personalizadas), no solo en críticas.
- **B** — Ajustes: ocultar la conexión a BD a los no-admin, **sección de operación** (nivel de correo
  + tiempo de disparo del job) y **tamaño de letra** accesible para todos.
- **C** — Editar reglas del motor mostrando la **unidad del umbral** (horas/galones/$/%/conteo) con
  tooltip + permitir el **doble carril** (Operativa **y** Auditoría a la vez).
- **D** — Abrir el detalle desde "Problemas de estación" **sin perder la pestaña**.
- **E** — **Leído/no leído por usuario** (si el admin ve una alerta, el auditor la sigue viendo nueva).
- **F** — Alertas de **reglas personalizadas legibles** + que muestren la **descripción** de la regla.

Esta sección cubre la **etapa 1 (D + F)**:

**F — Alertas de reglas personalizadas legibles.** Antes la alerta mostraba la condición en código
("`Cantidad >= '400'`") y **no** mostraba la descripción que el usuario escribió en la regla (solo se
veía en Reglas). Ahora `CustomRuleDetector` arma la descripción **liderada por `regla.Descripcion`** +
la condición en **lenguaje natural** (etiquetas del catálogo + operadores en palabras: "Despacho
(detalle de factura): Galones mayor o igual a 400") + el monto. La condición técnica exacta sigue en la
**evidencia** (`Condiciones`/`Expresion`) y se agrega `Qué detecta` = la descripción de la regla.
Aplica al modo por registro y al agregado. Helpers nuevos: `DescripcionLegible`, `FrasearCondiciones`,
`FrasearCondicion`, `OperadorEnPalabras`, `EtiquetaFuente`.

**D — Abrir el detalle sin perder la pestaña.** Desde "Problemas de estación", al abrir un problema y
volver, antes caías en `/alertas` (otra sección) y perdías el contexto. Ahora `ProblemasEstacionPage`
guarda la **estación expandida y el rango de días en la URL** (`?dias&g`) y navega al detalle pasando
`state.volverA` + `volverLabel`; `DetalleAlertaPage` usa ese origen, así el botón dice **"Volver a
problemas de estación"** y regresa a la lista con la estación **aún expandida**. Si vienes de la bandeja
de alertas, sigue diciendo "Volver a alertas".

**Verificación.** Build Release 0 warnings/0 errores; **119/119** pruebas de Detectors; `tsc -b &&
vite build` limpio. En Chrome (stack arriba): Problemas de estación → abrir "Turno 990001 sin cerrar"
(Alerta #19) → el detalle muestra **"← Volver a problemas de estación"** → al pulsarlo regresa con
"Estación Patricia Pilar" **aún expandida** (URL conserva `?g=…`). Etapas B/C/A/E pendientes.

---

## 57. Lote del 25-jun — etapa 2 (C): unidad del umbral + doble carril

Mejora al editor de las reglas del **motor** (las predeterminadas):

**Unidad del umbral.** Antes, al editar una regla solo se veía "umbral 18" sin saber si eran horas,
dólares, % o galones. Ahora cada regla muestra su **unidad** junto al número ("umbral 18 horas",
"umbral 50 USD ($)", "umbral 3 veces", "umbral 30 %", "1 = activado"…) y un **tooltip** que explica
qué representa y el parámetro técnico. Es metadato **derivado del parámetro** en `ReglaService`
(`UmbralMeta`, 20 entradas) → expuesto como `Unidad` + `AyudaUmbral` en `ReglaDeteccionResponse`;
**sin columna nueva ni migración** (los parámetros del motor son fijos).

**Doble carril (Operativa Y Auditoría).** Nuevo valor `AmbitoAlerta.Ambos`. El chip de carril en
Reglas ahora **cicla** Operativa → Auditoría → **Ambos** (violeta). Una alerta "Ambos" aparece en
"Problemas de estación" (se avisa a la estación) **y** en la bandeja del central: `AlertaService`
incluye `Ambos` en la consulta de problemas, `AnomalyDetectionJob` la suma al digest operativo y
emite los **dos** eventos SignalR (`ProblemaEstacion` + `NuevaAlerta`, cada uno con su NotificationId,
vía el nuevo `PublicarPushAsync`). Los detectores ya lo respetan (`DetectionRuleBase.Carril` devuelve
el ámbito; `CustomRuleDetector.AmbitoDe` y `ReglaPersonalizada.NormalizarAmbito` aceptan "Ambos"). El
enum se guarda como int, así que **tampoco hubo migración** (EF: "No changes to the model").

**Verificación.** Build Release 0w/0e; Domain **40** + Detectors **119**; `ef has-pending-model-changes`
= sin cambios; `tsc -b && vite build` limpio. En Chrome: cada umbral muestra su unidad (50 USD, 3
veces, 30 días, 30 %, 18 horas, 1 = activado); el chip de "Diferencia efectivo" cicló Auditoría →
Ambos (violeta, contador "Ambos: 1") → Operativa → Auditoría (restaurado). Etapas A/B/E pendientes.

---

## 58. Lote del 25-jun — etapa 3 (A): notificación por correo por regla

El correo ya no es solo para las críticas: ahora **cada regla** (del motor y personalizada) puede
**marcar "avisar por correo"** y, cuando se dispara, el sistema manda un correo a supervisores y
administradores.

**Mecanismo.** `DetectedAnomaly.NotificarCorreo` (settable). Los detectores del motor lo estampan en
**un solo lugar** (`RuleBasedDetector`: si `config.NotificarCorreo`, marca las anomalías de esa regla);
`CustomRuleDetector` lo estampa desde `regla.NotificarCorreo`. En `AnomalyDetectionJob`, tras crear la
alerta: si es crítica → correo de siempre; **si no, y la regla pidió correo → `NotificarReglaPorCorreoAsync`**
(mismos destinatarios: supervisores + administradores). Columnas nuevas `ReglaDeteccion.NotificarCorreo`
y `ReglaPersonalizada.NotificarCorreo` → migración `NotificarCorreoRegla`.

**UI.** En "Motor de detección", cada regla tiene un **botón de campana** (🔔 azul = activado / gris =
apagado) junto al interruptor de activa. En "Reglas personalizadas", el formulario tiene una casilla
**"Avisar por correo cuando se dispare"**. DTOs y servicios de ambos lados mapean `NotificarCorreo`.

**Verificación.** Build Release 0w/0e; migración `20260625175307_NotificarCorreoRegla` creada y EF
"sin cambios pendientes al modelo"; Domain **40** + Detectors **119**; `tsc -b && vite build` limpio.
En Chrome: la campana de "Transacciones duplicadas" pasó a **azul** (activada y persistida) y volví a
apagarla (restaurado). Etapas B/E pendientes.

---

## 59. Lote del 25-jun — etapa 4 (B): tamaño de letra + operación del sistema (+ permisos)

Los tres puntos del pedido de Ajustes:

**Permisos.** La sección "Conexión a la base de datos" ya estaba **limitada al Administrador**
(`{esAdmin && …}`); se confirma. La nueva sección de operación también es solo-Admin.

**Tamaño de letra (todos, accesibilidad).** Nueva tarjeta "Tamaño de letra" con **Normal / Grande /
Mayor**; escala el tamaño base (rem) de **toda** la interfaz vía `document.documentElement.style.fontSize`
(16/17/19 px). Preferencia por navegador (`SettingsContext.tamanoFuente`), se aplica al instante.

**Operación del sistema (solo Admin).** Nueva tarjeta para editar **desde qué nivel se avisa por correo**
(Bajo/Medio/Alto/Crítico) y la **frecuencia (cron) del job** de detección, sin recompilar. Se persiste en
`config/operacion.json` (mismo patrón que `ConexionStore`): `OperacionConfig` + `IParametrosOperacion` +
`ParametrosOperacionStore` + `ParametrosOperacionController` (GET/PUT, Admin). El job lee el nivel mínimo
cada ciclo (`NotificarNivelPorCorreoAsync` reemplaza el "solo críticas" fijo); el cron sale del store al
arrancar (`Program.cs`) y se **re-registra en vivo** al guardar (`RecurringJob.AddOrUpdate`). Sin migración
(es config en archivo, no entidad).

**Verificación.** Build Release 0w/0e; EF "sin cambios al modelo"; Domain **40** + Detectors **119**;
`tsc -b && vite build` limpio. En Chrome: "Grande" agranda todo el panel (restaurado a Normal); la tarjeta
"Operación del sistema" cargó "Solo críticas" + cron "* * * * *" desde la API. **Etapa E pendiente.**

---

## 60. Lote del 25-jun — etapa 5 (E): estado leído/no leído POR USUARIO

Cierra el lote de 6 mejoras. Antes, "Nueva" era el estado **global** de la alerta; no había forma de
saber **qué he visto YO** sin alterar lo que ven los demás. Ahora cada usuario tiene su propio
leído/no leído, independiente del resto.

**Backend.** Entidad `AlertaVista` (AlertaId, UsuarioId, FechaVista) + config con índice **único**
(AlertaId, UsuarioId) + migración `AlertaVistaPorUsuario`. `IAlertaService`/`AlertaService`:
`MarcarVistaAsync` (idempotente) + `GetVistasAsync`. `AlertasController`: `POST /alertas/{id}/marcar-vista`
y `GET /alertas/vistas`, ambos con el usuario del token (claim `NameIdentifier`).

**Frontend.** Al abrir el detalle, `DetalleAlertaPage` llama `marcar-vista` (me la marca a MÍ) e invalida
el set de vistas. La lista (`AlertasPage`) consulta `GET /vistas` y muestra un **punto azul "nueva para
ti"** en las alertas que aún no he abierto.

**Verificación.** Build Release 0w/0e; migración `20260625181924_AlertaVistaPorUsuario` + EF "sin cambios
al modelo"; Domain **40** + Detectors **119**; `tsc -b && vite build` limpio. En Chrome: todas las
alertas con punto azul; abrí la **#31** y al volver su punto **desapareció**, mientras la **#30** (no
abierta) lo conserva. **Lote de 6 mejoras (A–F) COMPLETO y verificado.**

---

## 61. Restablecer reglas del motor a predeterminados (+ fix de confirmación que congelaba la página)

Pedido en el pase de QA: un botón para **deshacer** los cambios de umbral/carril de un detector y
volver a los valores de fábrica.

**Backend.** `IReglaService.RestablecerDetectorAsync(tipoDetector)` + `POST /api/v1/reglas/restablecer/{tipoDetector}`.
La fuente de verdad de los defaults son las **propias reglas del motor** (`IDetectionRule.UmbralPorDefecto`
y `AmbitoPorDefecto`), inyectadas en `ReglaService` — sin duplicar valores ni migración. Resetea umbral +
carril + activa (todas salvo "fuera de horario") + `NotificarCorreo=false`. Para el único parámetro sin
clase propia (`FaltantesRecurrentesDias`) hay un default suelto.

**Frontend.** Botón **"Restablecer"** en la cabecera de cada grupo de detector (Reglas → Motor).

**Bug cazado y arreglado en el QA:** el botón usaba `window.confirm`, que **congela el renderer**
(la página quedó bloqueada y el CDP de Chrome dio timeout). Se reemplazó por una **confirmación en la
propia interfaz** ("¿Restablecer? · Sí, a fábrica · Cancelar" en línea), sin diálogo nativo.

**Verificación.** Build Release 0w/0e; EF "sin cambios al modelo"; Domain 40 + Detectors 119; `tsc -b &&
vite build` limpio. En Chrome: cambié "Diferencia efectivo vs sistema" de **50 → 999**, pulsé Restablecer
→ confirmación en línea (sin congelar) → "Sí, a fábrica" → **volvió a 50**.

---

## 62. Pase de QA E2E en Chrome (creación de regla + Firebird → agente → alerta)

Recorrido de QA cazando bugs, con datos reales insertados **en Firebird** (no Postgres) para probar
todo el pipeline del agente:

1. **Regla personalizada nueva** "QA - Factura efectivo alto valor" (Facturas/DCTO, `TotalNeto > 400`,
   Auditoría) creada desde el builder con **5 campos a mostrar** (Total, Forma de pago, Placa, RUC,
   Vendedor).
2. **Inserción en CONTAC.FDB** de una factura `SEC_DCTO=9900060`, `TNI_DCTO=500`, `COD_PAGO='EF'`,
   `PLA_DCTO='QAB9999'`, `COD_VEND='004'`, `FEC_DCTO=CURRENT_TIMESTAMP` (vía isql en el contenedor).
3. **Agente EST-001 (v2.3.0) en línea**: la tomó por watermark y la envió al central (Conexiones lo
   confirma: última ingesta reciente, 503 en 24 h). El Motor de Detección corrió el ciclo.
4. **Alerta #33 generada** ("Regla Personalizada", Crítico, score 84): descripción **legible**
   (`[QA - Factura efectivo alto valor] … · Factura: Total de la factura ($) mayor que 400 · monto
   $500.00`), **empleado JORGE MENDOZA (004)** resuelto, y la **evidencia muestra los 5 campos elegidos**
   (Total 500, Forma de pago EF, Placa QAB9999, RUC 1790000000001, Vendedor 004) + "Qué detecta".
5. **Clasificar** ("Tomar en Revisión" → estado **En Revisión**) y **comentar** (comentario guardado con
   autor + fecha) — ambos OK.

**Bug cazado y arreglado:** el botón Restablecer usaba `window.confirm` y congelaba el renderer →
reemplazado por confirmación en línea (CAMBIOS §61). Resto de funcionalidades sin bugs.

---

## 63. Asignación de alertas "al 1000%": correo al asignado + quién asignó + visible en detalle y lista + aviso en vivo

**Motivación (pedido de Steven).** El botón "Asignar a auditor" antes solo guardaba un registro
silencioso (`asignaciones_alerta`) y ponía la alerta En Revisión: **no avisaba a nadie, no se veía a
quién quedó asignada, y no registraba quién la asignó**. Pedido: que se envíe un correo al
supervisor/auditor asignado, que se muestre a quién está/fue asignada, "entre otras cosas, al 1000%".

**Qué se hizo.**

1. **Registrar quién asignó.** `AsignacionAlerta` gana `AsignadoPorId` (FK a `usuarios`, **nullable** para
   no romper asignaciones históricas) + nav `AsignadoPor`. `Create(alertaId, usuarioId, asignadoPorId,
   comentario)`. Migración **`AsignacionAsignadoPor`** (columna + índice + FK Restrict).
2. **Mostrar la asignación en las respuestas.** `AlertaResponse` gana `AsignadoAId`, `AsignadoANombre`,
   `AsignadoARol`, `AsignadoPorNombre`, `FechaAsignacion`. `AlertaService` carga la **última** asignación
   por alerta (`CargarAsignacionesAsync`: una consulta con `Include(Usuario.Rol)` + `Include(AsignadoPor)`,
   agrupada en memoria) y la inyecta en `MapToResponse` para la lista, el detalle y "Problemas de estación".
3. **Correo al asignado.** `AlertaService` ahora inyecta `IEmailNotificacionService`: al asignar, si el
   correo está habilitado y el usuario tiene email real (no `agent-`), se le envía un correo **dirigido
   solo a él** ("Se te asignó la alerta #N…", con estación, detector, nivel, descripción y quién la asignó).
4. **Aviso en tiempo real (SignalR).** Nuevo evento **`AlertaAsignada`** (a los grupos `auditores`/
   `supervisores`/`administradores`) con `AsignadoAId`/`AsignadoANombre` en el payload. El frontend
   (`NotificationProvider`) muestra un toast **personalizado al asignado** ("Te asignaron una alerta") y a
   los demás una confirmación, y refresca las bandejas. `IAlertaService.AsignarAsync` recibe `asignadoPorId`
   (claim `NameIdentifier`, helper nuevo `ClaimsPrincipal.GetUsuarioId()`) y **devuelve la alerta
   actualizada** (200 en vez de 204), para que la UI refresque al instante.
5. **Frontend.** Detalle: **banner** "Asignada a X (rol) · asignada por Y · fecha" + la tarjeta cambia a
   **"Reasignar alerta"** cuando ya hay responsable. Lista de alertas: el asignado aparece **bajo el estado**
   (ícono + nombre). Tipo `AlertaResponse` ampliado.

**Pruebas nuevas.** `AlertaServiceAsignacionTests` (EF InMemory + Moq): registra `AsignadoPor` y pone En
Revisión; **envía el correo solo al asignado** y emite el evento `AlertaAsignada` con su id; sin correo
habilitado no envía pero igual asigna y emite; **reasignar** deja ver al último responsable (historial de 2).

**Verificación.** Gate verde: Build Release **0w/0e**; migración generada y EF **"sin cambios pendientes"**;
tests **Domain 40 · Detectors 119 · Monitor 2 · Api 73** (con los 4 nuevos, 0 fallos); `eslint` limpio;
`tsc -b && vite build` OK. **En Chrome (E2E):** login por API como admin; alerta histórica #33 (sin
`AsignadoPor`) ya muestra a su responsable "Leonardo Andrade"; asigné la #32 a **Maria Fernanda Auditora**
→ 200 con `asignadoANombre`/`asignadoARol=Auditor`/`asignadoPorNombre="Administrador del Sistema"`/fecha;
el **detalle** muestra el banner "Asignada a Maria Fernanda Auditora (Auditor) · asignada por Administrador
del Sistema · …" y la tarjeta "Reasignar"; la **lista** muestra el asignado bajo el estado (#33 Leonardo
Andrade, #32 Maria Fernanda Auditora).

---

## 64. Reorganización total de scripts: todo bajo `ejecutables/`, por tipo, con nombres descriptivos y resumen

**Motivación (pedido de Steven).** Los scripts estaban dispersos (`scripts/`, `_arranque/`, `INSTALACION/`,
`frontend/`) con nombres crípticos (`05_firebird_demo.bat`, `paso.bat`), sin saber cuáles seguían vigentes.
Pedido: juntarlos **todos** en `ejecutables/`, ordenarlos por tipo, **renombrarlos descriptivamente**, poner
un **resumen al inicio de cada uno** y **limpiar los obsoletos**.

**Qué se hizo.**

1. **6 carpetas por tipo** bajo `ejecutables/`: `1-INICIAR-Y-DETENER`, `2-BASE-DE-DATOS-Y-DEMO`,
   `3-DIAGNOSTICO`, `4-VERIFICACION-Y-PRUEBAS`, `5-PUBLICACION-Y-DESPLIEGUE`, `6-INSTALAR-EN-NUEVO-PC`.
2. **Todo movido con `git mv`** (historial preservado): se vaciaron y eliminaron `scripts/`, `_arranque/` e
   `INSTALACION/`, y `frontend/start_dev.bat`. Nombres nuevos descriptivos (p. ej. `INICIAR_TODO.bat` →
   `iniciar-todo-el-sistema.bat`; `scripts/verificar-mejoras.bat` → `4-VERIFICACION-Y-PRUEBAS/verificar-todo-gate-oficial.bat`;
   `05_firebird_demo.bat` → `levantar-firebird-y-restaurar-contaplus.bat`).
3. **Resumen (cabecera) al inicio de cada script** (`REM`/`#` con bloque "RESUMEN (que hace este script)").
4. **Obsoletos eliminados:** `verificar_2fa.bat` y `verificar_ronda_fuentes.bat` (verificadores de rondas
   puntuales ya cubiertos por el gate) y 3 *wrappers* que solo llamaban a otro script
   (`2_insertar_ventas_anomalas`, `reparar_auth_firebird`, `verificar_build_y_tests`).
5. **Rutas relativas y cross-refs arregladas:** scripts que subían un nivel ahora suben dos (`%~dp0..` →
   `%~dp0..\..`, `Split-Path -Parent` → doble); `publicar-solo-el-agente` copia las fuentes desde la nueva
   ubicación; la cadena de Firebird (`restaurar-firebird-desde-cero`) llama a sus hermanos renombrados; etc.
6. **`.gitignore`:** se amplió para ignorar cualquier `firebird_data/` y la BD restaurada (`*.FDB`/`*.fdb`);
   se quitó del control la `CONTAC.FDB` que se había versionado por error. `dist/` (build) sigue ignorado y
   se regenera al publicar.
7. **Docs actualizados:** `ejecutables/LEEME.md` reescrito como índice (con mapa "antes → ahora");
   `README.md`, `INSTALACION.md`, `CONEXION_RED.md`, `docs/OPERACION.md`, `docs/CONECTIVIDAD-VPN.md` y el
   prompt de reinicio apuntan a las rutas nuevas.

**Verificación.** Movimiento confirmado por `git status` de Windows (todo como *renames* `R`, sin tocar
otros archivos); el PowerShell de cabeceras/rutas reportó **todos los reemplazos encontrados** (0
"NO-ENCONTRADO"); y el **gate oficial corrido desde su nueva ruta** quedó verde (build Release 0w/0e,
EF sin cambios pendientes, Domain 40 / Detectors 119 / Monitor 2 / Api 73, lint + frontend build OK).

---

## 65. Auditoría agente/reglas (San Pío): watermark por zona horaria + tolerancia de nombres en reglas

**Motivación.** En San Pío (estación real, UTC-5) el agente enviaba datos (vía la fuente configurable
`Dcto`) pero la **regla nueva no disparaba**, y los **4 detectores predeterminados** se quedaban sin datos
nuevos. Auditoría profunda (código + base central en vivo) → dos defectos reales, documentados en
`docs/DIAGNOSTICO-AGENTE-REGLAS.md`.

**FIX 1 — Watermark por reloj de Firebird (bug de producción crítico).** El extractor built-in avanzaba la
marca de agua global con `DateTime.UtcNow`, pero `FEC_DCTO`/`FIN_DESP`/`FFI_TURN` se graban con el reloj
**local** de la estación. En UTC-5, tras el primer envío la marca saltaba 5 h al futuro y **dejaba de
extraer** lo nuevo (congelaba `Factura`, `DetalleFactura`, etc. → los 4 detectores predeterminados se
quedaban ciegos). El demo no lo mostraba porque el contenedor Docker corre en UTC. Solución: la marca
ahora avanza con el **reloj del servidor Firebird** (`SELECT CURRENT_TIMESTAMP FROM RDB$DATABASE`,
`ResultadoExtraccionAgente.RelojFirebird`), serialización `Unspecified` con `RoundtripKind`, semilla
"1 h atrás en reloj de Firebird", y re-siembra automática si encuentra una marca antigua en UTC (`...Z`)
al actualizar el agente. (Archivos: `FirebirdExtractor.cs`, `CycleRunner.cs`.)

**FIX 2 — Tolerancia de nombres en reglas sobre fuentes configurables.** `CatalogoReglasPersonalizadas.GetValor`
hacía match **exacto** del nombre crudo (`TNI_DCTO`) en fuentes configurables; una regla escrita con el
nombre amigable (`TotalNeto`) no resolvía. Ahora resuelve con tolerancia: (1) exacto, (2) sin distinguir
mayúsculas/espacios, (3) **puente amigable→crudo** (`TotalNeto`→`TNI_DCTO`, `Subtotal`→`SUB_DCTO`,
`Cantidad`→`CAN_DESP`/`CAN_TURN_TARJ`, …). 5 pruebas nuevas (`CatalogoGetValorTests`).

**Causa de la confusión del "diccionario" (documentada):** el mismo total `TNI_DCTO` se mostraba como
"Total de la factura ($)" en la fuente built-in `Factura` y como "Total con IVA ($)" en la configurable
`Dcto` — dos nombres para la misma columna. La regla `#45` se creó sobre `Factura/TotalNeto` pero el dato
fluía como `Dcto/TNI_DCTO`. Con FIX 1 la fuente `Factura` vuelve a fluir (su regla dispara) y con FIX 2 una
regla sobre `Dcto` funciona con cualquier nombre.

**Verificación.** Gate verde: build Release 0w/0e; EF sin cambios pendientes; Domain 40 / **Detectors 124**
(+5) / Monitor 2 / Api 57 (+16 saltadas sin Docker); eslint + vite build OK. **Pendiente de validar en San
Pío mañana** (el desfase horario solo se reproduce en una estación real fuera de UTC; el demo es UTC).
Recomendación operativa anotada: quitar del selector la fuente `Dcto` duplicada una vez confirmado que la
built-in `Factura` fluye (evita doble staging).

---

## 66. Guard anti-duplicación: no registrar en el selector tablas que ya extrae un built-in

**Motivación (Steven).** El bug de San Pío nació de agregar **DCTO** (y **Anulaciones**/ANUL) en el selector
de tablas cuando esas tablas YA se extraen como fuentes built-in (`Factura`, `Anulacion`). Eso duplicaba el
dato (dos veces en staging, con nombres de campo crudos vs amigables) y confundía al crear reglas. Pedido:
"que no me deje agregar tablas que ya existan".

**Qué se hizo.**
- **Lista compartida** `FuenteDatosPolicy.TablasBuiltIn` (tabla cruda → fuente built-in): DCTO→Factura,
  DESP→DetalleFactura, TURN→CierreTurno, TURN_DEPO→DepositoTurno, ANUL→Anulacion, CRED_CABE→Credito,
  TURN_TARJ→TarjetaTurno. Helpers `TablaCubiertaPorBuiltIn` / `FuenteBuiltInDe`.
- **Central (guard):** `FuentesDatosController.ValidarDefinicionAsync` (crear y editar) **rechaza** registrar
  una tabla built-in con un mensaje claro ("La tabla 'DCTO' ya se procesa automáticamente como la fuente
  'Factura'…").
- **Agente (red de seguridad):** `FirebirdExtractor` **omite** del catálogo configurable las tablas built-in
  (por si quedaron registradas de antes), con log; así dejan de duplicarse sin borrar nada a mano.
- 11 pruebas nuevas (`FuenteDatosPolicyTests`).

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / **Detectors 135** / Monitor 2 /
Api 57 (+16 saltadas sin Docker), eslint + vite build OK.

---

## 67. Sección "Datos recibidos" en el central: logs crudos de lo que envían los agentes

**Motivación (Steven).** Hacía falta una vista para ver **la información cruda recibida de los agentes**
(sea anomalía o no), ordenada, con **filtro y buscador**, para confirmar que las tablas registradas en el
selector (las que se auto-enlazan a los agentes) realmente **están llegando** al central.

**Qué se hizo.**
- **Backend:** `DatosRecibidosController` (`GET /api/v1/datos-recibidos`) — lista paginada de
  `transacciones_staging` (lo crudo que envía el agente), ordenada por más reciente, con filtros por
  **tipo**, **estación** y **estado (procesada/sin procesar)** y **búsqueda por tipo**. Resuelve el nombre
  de la estación. `GET /datos-recibidos/tipos` da los tipos distintos para el desplegable. Solo lectura,
  cuentas del central (excluye agentes). DTO `DatoRecibidoResponse`.
- **Frontend:** página **"Datos recibidos"** (sección Monitoreo del menú): tabla con tipo, estación, fecha
  original, estado y **datos crudos expandibles** (JSON formateado); filtros (tipo/estación/estado) +
  buscador; paginación. Tipo + servicio + ruta + ítem de menú.

**Para qué sirve:** filtrar por "Tanques", "Dcto", etc. confirma de un vistazo si esa tabla del selector
está enviando datos a esta estación (lo que Steven pidió para validar el auto-enlace de tablas).

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / Detectors 135 / Monitor 2 /
**Api 73** (Docker arriba), eslint + vite build OK.

---

## 68. Robustez del creador de reglas: stress-test en Chrome + guard de longitud (bug 500 → 400)

**Motivación (Steven).** "Intenta romper el programa, vuélvete loco en la creación de reglas… el sistema
debe ser escalable". Se sometió el endpoint `POST /api/v1/reglas-personalizadas` a una batería agresiva de
casos límite desde Chrome (contra la API + Postgres reales), y se verificaron en vivo el **guard
anti-duplicación** (§66) y la **página de Datos recibidos** (§67).

**Qué se probó (25+ casos, 0 caídas salvo la documentada abajo).**
- Nombre vacío / solo espacios → 400. Riesgo 0 / 101 / negativo / `1e308` → 400. Fuente vacía → 400.
- Campo inexistente, operador inválido (`DROP TABLE`), función de agregación falsa (`HACK`),
  agrupación por campo que no existe → 400 (validación de catálogo).
- Expresión avanzada basura (`a OR OR b )(`) → 400; expresión válida → 201.
- **Inyección SQL** en el nombre (`'); DROP TABLE …; --`) → se guarda como literal (EF parametriza); la
  tabla **sigue viva** tras el intento (se re-consultó el total). XSS en un valor → se guarda como texto
  (React lo escapa al render). Emoji/Unicode (`🔥用户αβγ`) en el nombre → 201.
- **Escalabilidad:** regla con **1000 condiciones** → 201; **fuente configurable arbitraria** ("Tanques"
  con campo libre) → 201 (se valida en tiempo de ejecución, no al guardar). Body `{}` → 400.

**Bug encontrado y corregido.** Un **nombre de más de 150 caracteres** (o descripción > 500, expresión >
2000) **pasaba la validación y reventaba con HTTP 500** al guardar, porque las columnas de
`reglas_personalizadas` tienen límite (`Nombre` 150, `Descripcion` 500, `FuenteDatos` 50,
`ExpresionAvanzada` 2000) pero `Validar()` no lo comprobaba. **Fix:** se añadieron guards de longitud
espejo de la BD en `ReglasPersonalizadasController.Validar()` (usado por crear **y** actualizar), que ahora
devuelven un **400 limpio** ("El nombre no puede superar 150 caracteres.", etc.) en vez de un 500.

**Otras verificaciones en vivo (Chrome).**
- **Guard de tablas built-in (§66):** registrar `TURN_DEPO`/`CRED_CABE`/`TURN_TARJ` en el selector →
  400 con "ya se procesa automáticamente como la fuente 'DepositoTurno/Credito/TarjetaTurno'"; `DCTO`
  (que ya estaba registrada de antes) → 409 "ya está registrada"; `dcto` minúscula se normaliza y bloquea.
  Se confirmó la duplicación histórica que sospechaba Steven: 3214 filas tipo `Dcto` (selector) vs 13
  `Factura` (built-in); el agente ya **omite** la fuente duplicada (§65), así que no se re-procesa.
- **Datos recibidos (§67):** la página renderiza 3.241 registros reales de San Pío, con filtros
  (tipo/estación/estado), buscador y **filas expandibles** que muestran el JSON crudo del `DCTO`
  (COD_PAGO="001", COD_VEND="DD0000013", COD_CLIE="CZ0061065", DEE_DCTO="RECIBIDO POR EL SRI").
- **Autounidor de tablas (`DescubridorRelacionesService`):** revisado — descubre relaciones por similitud
  de nombre + solape de valores en el staging, con umbrales prudentes; bien implementado y escalable
  (no inventa relaciones sin evidencia de datos). Sin cambios.

**Prueba automatizada.** Nuevo test de integración `ReglasPersonalizadas_NombreDemasiadoLargo_DevuelveBadRequestNo500`
en `ApiIntegrationTests` (nombre de 200 caracteres → espera 400, nunca 500).

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / Detectors 135 / Monitor 2 /
**Api 74** (el test nuevo corrió con Docker), eslint + vite build OK. Regresión en vivo confirmada tras
reconstruir: nombre 200 → 400 ("El nombre no puede superar 150 caracteres."), nombre 5000 → 400,
descripción 600 → 400, regla válida → 201.

---

## 69. Logs "Datos recibidos": el tipo se muestra como "Nombre natural (TABLA técnica)"

**Motivación (Steven).** En "Datos recibidos" el tipo salía con el nombre interno (p. ej. `Factura`, `Dcto`)
y no se veía la **tabla** real de Contaplus. Pedido: mostrar **el nombre natural y, entre paréntesis, el
nombre técnico (la tabla)** — p. ej. `Factura (DCTO)`, `Anulación (ANUL)` — tanto en la columna como en el
desplegable del filtro, para tener las dos referencias a la vez. (De paso, Steven borró la fuente `Dcto`
duplicada del selector.)

**Por qué ayuda.** El built-in `Factura` y la (vieja) fuente `Dcto` apuntan a la **misma** tabla `DCTO`;
ver la tabla técnica deja claro de qué tabla salió cada registro y desmonta la confusión "¿Factura y Dcto
no eran lo mismo?".

**Qué se hizo.**
- **Backend:** nuevo `CatalogoTiposTransaccion` (Application/Fuentes) que traduce el tipo del staging a
  `(nombre natural, tabla técnica)`. Cubre las 7 fuentes built-in (`Factura`→DCTO, `DetalleFactura`→DESP,
  `CierreTurno`→TURN, `DepositoTurno`→TURN_DEPO, `Anulación`→ANUL, `Crédito`→CRED_CABE,
  `TarjetaTurno`→TURN_TARJ), **tolera variantes de agentes antiguos** (`Anulaciones`→ANUL) y la fuente
  `Dcto`→DCTO; para las **fuentes configurables** del selector el tipo es el nombre de la fuente y la tabla
  se toma del catálogo (`FuentesDatos.Nombre→Tabla`). `DatoRecibidoResponse` gana `TipoNatural` y `Tabla`;
  `GET /datos-recibidos/tipos` ahora devuelve `{ tipo, etiqueta }` (etiqueta = "Natural (TABLA)").
- **Frontend:** la columna **Tipo** muestra el nombre natural + la tabla en gris monoespaciado entre
  paréntesis; el **desplegable** del filtro usa la etiqueta (valor crudo para filtrar, texto legible).

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / **Detectors 150** (+15 del
nuevo `CatalogoTiposTransaccionTests`) / Monitor 2 / Api 74 (57 + 17 de integración omitidas sin Docker en
esa corrida), eslint + vite build OK.

**Bug de repo corregido (al commitear).** `DatoRecibidoResponse.cs` vive en `…/DTOs/Logs/`, y la regla de
`.gitignore` `[Ll]ogs/` (plantilla de Visual Studio para logs de runtime) **ignoraba esa carpeta de código**:
el DTO nunca se había versionado (el commit de §67 también lo omitió en silencio), así que un clon limpio no
compilaría. Se añadió una **excepción** en `.gitignore` (`!src/PetrolRios.Application/DTOs/Logs/**`) y se
versionó el DTO. Lección: cuidado con las reglas amplias `[Ll]ogs/`/`[Ll]og/` cuando hay carpetas de fuente
homónimas.

---

## 70. Agente: auto-detección de la base ContaGober + arranque automático al encender

**Motivación (Steven).** Dos pedidos sobre el agente, de cara al nuevo despliegue: (1) que el botón
**"Detectar Firebird automáticamente"** encuentre la base real de la estación,
`C:\Programas\ContaGober1\Datosc\CONTAB.FDB` (ojo: **CONTAB**, no CONTAC); y (2) que el agente **se levante
solo cada vez que se enciende el equipo**, idealmente con una opción dentro del propio agente.

**Qué se hizo.**
- **Auto-detección ampliada (`FirebirdExtractor.RutasCandidatas`).** Añadidas las rutas de **ContaGober**
  (`C:\Programas\ContaGober1\Datosc\CONTAB.FDB` y variantes) y las variantes **`CONTAB.FDB`** de las
  ubicaciones clásicas. Además ahora **escanea** (acotado, `IgnoreInaccessible`, profundidad limitada) las
  raíces típicas de instalación (`C:\Programas`, `C:\Program Files`, `C:\Program Files (x86)`, `C:\Conta`,
  `C:\` superficial) buscando cualquier `CONTA*.FDB`, así detecta instalaciones nuevas sin hardcodear cada
  una. El campo de ruta del panel ahora rotula "CONTAC.FDB / CONTAB.FDB".
- **Arranque automático al encender — dos vías ("las dos", como pediste):**
  - **Botón en el panel, SIN admin** (la fácil): nueva sección **"Arranque automático al encender"** en
    Configuración con un botón Activar/Desactivar. Backend `InicioAutomatico` + endpoints
    `GET/POST /api/inicio-automatico`: en Windows escribe/borra un lanzador `.vbs` en la **carpeta de
    Inicio** del usuario que levanta el agente **oculto** (sin ventana) al iniciar sesión. Con autologin =
    "al encender". Reversible con un clic, sin permisos.
  - **Servicio de Windows** (avanzada, arranca antes del login, pide admin): se mantiene el
    `instalar_agente_servicio.bat` que viaja en el portable; el panel lo explica en un desplegable y el
    `LEEME-windows.txt` documenta ambas opciones paso a paso.
- **Portable reconstruido** con todos los cambios acumulados (watermark por reloj de Firebird, nombres
  naturales en "Datos recibidos", auto-detección ContaGober, arranque automático) para las **4 plataformas**
  (Windows, Linux, macOS Intel y ARM) vía `publicar-solo-el-agente-multiplataforma.bat`.

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / Detectors 150 / Monitor 2 /
**Api 74** (con Docker), eslint + vite build OK. Publicación de las 4 plataformas completada en `dist/`.

---

## 71. "Frecuencia del análisis" a prueba de errores: desplegable en español + cron validado de verdad

**Motivación (Steven).** En Ajustes → "Operación del sistema", la **frecuencia del análisis** se editaba
como una **expresión cron cruda** (`* * * * *`): difícil de entender y fácil de romper sin querer. Era para
cambiar cada cuánto corre la detección de reglas. Pedido: hacerla clara y segura.

**Qué se hizo.**
- **Frontend (Ajustes):** el campo de cron crudo se reemplazó por un **desplegable en español** —
  "Cada minuto", "Cada 2/5/10/15/30 minutos", "Cada hora", "Cada 2 horas"— donde cada opción mapea a su
  cron por debajo, con una frase que explica lo elegido ("El sistema revisará las reglas cada 5 minutos…").
  Se conserva una opción **"Personalizado… (avanzado)"** que revela el campo cron con sus ejemplos, para
  quien sepa. Al abrir, si la frecuencia guardada coincide con una opción, se preselecciona; si es una
  expresión rara, entra directo en modo avanzado.
- **Backend (`ParametrosOperacionController`):** la validación dejó de ser "cuenta 5 campos" (que dejaba
  pasar crons inválidos y podía tirar **500** al re-registrar el job). Ahora **registra el job dentro de un
  `try/catch`**: Hangfire **parsea** el cron ahí y lanza si es inválido **antes de tocar el almacenamiento**,
  así que un cron malo devuelve un **400 limpio** sin romper el job vigente, y solo si el cron es válido se
  **persiste** (ya recortado). Mensajes guían a elegir una opción de la lista.

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / Detectors 150 / Monitor 2 /
**Api 75** (+1: test de regresión `Operacion_CronInvalido_DevuelveBadRequestNo500`, con Docker), eslint +
vite build OK.

---

## 72. Release 2.4.0 + "fábrica" de despliegue lista para San Pío (reemplazo y actualización)

**Motivación (Steven).** Antes de ir a San Pío: revisar y dejar lista toda la cadena de despliegue
(instaladores/portables del central, agente y monitor; base de datos; versionado; y la lógica de
actualización de cada sistema), para poder **reemplazar** el agente viejo o **actualizarlo** con la lógica
existente.

**Revisión (qué hay y funciona).**
- **Central + BD:** `instalar-central-windows.bat/.ps1` verifica Docker, genera `.env` con secretos
  aleatorios, detecta la IP y levanta `docker-compose.prod.yml` (Postgres + API en :8080, migraciones
  automáticas). Sólido. Alternativa sin Docker: el `.exe` self-contained (:5170 + Postgres externo).
- **Portables/exes:** `publicar-servidor-agente-y-monitor.bat` (los 3, + Inno Setup) y
  `publicar-solo-el-agente-multiplataforma.bat` (agente x4 SO). Versión única en `Directory.Build.props`.
- **Actualización:** el **monitor se auto-actualiza** (cada 6 h); el **agente solo avisa** y se aplica
  **con un clic** desde su panel (descarga → verifica SHA256 → intercambia exe → reinicia). El central
  publica el manifiesto en `/api/v1/agente/version` y sirve el exe en `/descargas`.

**Qué se hizo para dejarlo "todo listo".**
- **Versión 2.4.0** en `Directory.Build.props` (era 2.3.0): así la vía de actualización dispara y se
  distingue el build nuevo.
- **URL del manifiesto ahora ABSOLUTA desde el request** (`AgenteController.Absolutizar`): el
  `agente-version.json` lleva una URL **relativa** (`/descargas/…`) y el central la vuelve absoluta con su
  propio host → sin IP hardcodeada, funciona en cualquier red.
- **Volúmenes en `docker-compose.prod.yml`:** `central-config` (→ `/app/config`) y `central-descargas`
  (→ `/app/wwwroot/descargas`). Así el central Docker sirve el manifiesto + el exe **sin reconstruir la
  imagen**, y además **persisten** `connection.json` y `operacion.json` (antes se perdían al recrear).
- **`publicar-actualizacion-del-agente.bat`/`.ps1` (un clic):** calcula el SHA256 del exe publicado, lo
  copia a `central-descargas/` y genera `central-config/agente-version.json`. Quita el paso manual y
  propenso a errores.
- **`docs/RUNBOOK-PUESTA-EN-MARCHA.md`:** guía única (central, agente, monitor, versionado/actualización y
  el plan de San Pío con las dos opciones). `agente-LEEME-windows.txt` ya cubría el arranque automático.
- `.gitignore` ignora `central-config/` y `central-descargas/` (artefactos de runtime).

**Verificación.** Cadena completa en Windows: **gate verde** (build Release 0w/0e, Domain 40 / Detectors
150 / Monitor 2; las de integración Api se omiten sin Docker en esa corrida), **publicación de las 4
plataformas** OK, y **`publicar-actualizacion`** generó `central-descargas/PetrolRios.StationAgent.exe` +
`central-config/agente-version.json` para la **2.4.0** (SHA256 `fda86354…baabe`). Quedan listas las dos
vías para San Pío: portable nuevo (reemplazo) y paquete de actualización (un clic).

---

## 73. Bug grave: el selector/buscador de "campos a mostrar" se rompía (keys duplicadas)

**Motivación (Steven).** En el creador de reglas, sección **"Información a mostrar en la alerta"**: al
escribir en el buscador (p. ej. `sec_dcto`) y borrar con backspace, o al seleccionar varios campos, **el
buscador, el filtro por tipo y los chips dejaban de responder** y no se podía seleccionar nada.

**Causa raíz (confirmada en el backend).** `ReglasPersonalizadasController` adjunta a cada fuente los campos
de sus tablas **relacionadas** con el nombre `"Destino.Campo"`, recorriendo **todas** las relaciones activas
y agregando **todos** los campos del destino **por cada relación**. Si una fuente tenía **varias relaciones
a la MISMA tabla** (el autodescubridor las crea con facilidad), el mismo campo (`Dcto.ANE_DCTO`) entraba
**repetido**. En el frontend el `.map` de los chips usa `key={c.nombre}` → **key de React duplicada** → al
re-renderizar (filtrar, borrar, seleccionar) el reconciliado se corrompe y el buscador/los chips dejan de
responder.

**Qué se hizo (dos capas).**
- **Backend (raíz):** se **deduplican** los campos relacionados por `Nombre`
  (`GroupBy(c.Nombre).Select(g.First())`) antes de adjuntarlos al catálogo — un campo es el mismo dato sin
  importar por cuántas relaciones se alcance. Sana a todos los consumidores de la UI.
- **Frontend (defensa):** en el picker, `todos` se deduplica por `nombre` antes de filtrar/renderizar, para
  que jamás haya una key repetida aunque el catálogo cambie.

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / Detectors 150 / Monitor 2 /
**Api 75** (con Docker), **eslint + tsc + vite build OK**.

---

## 74. Documentación: frecuencia por regla (propuesta anotada) + guía de relanzamiento

**Motivación (Steven).** (1) Dejar **anotado y bien documentado** el feature que pidió el ingeniero —"un
parámetro para poner cada qué tiempo se ejecuta cada regla"— con su alcance/dificultad, **sin
implementarlo aún**. (2) Tener por escrito cómo **relanzar cada componente** para la puesta en producción.

**Qué se hizo (solo documentación, sin tocar código del producto).**
- **`docs/PROPUESTA-FRECUENCIA-POR-REGLA.md`** (nuevo): documento de diseño/alcance de la frecuencia por
  regla. Conclusión investigada: **cambio mediano, no enorme**. El "campo de frecuencia + saltarse ciclos"
  es trivial; lo no-trivial es que las reglas lentas (semanal/mensual) necesitan su **ventana de datos**, no
  el lote incremental (hoy el job marca `Procesada` tras una pasada). **Pero** la maquinaria clave ya existe:
  `IReglaBacktestService` ya evalúa una regla sobre los últimos N días de staging, y la **idempotencia**
  evita alertas duplicadas al re-escanear. Plan por fases (1: throttle por-transacción; 2: ventana real
  reusando backtest), entidades/migración/UI/endpoints afectados, riesgos y alcance acotado.
- **`docs/RUNBOOK-PUESTA-EN-MARCHA.md`** (sección 6 nueva): **relanzar/reiniciar cada componente** —central
  Docker (`restart`, `up -d --build`, `down`/`up`, logs), estación (agente/monitor), y los scripts de
  desarrollo (`reiniciar-solo-la-api`, `reiniciar-solo-el-frontend`, etc.).

**Verificación.** Solo documentación: no cambia código ni build. (El feature de frecuencia por regla queda
**anotado**, pendiente de decisión para implementar.)

---

## 75. Frecuencia/calendario por regla — Etapa 1: el cerebro (modelo + Cronos + pruebas)

**Motivación (Steven).** Implementar el feature del ingeniero "cada regla con su propia cadencia", con un
selector en **segundos/horas/días/semanas/meses** y, además, **calendario anclado** ("el día 29 de cada
mes, incremental; el sistema instalado a mitad de mes no debe desfasarse").

**Investigación previa (pedida por Steven).** Se revisó cómo lo hacen los schedulers serios:
- **Quartz** separa **SimpleTrigger** (intervalo fijo: "cada N min/semanas/meses") de **CronTrigger**
  (calendario: "día 29", "último día", "cada viernes"), porque cron **no** expresa bien "cada N
  semanas/meses". → Confirma el diseño de **doble modo**.
- **Cronos** (librería de HangfireIO, paquete NuGet) calcula la próxima ocurrencia de un cron y soporta
  **`L` = último día del mes**, años bisiestos y zona horaria. → Se usa para el modo Calendario en vez de
  reinventar la matemática de fechas (que es donde es fácil fallar).

**Qué se hizo (Etapa 1).**
- **`ProgramacionEjecucion`** (value object, `Application/Programacion`): modo `CadaCiclo` |
  `Intervalo` (N + unidad seg/min/h/d/sem/mes) | `Calendario` (diario/semanal/mensual con día del mes,
  "último día del mes", día de la semana y hora). Serializa a JSON; trae una `Descripcion()` legible.
- **`CalculadoraProgramacion`**: `EstaPendiente(...)` y `CalcularProxima(...)`. Intervalo = `desde + N`;
  Calendario = se traduce a cron y **Cronos** calcula la próxima fecha anclada (resuelve "día 29 incremental"
  y "fin de mes" sin desfase, aunque se instale a mitad de mes). + `DiasVentanaSugerida()` para la ventana
  de datos de reglas lentas (Etapa 3).
- Paquete **`Cronos` 0.8.4** añadido a `PetrolRios.Application`.
- **15 pruebas** (`CalculadoraProgramacionTests`): intervalos, mensual día-29 anclado, no-desfase a mitad de
  mes, último día con febrero (no bisiesto), semanal, diario, "¿le toca?", serialización y descripción.

**Verificación.** Gate verde: build Release 0w/0e, EF sin cambios, Domain 40 / **Detectors 165** (+15) /
Monitor 2 / Api 75, eslint + vite OK. (Pendientes: entidades+migración, integración en el job, DTOs/API y
UI — ver `docs/PROPUESTA-FRECUENCIA-POR-REGLA.md` y `docs/PENDIENTES.md`.)

---

## 76. Frecuencia/calendario por regla — Etapa 2: columnas + migración EF

**Qué se hizo.** A las dos entidades de regla (`ReglaDeteccion` y `ReglaPersonalizada`) se les añadieron 3
columnas: **`ProgramacionJson`** (JSON de `ProgramacionEjecucion`; vacío = "cada ciclo"),
**`ProximaEjecucion`** y **`UltimaEjecucion`** (UTC, nullable). Migración EF
`20260626145512_ProgramacionDeRegla`: en ambas tablas, `ProgramacionJson text NOT NULL DEFAULT ''` (así las
reglas existentes quedan en "cada ciclo", sin cambio de comportamiento) y los dos timestamps `timestamptz`
nullables. Down las elimina.

**Verificación.** Gate verde: build Release 0w/0e, **EF sin cambios pendientes tras la migración**,
Domain 40 / Detectors 165 / Monitor 2 / Api 75, eslint + vite OK.

---

## 77. Frecuencia/calendario por regla — Etapa 3: integración en el job (gate + ventana + avance)

**Motivación.** Cerrar el corazón del feature: que el ciclo de detección **respete la cadencia de cada
regla**. Antes, todas las reglas corrían en cada ciclo del job (cada minuto en dev). El ingeniero pidió que
"una regla que analiza las transacciones finales del mes no se ejecute todo el rato"; ahora cada regla decide
cuándo le toca y, cuando le toca, mira **su ventana de datos** (no solo el último lote).

**Qué se hizo.** `AnomalyDetectionJob` pasó a un modelo de **dos pasadas** por estación, gobernado por la
`ProgramacionEjecucion` de cada regla (Etapa 1) leída de `ProgramacionJson` (Etapa 2):

- **Gate de programación (antes del bucle).** Se clasifica cada regla del motor y personalizada: "cada ciclo"
  (default), "le toca" (su `ProximaEjecucion` ya venció) o "anclar" (programada sin `ProximaEjecucion` aún →
  se calcula y **espera su turno**, no corre este ciclo). Las reglas se cargan **sin tracking** para poder
  alternar el flag `Activa` en memoria sin tocar la BD.
- **Pasada A — incremental.** Corre solo las reglas "cada ciclo" sobre el lote **no procesado** y lo marca
  `Procesada` (comportamiento clásico, intacto). Las reglas del motor programadas se presentan con `Activa=false`
  (así el detector NO les aplica su umbral por defecto); las personalizadas programadas se excluingen de la lista.
- **Pasada B — por ventana.** Para cada regla programada a la que le toca, se construye su **ventana**
  `FechaOriginal ∈ (UltimaEjecucion ?? ahora−díasVentana, ahora]` (sin marcar `Procesada`) y se evalúa **solo
  esa regla**. Como la ventana **no se solapa** entre corridas (el avance de `UltimaEjecucion` mueve el borde),
  cada transacción la ve la regla una sola vez → **sin alertas duplicadas** (no hizo falta tocar el modelo de
  Alerta ni añadir idempotencia nueva).
- **Avance de programación (tras el bucle).** Las reglas que corrieron fijan `UltimaEjecucion = ahora` y
  recalculan `ProximaEjecucion`; las recién ancladas fijan su primera `ProximaEjecucion`. Se persiste una sola
  vez por ciclo (la próxima fecha es global a todas las estaciones). El modo Calendario usa la zona **Ecuador
  (UTC-5, sin DST)** vía Cronos, y todo se normaliza a UTC para Npgsql (`timestamptz`).

**Garantía de no-regresión.** Con todas las reglas en "cada ciclo" (el default de hoy), la Pasada A corre
exactamente como antes y la Pasada B no hace nada → comportamiento **idéntico**. El feature es opt-in por regla.

**Verificación.** Gate oficial verde a la primera: build Release **0 warnings / 0 errores**, Domain 40 /
Detectors 165 / Monitor 2 / **Api 77** (+2 pruebas nuevas del camino programado: "no le toca → no evalúa" y
"le toca → evalúa por ventana y avanza la próxima"; los 7 E2E previos siguen verdes = carril incremental
intacto), **EF sin cambios pendientes** (Etapa 3 no toca el esquema), eslint + vite OK. Faltan Etapas 4 (API/DTOs)
y 5 (UI).

---

## 78. Frecuencia/calendario por regla — Etapa 4: la API lee y escribe la programación

**Motivación.** Que la cadencia de cada regla se pueda **configurar y consultar por la API** (lo que la UI
de la Etapa 5 usará). Hasta ahora la programación vivía en la BD y la respetaba el job, pero no había forma
de fijarla ni de ver la próxima ejecución desde fuera.

**Qué se hizo.**

- **`ProgramacionDto`** (Application/Programacion): DTO de entrada/salida con los enums como **texto**
  (`Modo`, `IntervaloUnidad`, `CalendarioTipo`) para validarlos contra **listas cerradas** y ser robusto ante
  cualquier configuración de JSON. Trae `De`/`DeJson` (proyectar) y `TryConvertir` (validar + convertir a
  `ProgramacionEjecucion`): rechaza modo/unidad/tipo desconocidos y rangos inválidos (N≥1, hora 0–23, minuto
  0–59, día-semana 0–6, día-mes 1–31) con un mensaje en español → **400 limpio, nunca 500**.
- **Reglas del motor** (`ReglaDeteccionResponse` + `ActualizarReglaRequest` + `ReglaService`): la respuesta
  ahora incluye `Programacion` + `ProximaEjecucion` + `UltimaEjecucion`; el `PUT` acepta `Programacion`
  (opcional). Al cambiarla se guarda el JSON y se **reinicia `ProximaEjecucion = null`** para que el job la
  ancle (el "día 29" empieza el día 29, no al instante). `ReglasController.Update` captura la validación como
  **400**.
- **Reglas personalizadas** (`ReglaPersonalizadaResponse` + `GuardarReglaPersonalizadaRequest` + controlador):
  mismos 3 campos en la respuesta; `Create`/`Update` aceptan `Programacion` (opcional, validada). Semántica
  segura: en `Create` `null` = "cada ciclo"; en `Update` `null` = **conserva la previa** (un cliente que no la
  envía no la borra por accidente). Al cambiarla se reinicia `ProximaEjecucion = null`.

**Verificación.** Gate oficial verde a la primera: build Release **0w/0e**, Domain 40 / **Detectors 177**
(+12 pruebas de `ProgramacionDto`: ida-y-vuelta de intervalo y de "mensual día 29", y rechazo de modo/unidad/
hora/minuto/día-mes fuera de rango, incl. mayúsculas y "último día") / Monitor 2 / **Api 77** (integración
intacta), **EF sin cambios pendientes** (Etapa 4 no toca el esquema), eslint + vite OK. Falta la **Etapa 5** (UI).

---

## 79. Frecuencia/calendario por regla — Etapa 5: el selector en la interfaz (feature completo)

**Motivación.** Cerrar el feature con la capa visible: que el usuario configure la cadencia de cada regla
desde la interfaz, con un selector claro en español (segundos…meses + calendario) y viendo la "próxima
ejecución".

**Qué se hizo.**

- **`<ProgramacionSelector>`** (`components/reglas/ProgramacionSelector.tsx`): componente **reutilizable**
  y controlado con tres modos — *En cada ciclo*, *Cada cierto tiempo* (N + unidad seg/min/h/días/sem/meses)
  y *En un calendario* (diario; semanal con día de la semana; mensual con día-D o **"último día del mes"**;
  hora:minuto en zona estación UTC-5). Muestra una **vista previa en vivo** ("▶ El día 29 de cada mes a las
  00:00"). Las constantes y los helpers (`PROGRAMACION_CADA_CICLO`, `describirProgramacion`, `formatProxima`)
  viven en `lib/programacion.ts` (módulo aparte, para no romper Fast Refresh: un archivo de componente solo
  exporta componentes).
- **Reglas del motor** (`ReglasPage.tsx`): cada fila tiene un **chip de cadencia** (icono + descripción) que
  abre un panel inline con el selector + "Guardar cadencia"/"Cancelar" y la **próxima ejecución**. Al guardar
  hace `PUT { programacion }`.
- **Reglas personalizadas** (`ReglasPersonalizadasSection.tsx`): el formulario gana la sección "¿Cada cuánto
  se ejecuta esta regla?" con el mismo selector; la lista muestra la cadencia + próxima ejecución bajo cada
  regla programada. El toggle activar/desactivar y el cambio de fuente **conservan** la programación (no la
  borran).
- **Tipos TS** (`types/regla.ts`, `types/reglaPersonalizada.ts`): `ProgramacionDto` + enums espejo del
  backend; `programacion`/`proximaEjecucion`/`ultimaEjecucion` en las respuestas y el request.

**Verificación.** Gate oficial verde: build Release **0w/0e**, Domain 40 / Detectors 177 / Monitor 2 /
Api 77, **eslint limpio** (se corrigió `react-refresh/only-export-components` separando constantes/helpers a
`lib/programacion.ts`) y **tsc + vite OK**. Con esto el feature **frecuencia/calendario por regla queda
completo** (Etapas 1-5): modelo+Cronos → columnas+migración → job dos-pasadas+ventana → API+validación → UI.

---

## 80. Pulido UX (feedback de Steven): calendario intuitivo + filtro por tabla en "campos a mostrar"

**Motivación.** Dos observaciones de uso sobre la ronda anterior: (1) el selector de calendario con cajas de
número se veía pobre y poco intuitivo; (2) al elegir "información a mostrar en la alerta", el autoenlace de
tablas expone varios campos del mismo concepto (p. ej. "Código de cliente" propio y "Código de cliente ·
Despacho…"), lo que confunde sin saber de qué tabla viene cada uno.

**Qué se hizo.**

- **Selector de calendario tipo calendario** (`ProgramacionSelector.tsx`): el día del mes ahora es una
  **grilla 1–31** (clic para elegir, como un calendario) con botón **"Último día del mes"**; el día de la
  semana son **pastillas** (dom–sáb); y la hora usa el **selector nativo** `<input type="time">` (reloj del
  navegador) en vez de dos cajas de número. Mucho más claro e intuitivo; la vista previa en vivo se mantiene.
- **Filtro por tabla** en "Información a mostrar en la alerta" (`ReglasPersonalizadasSection.tsx`): además del
  buscador y el filtro por tipo, un desplegable **"📂 Todas las tablas"** permite filtrar los campos por su
  tabla de origen (la fuente propia o la tabla relacionada — el prefijo "Fuente." del campo). Así, si se
  agregan dos "Código de cliente", se sabe de qué tabla viene cada uno. Guarda anti-stale si la tabla elegida
  ya no existe en la fuente actual.
- **Auditoría del autoenlazador** (`DescubridorRelacionesService`): revisado a fondo — es correcto (data
  profiling: similitud de nombre de columna + solapamiento de valores reales en staging, descarta falsos
  positivos). Los múltiples "código" son esperados (cada tabla relacionada tiene el suyo); **no era un bug**,
  solo faltaba la UX para distinguirlos. Sin cambios de backend.

**Verificación.** Gate del frontend verde: **eslint limpio** + **tsc + vite OK** (.NET sin tocar, quedó verde
en la ronda previa). *Pendiente: QA en vivo en Chrome (Steven dejó una regla de prueba "Despacho Excesivo"
cada 30 s).*

---

## 81. Fix: "Despacho NO facturado" disparaba en TODOS los despachos (lectura errónea de FAC_DESP)

**Síntoma.** En San Pío llegaban sin parar alertas operativas "Despacho N NO facturado: X gal por $Y. Revisar
(combustible servido sin cobrar)" — en prácticamente cada despacho.

**Causa raíz.** `DespachoNoFacturadoRule` asumía que el campo `FAC_DESP` (DESP) es un 0/1 y marcaba como
anomalía **todo lo que no fuera "1"**. Consulta a los datos reales (`transacciones_staging`): `FAC_DESP`
toma valores **2, 4, 5, 7** (y a veces vacío), **nunca "1"**. Es un **código de estado de facturación**
(distintos canales de liquidación), no un booleano: cualquier valor poblado = despacho **ya facturado**. La
regla, al exigir "1", disparaba en todos; y, peor, **saltaba** el caso realmente sospechoso (marca vacía).

**Qué se hizo.** Se invirtió la lógica: la regla marca "no facturado" **solo cuando `FAC_DESP` viene vacío o
"0"** (despacho sin liquidar = combustible servido sin cobrar); cualquier código poblado (2/4/5/7…) se
considera facturado y no genera alerta. Se corrigió el comentario del DTO `DetalleFacturaDto.Facturado` (que
afirmaba erróneamente "'1' facturado"), y la evidencia muestra "(vacío)" cuando la marca está en blanco.
*Diagnóstico apoyado en el schema (`docs/contac-schema.sql`, `DESP.FAC_DESP Char(1)`) y en la distribución
real de valores.*

**De paso (duda de Steven):** en el log "Datos recibidos" se ven dos tipos que parecen "facturas" pero son
**dos tablas distintas y complementarias**: **`DCTO`** = la **cabecera de la factura** (documento de venta:
IVA, subtotal, total, RUC, vendedor, forma de pago, nº de documento…) y **`DESP`** = el **detalle del
despacho** (cada surtido de combustible: galones, producto, manguera, valor unitario, nº de despacho). Una
factura `DCTO` agrupa uno o varios despachos `DESP`. Por eso aparecen ambas para San Pío.

**Verificación.** Gate verde: build Release 0w/0e, Domain 40 / **Detectors 182** (+5 pruebas de regresión:
`FAC_DESP` = 2/4/5/7 → **no** alerta; vacío → **sí** alerta; las previas de "0"→alerta y "1"→no-alerta siguen
en verde) / Monitor 2 / Api 77, EF sin cambios. **Para que surta efecto hay que reiniciar el API**
(`ejecutables/1-INICIAR-Y-DETENER/reiniciar-solo-la-api.bat`); las alertas falsas ya generadas quedan como
histórico (pueden marcarse como falso positivo).

---

## 82. Análisis ContaPlus + entrevista de auditoría → corrección `FAC_DESP` y backlog

**Motivación.** Steven aportó tres insumos: la **entrevista con la auditora** (qué quieren añadir), la
**documentación técnica de ContaPlus** (PDF + `.txt`) y **capturas reales del catálogo Forma de Pago** del
POS. Pidió analizarlo todo y volcarlo al diccionario de datos. **Aviso clave de Steven:** el PDF y el `.txt`
son **algo viejos** (de hecho el PDF usa nombres idealizados —`COD_DCTO`,`TOT_DCTO`— que NO existen en el
schema real —`SEC_DCTO`,`TNI_DCTO`—); las **imágenes y la entrevista** son recientes y confiables.

**Qué se hizo.**

- **`docs/ANALISIS-CONTAPLUS-Y-ENTREVISTA-AUDITORIA.md`** (nuevo): síntesis completa con (1) tabla de
  confiabilidad de fuentes, (2) el hallazgo crítico de `FAC_DESP`, (3) diccionario de negocio verificado
  contra el schema real, (4) catálogo real de `COD_PAGO` (de las capturas) y (5) el **backlog priorizado**
  de auditoría.
- **Hallazgo crítico:** **`FAC_DESP` NO es "facturado" — es la FORMA DE PAGO del despacho.** Lo confirman
  los **datos reales** ({2,4,5,7,…}, imposibles para un 0/1), el `.txt` técnico (mapa 0–9 →
  contado/crédito/tarjeta/cheque), el PDF (§3.4) y la entrevista (la auditora no sabía qué era "5"). Esto
  significa que la regla "Despacho no facturado" y el fix §81 están sobre **premisa equivocada** ("0"=cheque,
  no "sin cobrar"); saber si un despacho se facturó requiere cruzar `DESP.NUM_DESP ↔ DCTO.NDO_DCTO`
  (documentado, **a verificar contra datos**). Anotado en el doc para rehacer/desactivar la regla.
- **Diccionario corregido (seguro):** `DiccionarioCamposContaplus` — `FAC_DESP` pasa de "¿Facturado?" a
  **"Forma de pago del despacho"** (rol Código), y `COD_PAGO` apunta al catálogo real
  (001/002/003/004/CRE/EFE/020…). Corregido también el comentario de `DetalleFacturaDto.Facturado`.
- **Backlog de auditoría (en el doc):** 🔴 misma placa/cliente facturada N+ veces al día (caso real: 14×/día,
  riesgo SRI), factura no incluida en liquidación; 🟠 hipervínculos en alertas + ventana nueva, buscador por
  placa/RUC/nombre, nº de factura visible, dashboard filtrable por estación; 🟡 botón copiar, donaciones
  excesivas por vendedor. Confirmaciones: descuento nunca se aplica (umbral 0), turno sin cerrar >24 h.

**Verificación.** Cambios no funcionales (literales de texto + comentarios + un `.md`): build Release verde,
suites sin cambios. *Pendiente de confirmar con Steven qué del backlog abordar primero.*

Commit: `f7cf7cb`.

---

## 83. Regla nueva (auditoría #1): "Placa reutilizada en el día"

**Motivación.** Primer ítem 🔴 del backlog de auditoría (§82). La auditora describió un caso real: **14
facturas en un mismo día a la misma placa**. Es el patrón de *reutilización de placa* — el despachador
carga ventas a una placa "comodín" de un cliente frecuente para cuadrar efectivo o emitir facturas
ficticias, lo que expone a la estación a una **denuncia ante el SRI**. La regla existente "Despachos
rápidos sucesivos" usa una ventana de **minutos**; faltaba la mirada **por jornada**.

**Qué se hizo.**

- **`PlacaReutilizadaRule`** (`src/PetrolRios.Detectors/Rules/InvoiceAnomaly/`, carril Auditoría). Agrupa
  las facturas por **(placa normalizada, día)** y alerta cuando una placa supera el umbral en el día.
  **Excluye** la placa genérica `ZZZ999949` (consumidor final, que aparece legítimamente cientos de veces;
  su exceso lo controla `PlacaGenericaRule`) y las placas vacías. Mete en la evidencia: placa, día, conteo,
  umbral, monto total, **números de factura**, clientes y vendedores involucrados (más riesgo si varios
  despachadores cargaron la misma placa el mismo día). Se auto-registra por reflexión (no toca la DI).
- **Programación diaria.** Como el conteo es *por jornada*, la regla **no corre "cada ciclo"** (vería solo
  el lote incremental de minutos): se siembra con **Calendario Diario 23:55** (hora de estación), de modo
  que el Pass B del job la evalúe una vez al día sobre la **ventana del día** (`DiasVentanaSugerida=1`),
  con ventanas no solapadas → sin alertas duplicadas. Encaja con el "ejecutar la regla tras el cierre" que
  pidió la auditora y con el motor de programación por regla (§79/Etapas 1–5).
- **Umbral configurable** (`PlacaReutilizadaDiaUmbral`, **default 5**). Se sembró conservador para no
  inundar en el primer despliegue; la **auditora sugiere bajarlo hasta 2** y es editable en un clic desde
  Reglas. Sembrado idempotente en `SeedData.EnsureReglasNuevasAsync` (también aplica a bases existentes).
- **+7 pruebas** (`PlacaReutilizadaRuleTests`): sobre umbral, en umbral (no dispara), placa genérica
  excluida, placas distintas, repartida en dos días (no dispara), umbral configurable a 2, regla inactiva.

**Verificación.** `_gate1.bat` lanzado desde el Explorador (en la PC de Steven): **build Release 0
warnings/0 errors**; **tests verdes** — Domain 40, **Detectors 189 (+7)**, Api 59 (+18 skip de
integración), Monitor 2. Sin cambio de esquema EF (solo clase de regla + seed de datos).

Commit: `99d3330`.

---

## 84. UX de alertas (auditoría #2): buscador, hipervínculos, copiar, n° de factura

**Motivación.** Backlog de auditoría 🟠 (§82). La auditora pidió que la bandeja funcione "como un ERP":
poder **buscar** una alerta por placa/RUC/n° de factura/cliente, ver el **número de factura** sin tener
que leer la referencia técnica larga, **copiar** valores de un clic, y **saltar** desde una alerta a todas
las demás de la misma placa/cliente (y abrirlas en otra pestaña para comparar).

**Qué se hizo.**

- **Buscador (backend).** `AlertaRepository.ApplyFilters` acepta `buscar`: coincidencia parcial e
  insensible a mayúsculas (`ToLower().Contains` → `LIKE` en PostgreSQL) sobre **Descripción**, **Referencia**,
  **MetadataJson** (la evidencia: placa, RUC, cliente, n° de factura) y **EmpleadoCodigo**. Hilo completo
  `AlertasController` (`?buscar=`) → `IAlertaService`/`AlertaService` → `IAlertaRepository`/`AlertaRepository`
  (parámetro opcional, no rompe llamadas existentes).
- **Buscador (frontend).** Caja de búsqueda en `AlertasPage` con **rebote de 350 ms** (no consulta por
  cada tecla), botón de limpiar, y soporte de `?buscar=…` en la URL para los hipervínculos. Integrada con
  los filtros existentes (estación, tipo, nivel, estado, fechas) y el botón "Limpiar filtros".
- **Hipervínculos + n° de factura + copiar (detalle).** En "Evidencia de la detección", los valores
  **buscables** (placa, n° de factura, cliente, RUC) se muestran como **pastillas con enlace** que abren la
  bandeja filtrada por ese valor (`/alertas?buscar=…` → "ver todas las alertas de esta placa") **y botón
  copiar** (✓ de confirmación). Los valores de lista (varios n° de factura, clientes, despachadores de la
  regla de placa) se muestran como pastillas individuales. La **Referencia** ahora tiene botón copiar, y la
  cabecera un enlace **"Abrir en ventana nueva"** (comparar alertas lado a lado). Etiquetas nuevas para las
  claves de la regla de placa (Día, Cantidad de facturas, Números de factura, Clientes, Despachadores).
- **Nota:** el "ver todas las **facturas** del cliente" (reporte sobre los datos de origen) se hará junto a
  la mejora #4 (reportería); aquí el salto es a las **alertas** de esa placa/cliente, que es inmediato y no
  depende de un endpoint de reportes nuevo.

**Verificación.** `_gate2.bat` desde el Explorador (PC de Steven): **build Release 0/0**, **tests** Domain
40 / Detectors 189 / Api 59 / Monitor 2, **eslint OK**, **`tsc -b && vite build` OK**. *Pendiente: QA en
vivo en Chrome.*

Commit: `e5d7bdb`.

---

## 85. Dashboard por estación (auditoría #4)

**Motivación.** Backlog de auditoría 🟠 (§82): *"el dashboard mezcla todas las estaciones; las alertas ya
se filtran, el dashboard no"*. La auditora quiere ver el panel **acotado a una estación** y poder
**imprimir/exportar** desde la misma pantalla.

**Qué se hizo.**

- **Backend.** `DashboardService.AlertasAuditoria` pasó de propiedad a **método `AlertasAuditoria(int?
  estacionId)`** que, si llega `estacionId`, agrega `.Where(a => a.EstacionId == …)`. Se enhebró por
  `IDashboardService`/`DashboardController` (parámetro `?estacionId=`, opcional) en **KPIs, tendencia,
  distribución por tipo, por nivel, métricas de resolución y ranking de empleados**. La comparativa
  **"alertas por estación" queda global** (es la vista cruzada y alimenta el selector). Las tarjetas
  "estaciones en línea / totales" siguen siendo de flota (no se filtran). Parámetro opcional → no rompe
  llamadas existentes.
- **Frontend.** `DashboardPage` tiene un **selector de estación** (Todas / cada estación) que acota todos
  los paneles salvo la comparativa; `estacionId` entra en las `queryKeys` y se usa `keepPreviousData` para
  cambiar de estación **sin parpadeo a esqueleto**. El subtítulo refleja la estación elegida. Botón
  **"Imprimir / PDF"** (`window.print()`, con `print:hidden` en los controles para una impresión limpia)
  → reportería en la misma pantalla. El service del frontend pasa `estacionId` a los 6 endpoints.
- **Nota:** el Excel completo sigue en la pestaña **Reportes** (no se duplicó); aquí se cubre el "imprimir
  /PDF sin salir del dashboard".

**Verificación.** `_gate3.bat` desde el Explorador (PC de Steven): **build Release 0/0** (tras corregir un
uso de `AlertasAuditoria` como propiedad → método), **tests** Domain 40 / Detectors 189 / Api 59 / Monitor
2, **eslint OK**, **`tsc -b && vite build` OK**. *Pendiente: QA en vivo en Chrome.*

Commit: `594f186`.

---

## 86. QA en vivo en Chrome (#1/#2/#4) + fix del buscador (jsonb) + lectura total de `ejecutables`

**Lectura de `ejecutables` (a pedido de Steven).** Se leyeron **las 6 subcarpetas completas** (no solo la
4): arranque/parada (1), BD/demo+Firebird en Docker (2), diagnóstico (3), verificación/gate (4),
publicación/despliegue (5) e instalación en PC nuevo (6). De ahí se usó: `reiniciar-solo-la-api.bat` +
`reiniciar-solo-el-frontend.bat` (Vite dev :5173) para el QA sin reconstruir `dist`, el patrón
`docker exec petrolrios-firebird isql` (carpeta 2) para la verificación de #3, y queda mapeado
`publicar-solo-el-agente-multiplataforma.bat` para el republish de #3.

**QA en vivo (con API+Vite reiniciados en la PC de Steven):**
- **#1 ✔** — En **Reglas**, "Placa reutilizada en el dia" aparece con carril **AUDITORÍA**, cadencia
  **"Todos los días a las 23:55"** (única programada; el resto "cada ciclo") y **umbral 5**. El reinicio
  de la API sembró la regla (`EnsureReglasNuevasAsync`).
- **#2 ✔** — El **buscador** filtra de verdad: "CZ0060636" → 1 alerta (#60). En el **detalle**, los chips
  buscables (Cliente), las listas (nº de factura, despachadores) como pastillas, el botón **copiar** y
  **"Abrir en ventana nueva"** renderizan bien.
- **#4 ✔** — El **dashboard** muestra el selector de estación (Todas / EST-001 / …) y el botón **Imprimir/PDF**.

**🐞 Fix encontrado por el QA en vivo (lo que el gate no veía).** El buscador devolvía **HTTP 500**:
`Alerta.MetadataJson` es columna **`jsonb`**, y `lower()` sobre `jsonb` **no se traduce** en
Npgsql/EF. Los tests de integración del Api estaban *skipped* (sin BD), por eso el gate pasó. **Fix:** el
`buscar` solo recorre columnas de **texto** (`Descripcion`, `TransaccionReferencia`, `EmpleadoCodigo`); la
placa, el cliente y el nº de factura siguen cubiertos porque aparecen en la **descripción** de cada regla.
Reverificado en vivo: la consulta ahora responde **200** y los 0-resultados muestran el estado vacío
limpio (antes se quedaba en "Cargando…"). *(Esta vez los tests del Api corrieron con BD: **77 passed, 0
skipped**.)* **Limitación conocida:** la búsqueda por **RUC** y por **nombre de despachador** no está
cubierta (el RUC vive solo en la evidencia `jsonb` y el nombre se resuelve aparte del código); queda
anotada como mejora futura (cruzar el catálogo de empleados y/o exponer el RUC en columna de texto).

Commits: `e5d7bdb` (UX), **`c011374`** (fix del 500). Gate del fix: build Release 0/0, **tests** Domain 40
/ Detectors 189 / Api **77 (0 skip, con BD)** / Monitor 2.

---

## 87. Buscador de alertas COMPLETO: por evidencia (RUC) y por nombre de despachador

**Motivación.** Cerrar la limitación de §86: el buscador no cubría **RUC** (vive en la evidencia) ni
**nombre de despachador** (se resuelve aparte del código). La auditora pidió explícitamente "placa/RUC/nombre".

**Qué se hizo.**

- **Evidencia buscable (`MetadataJson` jsonb → `text`).** Como esa columna se usa siempre como cadena JSON
  completa (nada la consulta con operadores jsonb), se cambió su tipo a `text` (migración EF
  **`BuscarEvidenciaAlertaTexto`**, `AlterColumn` jsonb→text, lossless). Así `lower(text)` SÍ traduce y el
  `buscar` vuelve a recorrer la evidencia → **placa, RUC, cliente, nº de factura** de la metadata son
  buscables. Se añadió `RucCliente` (como **`Rucs`**) a la evidencia de la regla de placa para que el RUC
  esté presente y se muestre.
- **Búsqueda por NOMBRE (servicio).** `AlertaService.GetFilteredAsync` resuelve contra el catálogo
  `Empleados` los **códigos cuyo nombre coincide** con el término y el repositorio incluye también las
  alertas de esos códigos (parámetro `codigosPorNombre`, OR `EmpleadoCodigo IN (...)`). Así "MENDOZA"
  encuentra las alertas de ese despachador aunque la tabla de alertas solo guarde el código.
- **Frontend:** etiqueta "RUC / cédulas" y `Rucs` como clave buscable (pastilla-enlace) en el detalle.

**Verificación (gate + QA en vivo en Chrome, con migración aplicada al reiniciar la API):**
- `_mig.bat`: **build Release 0/0**, **tests** Domain 40 / Detectors 189 / **Api 77 (0 skip, con BD)** /
  Monitor 2, **eslint OK**, **`tsc -b && vite build` OK**, migración generada.
- **En vivo:** "MENDOZA" → **8 alertas, todas de JORGE MENDOZA**; "032101000020765" (nº de factura que solo
  está en la evidencia de #60, no en su descripción) → **1 alerta, la #60**; "CZ0060636" → #60. El buscador
  cubre ahora **placa, RUC, nº factura, cliente, código y nombre**.

Commit: `69ac236`.

## 88. Factura fuera de liquidación / cuadre de turno (auditoría #3)

**Motivación.** Tercer ítem 🔴 del backlog de auditoría (§82). Al **cerrar un turno**, todas sus facturas
deben quedar en la **liquidación** (`LIQU`). Una factura "colgada" —su turno cerró pero **no aparece en
`LIQU`**— quedó **fuera del cuadre de caja**: facturación sin liquidar, indicio de descuadre o de retención
de efectivo. Enlace verificado contra la base real (§ commit `cd4008f`): `DCTO.NUM_TURN ↔ LIQU.NUM_TURN`
(149/149 liquidaciones con turno; las 25 FV sin liquidar eran del turno de prueba).

**Qué se hizo.**

- **Agente lee `LIQU`** (`FirebirdExtractor`): nuevo `LiquidacionDto` (`NUM_LIQU`, `NUM_TURN`, `FEC_LIQU`,
  `DIF_LIQU`) extraído **solo lectura** (`SELECT … WHERE FEC_LIQU > @Watermark`) y enviado como tipo
  **"Liquidacion"**. `LIQU` se registra como tabla built-in (no duplicable en el selector).
- **`CuadreLiquidacionService`** (Infrastructure). Lógica **pura y testeable**
  `CalcularTurnosSinLiquidar(cierres, liquidaciones, facturas, umbralHoras, ahora)`: devuelve los turnos
  **CERRADOS** (`EST_TURN='1'`) con cierre **anterior a `ahora − umbralHoras`** (periodo de gracia, da
  tiempo a que llegue la liquidación) que tienen facturas **FV** pero **no** están en ninguna `LIQU`.
  `EvaluarEstacionAsync` consulta el **staging acumulado** (30 días), deduplica los lotes reenviados, y
  crea **una alerta idempotente por turno** (referencia única `SINLIQU-{estacion}-{turno}` → no re-alerta).
- **Por qué NO es un detector de ventana.** La liquidación llega **después** del cierre (otro lote), así
  que un detector que solo ve la ventana del ciclo daría falsos positivos. Se consulta el staging directo.
  Es **correcto** porque `FEC_LIQU ≥ FFI_TURN` siempre (se liquida tras cerrar): si el cierre se extrajo
  (`FFI_TURN > watermark`), su liquidación —si existe— también → cierre y liquidación viajan juntos, nunca
  un cierre "huérfano" con su `LIQU` perdida fuera de la ventana.
- **Integración en el job** (`AnomalyDetectionJob`): el cuadre se gestiona **aparte** del gate de
  detectores; corre una vez al día sobre el staging, y **avanza su propia** `ProximaEjecucion`/
  `UltimaEjecucion`. `"Liquidacion"` añadido a `tiposConocidos`.
- **Regla sembrada** `FacturaSinLiquidacionHorasUmbral` (umbral **12 h** de gracia, ámbito **Auditoría**,
  activa, **Calendario Diario 23:50**, igual patrón que la regla de placa §83).
- **Frontend**: etiqueta **"Fecha de cierre del turno"** en la evidencia del detalle de alerta.
- **+9 pruebas puras** (`CuadreLiquidacionServiceTests`): turno cerrado sin liquidar (reporta), liquidado
  (no), abierto (no), dentro de gracia (no), sin facturas (no), solo documentos no-FV (no), solo FV cuenta
  en monto/conteo, varios turnos (solo incumplidores), tolerancia de espacios/mayúsculas. Fix del
  constructor en `AnomalyDetectionJobE2ETests` (nuevo parámetro `CuadreLiquidacionService`).

**Verificación.** Gate oficial (`_gate.bat`) en la PC de Steven: **build Release 0 warnings/0 errores**;
**pruebas 317 verdes / 0 skipped** (Domain 40, Monitor 2, **Detectors 189**, **Api 86** — incluye los 9
del cuadre y el E2E corregido, con BD); **EF sin migraciones pendientes** (no hay cambio de esquema:
usa staging + alertas para idempotencia); **lint + build de frontend OK**.

**QA E2E en vivo** (`iniciar-todo`, agente local **EST-001** + Firebird en Docker): se cerró el turno de
prueba **990001** en la Firebird de demo (`EST_TURN='1'`, cierre 26/06 12:00, 13 facturas FV, sin `LIQU`);
el agente lo re-extrajo (rebobinando la marca de agua) y el job ejecutó el cuadre → **alerta #80**
*"Turno 990001 cerrado el 26/06/2026 12:00 SIN liquidación: 13 factura(s) por $1674,20 quedaron fuera del
cuadre"*, nivel **Alto (score 71.7)**, ref `SINLIQU-1-990001`, evidencia con turno, **fecha de cierre**,
13 nº de factura y 7 RUC, empleado **JORGE MENDOZA (004)** (nombre resuelto). La regla **avanzó su agenda**
(`UltimaEjecucion` ahora, `ProximaEjecucion` al próximo 23:50) e **idempotente** (sin duplicado en los
ciclos siguientes). *(Endurecimiento futuro opcional: persistir las liquidaciones en una tabla del central
para retenciones de staging menores a la antigüedad de los turnos; hoy no hace falta por `FEC_LIQU ≥ FFI_TURN`.)*

Commit: `43fe272` (código + pruebas). Documentación en este commit.

---

## 89. Tasa de refresco configurable (todas las pantallas) + agente cada 1 s

**Motivación.** Feedback de Steven sobre la entrevista: la auditora quiere el sistema **más rápido** y
"casi en vivo". Antes cada pantalla refrescaba con un intervalo distinto hardcodeado (10/15/30/60 s) y el
agente enviaba cada **30 s**.

**Qué se hizo.**

- **Central — tasa de refresco GLOBAL y configurable.** `OperacionConfig` gana `RefrescoSegundos`
  (por defecto **1 s**, acotado **1 s … 1 h** en `ParametrosOperacionStore`). Nuevo endpoint liviano
  **`GET /api/v1/refresco`** accesible a **cualquier rol** autenticado (no solo Admin); el `PUT` de
  operación (solo Admin) lo persiste. En **Ajustes → Operación** hay un selector "Tasa de refresco de las
  pantallas".
- **Frontend — un solo ajuste cambia toda la interfaz.** `RefrescoContext` (hooks `useRefrescoMs` /
  `useRefrescoSegundos`) lee `/refresco` (y se re-lee cada 15 s para propagar cambios a sesiones abiertas).
  **TODAS** las pantallas lo usan como `refetchInterval`: Alertas, Dashboard, Conexiones, Problemas de
  estación, Logs, Datos recibidos y Fuentes de datos. Conexiones muestra la tasa real ("cada N s"). La
  consulta de versión del agente (servidor externo) se deja en 60 s a propósito.
- **Agente — casi tiempo real.** Intervalo por defecto **30 s → 1 s**; el clamp del `Worker` baja de
  **mín. 5 s → 1 s** (máx. 1 h); el panel del agente acepta `min=1`.
- **+8 pruebas** (`ParametrosOperacionStoreTests`): acota 0/negativos al valor por defecto y >máx a 3600 s;
  conserva nivel+cron junto al refresco.

**Verificación.** Gate oficial en la PC de Steven: **build Release 0/0**, **pruebas 325 / 0 skipped**
(Domain 40, Monitor 2, Detectors 189, **Api 94 (+8)**), **EF sin migraciones pendientes** (config en
archivo, sin esquema), **lint + build de frontend OK**.

Commit: `626c3e0`.

---

## 90. Evidencia identificable automática en las alertas (RUC, n° doc, placa, cliente, turno…)

**Motivación.** La auditora, viendo una alerta de "diferencia de efectivo" (caso real), notó que **falta
información** para identificar de un vistazo de qué se trata: quiere RUC, **número de documento** (el corto),
placa, cliente, etc. en TODAS las alertas — y que sea **automático y escalable** (también para reglas nuevas).

**Qué se hizo.**

- **`DetectedAnomaly.Fuente`** (objeto de origen, p. ej. una `FacturaDto`/`CierreTurnoDto`). El orquestador
  **`RuleBasedDetector`** enriquece la evidencia de **toda** anomalía reflejando desde la `Fuente` los campos
  **identificables estándar** — **RUC, número de documento, placa, cliente, turno, fecha, monto, forma de
  pago** — mediante **`EvidenciaEnriquecida`** (mapea por nombre de propiedad del DTO). **No pisa** las claves
  que la regla ya puso ni copia vacíos/cero. Es la pieza que vuelve la evidencia **automática y escalable**:
  una regla nueva solo fija `Fuente` y hereda todos esos campos.
- **`Fuente` fijada en 10 reglas** (las de mayor valor identificable): venta sin placa, venta sin
  identificación, alto volumen sin placa, placa genérica, descuento excesivo, total inconsistente, campos
  obligatorios, precio fuera de lista, crédito sin cliente y **diferencia de efectivo** (la del caso de la
  auditora). El resto de reglas puede sumarse en un renglón (solo `Fuente = …`).
- **Frontend**: el detalle de alerta etiqueta las claves nuevas (`Ruc` → "RUC / cédula", `Fecha`,
  `FormaPago` → "Forma de pago") y ya las hace **buscables/enlazadas** (RUC, n° de documento, placa, cliente
  llevan a la bandeja filtrada).
- **+4 pruebas** (`EvidenciaEnriquecidaTests`): refleja los campos, **no pisa** lo de la regla, **omite**
  vacíos/cero, y no hace nada con fuente nula.

**Verificación.** Gate oficial en la PC de Steven: **build Release 0/0**, **pruebas 329 / 0 skipped**
(Domain 40, Monitor 2, **Detectors 193 (+4)**, Api 94), **EF sin migraciones pendientes**, **lint + build de
frontend OK**.

Commit: `1114d65`. *(Pendiente: extender `Fuente` al resto de reglas built-in y al detector de reglas
personalizadas; QA en vivo junto con el resto de la ronda.)*

---

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
