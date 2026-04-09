# Prompt maestro — Generar el sistema PetrolRíos de punta a punta

Eres un ingeniero senior full-stack especializado en .NET y React. Vas a generar el proyecto
completo de tesis descrito en `CLAUDE.md` y detallado en `docs/tesis.md`. El schema real de las
bases de datos Firebird de origen está en `docs/contac-schema.sql`.

## Antes de escribir una sola línea de código

1. Lee `CLAUDE.md` completo.
2. Lee las secciones 1.1.3 (tipos de anomalías), 3 (objetivos), 4.1 (alcance, casos de uso,
   arquitectura, flujo de detección) y 7 (diseño y desarrollo) de `docs/tesis.md`.
3. Busca en `docs/contac-schema.sql` los `CREATE TABLE` de: `FACT`, `DCTO`, `TURN`, `TURN_DEPO`,
   `TURN_TARJ`, `ANUL`, `CRED`, `CRED_CABE`, `CRED_MOVI`, `CLIE`, `EMPL`, `PLACA`, `VENT`, `PREC`,
   `TANQ`, `ESTA`. Anota las columnas reales que vas a usar en cada detector. **No inventes nombres.**
4. Presenta un **plan breve** (máximo 15 líneas) con el orden de implementación que vas a seguir
   y espera mi confirmación antes de empezar el Bloque 1. Después del Bloque 1, sigue
   autónomamente sin pedir confirmación entre bloques, pero al terminar cada bloque haz un
   commit con mensaje descriptivo y un resumen de 3–5 líneas de lo hecho.

## Objetivo final

Un monorepo plug-and-play que yo pueda clonar, correr `docker compose up` para PostgreSQL,
`dotnet run` para el backend y `npm run dev` para el frontend, y que arranque sin tocar nada más
que un `.env` con credenciales. El código debe ser completo, compilable y con pruebas que pasen.
No me dejes métodos con `throw new NotImplementedException()`.

## Bloques a entregar (en este orden)

### Bloque 1 — Estructura base y tooling
- Crea `PetrolRios.sln` con los 5 proyectos de `src/` y los 3 de `tests/` descritos en `CLAUDE.md`.
- Configura referencias entre proyectos siguiendo Clean Architecture (Domain no depende de nadie;
  Application depende solo de Domain; Infrastructure depende de Application; Api depende de todo;
  Detectors depende de Domain y Application).
- Añade los paquetes NuGet necesarios en cada proyecto (EF Core 9, Npgsql, Dapper,
  FirebirdSql.Data.FirebirdClient, Hangfire.AspNetCore, Hangfire.PostgreSql, SignalR,
  Microsoft.AspNetCore.Authentication.JwtBearer, Serilog, FluentValidation, Mapster, xUnit,
  FluentAssertions, Moq, Testcontainers.PostgreSql).
- Crea `frontend/` con Vite + React 18 + TS + Tailwind + shadcn/ui inicializados.
- `docker-compose.yml` en la raíz con PostgreSQL 16 y el dashboard de Hangfire accesible.
- `.editorconfig`, `.gitignore` (.NET + Node + IDE), `README.md` con instrucciones claras
  paso a paso para arrancar en local, `appsettings.Development.json` con valores de ejemplo.
- **Criterio de aceptación:** `dotnet build` pasa sin errores ni warnings; `npm run build`
  pasa sin errores.

### Bloque 2 — Dominio y persistencia central (PostgreSQL)
- Entidades en `PetrolRios.Domain`: `Alerta`, `Estacion`, `Usuario`, `Rol`, `ReglaDeteccion`,
  `EjecucionJob`, `TransaccionStaging`, `EstacionWatermark`, `AsignacionAlerta`, `LogAuditoria`,
  `RefreshToken`. Usa `record` donde tenga sentido; propiedades con setters privados donde aplique.
- Enums: `TipoDetector` (CashFraud, InvoiceAnomaly, PaymentFraud, ComplianceViolation),
  `NivelRiesgo` (Bajo, Medio, Alto, Critico), `EstadoAlerta` (Nueva, EnRevision, Confirmada,
  FalsoPositivo, Cerrada), `EstadoJob` (Pendiente, EnProgreso, Completado, Fallido).
- `PetrolRiosDbContext` en Infrastructure con configuraciones Fluent API en clases separadas
  (`IEntityTypeConfiguration<T>`). Índices en columnas de filtrado frecuente: `FechaDeteccion`,
  `EstacionId`, `NivelRiesgo`, `TipoDetector`, `EstadoAlerta`.
- Migración inicial `InitialCreate` y un `SeedData` que inserte:
  los 3 roles, un usuario admin (contraseña hasheada con BCrypt), las 10 estaciones de ejemplo,
  y las reglas por defecto de cada detector con los umbrales de la tesis Tabla 3.
