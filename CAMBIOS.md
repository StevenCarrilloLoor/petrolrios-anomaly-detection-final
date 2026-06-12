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
