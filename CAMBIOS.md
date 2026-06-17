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

## 22. Auto-actualización remota del agente (control de versiones)

- **Versión única** en `Directory.Build.props` (fluye al `.exe`, al heartbeat, al manifiesto
  y a los instaladores). Se eliminó el `"2.1"` quemado: el agente reporta su versión real.
- **Endpoint `GET /api/v1/agente/version`** (anónimo) sirve un **manifiesto editable** (versión,
  URL, sha256, notas) desde `config/agente-version.json` o `appsettings`.
- **Agente:** `UpdateService` consulta el feed (URL configurable + respaldo, p. ej. GitHub),
  compara semver y aplica **con un clic**: descarga, **verifica el checksum**, intercambia el
  `.exe` y reinicia el servicio/proceso. El panel muestra el aviso "actualización disponible".
- **Panel central (Conexiones):** badge "actualización vX disponible" por estación.

## 23. Seguridad — fase 1 (accesos del servidor + correo de alertas)

- **Dashboard de Hangfire cerrado** (`HangfireLocalAuthorizationFilter`): solo local o
  Administrador autenticado (antes estaba abierto a cualquiera).
- **JWT con _fail-fast_ en producción**: el servidor no arranca si la clave es débil
  (< 32) o la de desarrollo. **HTTPS/HSTS** en producción (configurable).
- **Notificador SMTP de alertas críticas** (`IEmailNotificacionService`): configurable y
  **apagado por defecto**; sin credenciales quemadas; nunca tumba el ciclo de detección.

## 24. Seguridad — fase 2 (panel del agente con autenticación y RBAC)

- El panel del agente **exige iniciar sesión**; configurar/sincronizar/actualizar quedan
  bloqueados sin sesión válida (cierra el hueco de que cualquiera reconfigure el agente).
- **RBAC real:** las credenciales se verifican contra el servidor central y solo
  **Administrador/Supervisor** pueden administrar el agente (el rango de la empresa importa).
- **Respaldo local offline** (contraseña local cifrada con PBKDF2) para entrar cuando el
  central no está disponible; arranque inicial seguro (bootstrap solo antes de configurar).

## 25. Seguridad — fase 3 (sin contraseñas quemadas + bloqueo + cambio obligatorio)

- La **contraseña inicial del admin** sale de configuración/variable de entorno; ya no está
  quemada, y se obliga a **cambiarla** en el primer ingreso.
- **Bloqueo de cuenta por intentos fallidos** (anti fuerza bruta), configurable.
- Endpoint `POST /api/v1/auth/cambiar-password` y página de **Seguridad (mi cuenta)** en el
  frontend; campos de seguridad en `Usuario` + migración `SeguridadUsuario`.

## 26. Bugfix — el agente ya no enmascara fallos de Firebird

- Descubierto en pruebas en vivo: cada consulta abría su conexión y **tragaba** la excepción
  devolviendo vacío, por lo que un Firebird caído se veía como "OK Sin transacciones nuevas"
  (punto ciego de monitoreo). Ahora se abre **una** conexión y, si falla (WireCrypt,
  credenciales, archivo, servicio caído), la excepción **se propaga** y el ciclo la reporta
  como ERROR. Los fallos por tabla individual se siguen tolerando.
- **Verificado end-to-end:** Firebird OK (40.033 documentos), 14 anomalías inyectadas →
  el agente las extrajo → el motor generó 12 alertas nuevas.

## 27. Autenticador 2FA (TOTP) en el programa principal

- **`TotpService`** propio (RFC 6238, HMAC-SHA1, 6 dígitos, ventana 30 s), compatible con
  Google Authenticator / Authy / Microsoft Authenticator, sin dependencias externas.
- **Login con segundo factor**: si el usuario tiene 2FA, pide el código de 6 dígitos.
- **Pantalla "Seguridad (mi cuenta)"** con enrolamiento por **QR** (librería `qrcode`) y
  desactivación; endpoints `/auth/2fa/iniciar|confirmar|desactivar|estado`.

## 28. Login por QR estilo Steam

- La pantalla de login muestra un **QR**; un usuario ya autenticado lo **aprueba** desde otro
  dispositivo (`/aprobar-qr`) y la pantalla entra sola, sin teclear contraseña.
- `QrLoginService` (estado en memoria, código de un solo uso, expiración 2 min) + endpoints
  `/auth/qr/iniciar|estado|aprobar`. Respeta la verificación de correo.
- *Nota:* en `localhost` el QR no se escanea desde el celular (localhost = el propio teléfono);
  funciona con el servidor en una IP/dominio real.

## 29. Verificación de correo real y obligatoria

- Al crear un usuario se **envía un correo** con botón "Verificar correo electrónico" (enlace a
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

## 32. Resultado de verificación (rondas 5–6)

Build sin warnings; **126 pruebas unitarias en verde** (Domain 8, Detectors 89 — incluidos los
nuevos de expresiones, TOTP y QR —, Api 29 con las de integración). Frontend compila y empaqueta
limpio. Verificado en vivo: dashboard, conexiones (API v2.2.0, SignalR activo), detección
end-to-end con Firebird real, panel del agente con login RBAC, 2FA con QR, **verificación de
correo real contra Gmail**, y recuperación/login por autenticador. Migraciones EF nuevas:
`SeguridadUsuario` y `VerificacionEmail`.