- Repositorios (interfaces en Application, implementaciones en Infrastructure) con
  Unit of Work. Un `IAlertaRepository`, `IEstacionRepository`, `IUsuarioRepository`,
  `IReglaDeteccionRepository`.

### Bloque 3 — Adaptador Firebird (solo lectura) y ETL con watermark
- Interfaz `IFirebirdSourceClient` en Application con métodos tipados:
  `GetFacturasDesdeAsync(DateTime watermark, CancellationToken ct)`,
  `GetDetallesFacturaAsync(...)`, `GetCierresTurnoAsync(...)`, `GetAnulacionesAsync(...)`,
  `GetCreditosAsync(...)`, `GetTarjetasTurnoAsync(...)`.
- Implementación `FirebirdSourceClient` en Infrastructure usando Dapper contra las tablas
  reales del schema. Connection string con `ReadOnly=true`. **SQL solo SELECT.**
- DTOs en Application (`FacturaDto`, `DetalleFacturaDto`, `CierreTurnoDto`, `AnulacionDto`,
  `CreditoDto`, `TarjetaTurnoDto`) — estos son el "modelo canónico" al que mapeas desde las
  columnas reales de Contaplus.
- Servicio `EtlOrchestrator` que: para cada estación (de la tabla `Estaciones` en PostgreSQL)
  lee su watermark, invoca `IFirebirdSourceClient`, carga a tablas staging, actualiza watermark,
  registra errores sin detener el proceso global.
- Resiliencia con Polly (retry + circuit breaker) en las llamadas a Firebird.
- Pruebas unitarias de `EtlOrchestrator` mockeando `IFirebirdSourceClient`.

### Bloque 4 — Los 4 detectores (Strategy Pattern) + motor de scoring
Este es el corazón del proyecto. Debe quedar impecable.

- Interfaz `IAnomalyDetector` con `Task<IReadOnlyList<Alerta>> DetectAsync(DetectionContext ctx, CancellationToken ct)`.
- `DetectionContext` contiene las colecciones de DTOs ya cargadas en staging y la configuración
  de reglas vigente.
- Implementaciones:
  1. **`CashFraudDetector`** — compara suma de ventas en efectivo de cada turno (`TURN` + `FACT`
     filtrando medio de pago efectivo) contra efectivo reportado en `TURN_DEPO`. Genera alerta
     si diferencia > umbral. Segunda regla: consulta histórico de alertas Cash Fraud del mismo
     empleado en los últimos 30 días y si hay > 3 ocurrencias genera alerta de patrón.
  2. **`InvoiceAnomalyDetector`** — cruza `FACT`/`DCTO` contra `PREC` para detectar precios fuera
     de lista; calcula tasa de anulación por empleado desde `ANUL` y genera alerta si > umbral;
     detecta campos obligatorios vacíos en `FACT` (placa, identificación) según configuración.
  3. **`PaymentFraudDetector`** — identifica reversiones en `TURN_TARJ` con diferencia temporal
     > 30 min respecto a la venta original en `FACT`; detecta créditos en `CRED`/`CRED_CABE`
     que excedan `CLIE.limite` sin autorización; detecta transacciones duplicadas.
  4. **`ComplianceViolationDetector`** — filtra `DCTO`/`FACT` donde placa = 'ZZZ999949' con
     galones > 5; detecta misma `PLACA` con múltiples tipos de combustible en el mismo día;
     valida horarios de operación configurados por estación.
- Todos los umbrales leídos desde `ReglaDeteccion` (no hardcodeados).
- **Motor de scoring** `RiskScoringEngine`: calcula score 0–100 con `RiesgoBase × Multiplicadores`
  (multiplicador por monto, reincidencia del empleado, historial de la estación). Mapea a `NivelRiesgo`.
- Registro vía DI: `services.AddScoped<IAnomalyDetector, CashFraudDetector>()` y los otros 3;
  inyección como `IEnumerable<IAnomalyDetector>` donde se consuman en paralelo con `Task.WhenAll`.
- **Pruebas unitarias exhaustivas** en `PetrolRios.Detectors.Tests` con datos sintéticos que
  cubran casos positivos, negativos y bordes. **Objetivo > 80% de cobertura** (requisito OE5).
  Usa xUnit + FluentAssertions. Crea al menos 6 tests por detector.

### Bloque 5 — Hangfire + SignalR
- Configuración de Hangfire con PostgreSQL storage.
- Job recurrente `AnomalyDetectionJob` configurado con cron cada 5 minutos (configurable en
  `appsettings`). El job:
  1. Llama al `EtlOrchestrator` (Bloque 3).
  2. Ejecuta los 4 detectores en paralelo con `Task.WhenAll`.
  3. Aplica scoring.
  4. Persiste alertas en PostgreSQL.
  5. Notifica por SignalR a los grupos de usuarios correspondientes.
  6. Registra `EjecucionJob` con métricas (duración, alertas generadas, estaciones con error).
- Reintentos automáticos 3 veces con backoff exponencial. Dashboard de Hangfire protegido por
  autenticación JWT y rol Administrador.
- `AlertsHub : Hub` en ruta `/hubs/alerts`. Método `EnviarAlerta(Alerta alerta)` que emite a los
  grupos `auditores`, `supervisores`, `estacion-{id}`. Los clientes se unen a grupos al conectar
  según su rol.

### Bloque 6 — API REST + JWT/RBAC
- Endpoints (todos con OpenAPI/Swagger documentado, versionado v1):
  - `POST /api/v1/auth/login`, `POST /api/v1/auth/refresh`, `POST /api/v1/auth/logout`
  - `GET /api/v1/alertas` (con filtros: tipo, estación, nivelRiesgo, estado, fechaDesde,
    fechaHasta, paginación, ordenamiento)
  - `GET /api/v1/alertas/{id}`, `PATCH /api/v1/alertas/{id}/estado`, `POST /api/v1/alertas/{id}/asignar`
  - `GET /api/v1/dashboard/kpis`, `GET /api/v1/dashboard/alertas-por-tipo`,
    `GET /api/v1/dashboard/alertas-por-estacion`
  - `GET/POST/PUT/DELETE /api/v1/reglas` (solo Admin/Supervisor)
  - `GET/POST/PUT/DELETE /api/v1/usuarios` (solo Admin)
  - `GET /api/v1/logs` (solo Admin)
- Autorización declarativa con `[Authorize(Roles = "...")]`.
- Middleware de logging estructurado con Serilog, manejo global de excepciones con
  `ProblemDetails`, CORS configurado para el frontend, rate limiting básico.
- FluentValidation para todos los DTOs de entrada.
- **Sin lógica de negocio en los controllers:** delegan a servicios de Application.

### Bloque 7 — Frontend React 18 + TypeScript
- Configuración: Vite, Tailwind, shadcn/ui con tema oscuro/claro, React Router v6,
  TanStack Query para data fetching, cliente Axios con interceptor JWT y refresh automático,
  cliente SignalR conectado tras login.
- Páginas: `Login`, `Dashboard` (KPIs con Recharts: alertas por día, por tipo, por estación,
  por nivel), `Alertas` (tabla con filtros, ordenamiento, paginación, búsqueda),
  `DetalleAlerta` (con historial y acciones), `Reglas` (solo supervisor/admin),
  `Usuarios` (solo admin), `NotFound`.
- Componente `NotificationProvider` que escucha el hub SignalR y muestra toasts cuando llegan
  alertas nuevas. El contador de alertas pendientes se actualiza en vivo.
- Tipos TypeScript compartidos generados desde los DTOs del backend (crea un script o genéralos
  manualmente asegurando paridad).
- Protección de rutas por rol.
- **Criterio de aceptación:** `npm run build` pasa sin errores, sin `any`, sin warnings.

### Bloque 8 — Pruebas, documentación y cierre
- Tests de integración del API con `WebApplicationFactory` y Testcontainers.PostgreSql.
- Un test end-to-end de `AnomalyDetectionJob` con datos sintéticos que verifique el flujo
  ETL → detectores → scoring → persistencia → notificación.
- `README.md` completo en la raíz con: descripción, arquitectura (diagrama ASCII o mermaid),
  prerrequisitos, pasos de instalación, variables de entorno, comandos útiles,
  cómo correr los tests, cómo generar la cobertura.
- Documento `docs/ARQUITECTURA.md` con los diagramas C4 nivel 2, 3 y 4 en formato Mermaid
  (Context, Containers, Components, Code/ER).
- Script `scripts/coverage.sh` (y `.ps1`) que corre todas las pruebas y genera reporte de cobertura.

## Reglas de interacción conmigo

- **Al iniciar cada bloque:** dime en 1–2 líneas qué vas a hacer.
- **Al terminar cada bloque:** haz un commit con mensaje descriptivo, dame un resumen de 3–5 líneas,
  lista los archivos más importantes creados y continúa automáticamente con el siguiente bloque.
- **Si encuentras ambigüedad** entre la tesis y una buena práctica de ingeniería, prioriza la
  tesis si es una restricción explícita; si no, elige la buena práctica y explica brevemente por qué.
- **Si una tabla o columna de Contaplus que necesitas no está clara** en el schema, detente y
  pregúntame. No inventes.
- **Si un test falla**, arréglalo antes de continuar. No dejes tests rotos "para después".
- **No generes código placeholder.** Todo lo que escribas debe ser funcional.

Empieza ahora leyendo `CLAUDE.md`, `docs/tesis.md` y `docs/contac-schema.sql`, y preséntame
el plan del Bloque 1. Cuando te diga "adelante", ejecuta.
