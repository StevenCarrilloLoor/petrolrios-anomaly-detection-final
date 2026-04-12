# Arquitectura del Sistema — PetrolRios

Diagramas C4 en formato Mermaid que describen la arquitectura del sistema de deteccion de
anomalias transaccionales.

## Nivel 1 — Diagrama de Contexto

```mermaid
C4Context
    title Sistema de Deteccion de Anomalias — Contexto

    Person(auditor, "Auditor", "Revisa alertas y documenta hallazgos")
    Person(supervisor, "Supervisor", "Asigna casos, configura umbrales, genera reportes")
    Person(admin, "Administrador", "Gestiona usuarios, roles y configuracion del sistema")

    System(petrolrios, "PetrolRios Anomaly Detection", "Detecta anomalias transaccionales en estaciones de servicio")

    System_Ext(firebird, "Bases Firebird", "10 bases de datos Contaplus (solo lectura) en estaciones de servicio")

    Rel(auditor, petrolrios, "Revisa alertas", "HTTPS + WebSocket")
    Rel(supervisor, petrolrios, "Configura y supervisa", "HTTPS + WebSocket")
    Rel(admin, petrolrios, "Administra", "HTTPS")
    Rel(petrolrios, firebird, "Extrae transacciones", "Firebird SQL (solo lectura)")
```

## Nivel 2 — Diagrama de Contenedores

```mermaid
C4Container
    title Sistema de Deteccion de Anomalias — Contenedores

    Person(usuario, "Usuario", "Auditor, Supervisor o Administrador")

    Container_Boundary(sistema, "PetrolRios") {
        Container(frontend, "Frontend SPA", "React 19, TypeScript, Vite, TailwindCSS", "Dashboard, alertas, configuracion")
        Container(api, "API REST", "ASP.NET Core 9, C# 13", "Endpoints HTTP + autenticacion JWT")
        Container(signalr, "SignalR Hub", "ASP.NET Core SignalR", "Notificaciones en tiempo real via WebSocket")
        Container(hangfire, "Hangfire Worker", "Hangfire + PostgreSQL", "Ejecuta job de deteccion cada 5 min")
        ContainerDb(postgres, "PostgreSQL 16", "Base de datos central", "Alertas, usuarios, reglas, staging, logs")
    }

    System_Ext(firebird, "10 x Firebird", "Bases Contaplus (solo lectura)")

    Rel(usuario, frontend, "Usa", "HTTPS")
    Rel(frontend, api, "Consume", "HTTPS/JSON")
    Rel(frontend, signalr, "Escucha", "WebSocket")
    Rel(api, postgres, "Lee/Escribe", "Npgsql")
    Rel(hangfire, postgres, "Lee/Escribe", "Npgsql")
    Rel(hangfire, firebird, "Extrae datos", "FirebirdSql.Data (solo lectura)")
    Rel(hangfire, signalr, "Notifica alertas", "IHubContext")
    Rel(signalr, postgres, "Lee", "Npgsql")
```

## Nivel 3 — Diagrama de Componentes (Backend)

```mermaid
C4Component
    title Backend — Componentes internos

    Container_Boundary(api_layer, "PetrolRios.Api") {
        Component(auth_ctrl, "AuthController", "Controller", "Login, refresh, logout")
        Component(alertas_ctrl, "AlertasController", "Controller", "CRUD de alertas")
        Component(dashboard_ctrl, "DashboardController", "Controller", "KPIs y metricas")
        Component(reglas_ctrl, "ReglasController", "Controller", "Configuracion de reglas")
        Component(usuarios_ctrl, "UsuariosController", "Controller", "Gestion de usuarios")
        Component(logs_ctrl, "LogsController", "Controller", "Logs de auditoria")
        Component(jwt_middleware, "JWT Middleware", "Middleware", "Autenticacion Bearer")
        Component(exception_middleware, "ExceptionHandling", "Middleware", "Manejo global de errores")
    }

    Container_Boundary(app_layer, "PetrolRios.Application") {
        Component(auth_svc, "IAuthService", "Servicio", "Autenticacion y tokens")
        Component(alerta_svc, "IAlertaService", "Servicio", "Logica de alertas")
        Component(dashboard_svc, "IDashboardService", "Servicio", "Calculo de KPIs")
        Component(regla_svc, "IReglaService", "Servicio", "Gestion de reglas")
        Component(usuario_svc, "IUsuarioService", "Servicio", "Gestion de usuarios")
    }

    Container_Boundary(detectors_layer, "PetrolRios.Detectors") {
        Component(cash_det, "CashFraudDetector", "Strategy", "Fraude de efectivo")
        Component(invoice_det, "InvoiceAnomalyDetector", "Strategy", "Anomalias de factura")
        Component(payment_det, "PaymentFraudDetector", "Strategy", "Fraude de pago")
        Component(compliance_det, "ComplianceViolationDetector", "Strategy", "Violaciones normativas")
        Component(scoring, "RiskScoringEngine", "Singleton", "Calculo de score 0-100")
    }

    Container_Boundary(infra_layer, "PetrolRios.Infrastructure") {
        Component(dbcontext, "PetrolRiosDbContext", "EF Core", "Acceso a PostgreSQL")
        Component(repos, "Repositories", "Repository Pattern", "IAlertaRepository, IEstacionRepository, etc.")
        Component(uow, "UnitOfWork", "UoW Pattern", "Transaccionalidad")
        Component(etl, "EtlOrchestrator", "Servicio", "Extraccion incremental de Firebird")
        Component(firebird_client, "FirebirdSourceClient", "Dapper", "Queries SQL solo lectura")
        Component(job, "AnomalyDetectionJob", "Hangfire Job", "Ciclo ETL -> Deteccion -> Notificacion")
        Component(hub, "AlertsHub", "SignalR Hub", "Notificaciones en tiempo real")
        Component(jwt_svc, "JwtService", "Servicio", "Generacion y validacion de tokens")
    }

    Rel(auth_ctrl, auth_svc, "Delega")
    Rel(alertas_ctrl, alerta_svc, "Delega")
    Rel(dashboard_ctrl, dashboard_svc, "Delega")
    Rel(auth_svc, jwt_svc, "Genera tokens")
    Rel(auth_svc, repos, "Consulta usuarios")
    Rel(alerta_svc, repos, "CRUD alertas")
    Rel(job, etl, "Paso 1: Extrae datos")
    Rel(job, cash_det, "Paso 2: Detecta")
    Rel(job, invoice_det, "Paso 2: Detecta")
    Rel(job, payment_det, "Paso 2: Detecta")
    Rel(job, compliance_det, "Paso 2: Detecta")
    Rel(cash_det, scoring, "Calcula score")
    Rel(job, hub, "Paso 3: Notifica")
    Rel(etl, firebird_client, "Extrae de Firebird")
    Rel(repos, dbcontext, "Accede a datos")
```

## Nivel 4 — Diagrama ER (Entidades principales)

```mermaid
erDiagram
    USUARIO {
        int Id PK
        string Email UK
        string NombreCompleto
        string PasswordHash
        int RolId FK
        bool Activo
        datetime CreatedAt
        datetime UpdatedAt
    }

    ROL {
        int Id PK
        string Nombre UK
        string Descripcion
    }

    ESTACION {
        int Id PK
        string Nombre
        string Codigo UK
        string Direccion
        string Zona
        time HoraApertura
        time HoraCierre
        bool Activa
    }

    ALERTA {
        int Id PK
        enum TipoDetector
        enum NivelRiesgo
        enum EstadoAlerta
        string Descripcion
        double Score
        datetime FechaDeteccion
        string EmpleadoCodigo
        string TransaccionReferencia
        string MetadataJson
        int EstacionId FK
        int EjecucionJobId FK
    }

    REGLA_DETECCION {
        int Id PK
        enum TipoDetector
        string Nombre
        string Descripcion
        string ParametroNombre
        double ValorUmbral
        bool Activa
    }

    TRANSACCION_STAGING {
        int Id PK
        int EstacionId FK
        string TipoTransaccion
        string DataJson
        datetime FechaOriginal
        bool Procesada
    }

    ESTACION_WATERMARK {
        int Id PK
        int EstacionId FK
        datetime UltimaExtraccion
    }

    EJECUCION_JOB {
        int Id PK
        enum EstadoJob
        int AlertasGeneradas
        int EstacionesProcesadas
        int EstacionesConError
        double DuracionSegundos
        string Error
        datetime FechaInicio
        datetime FechaFin
    }

    ASIGNACION_ALERTA {
        int Id PK
        int AlertaId FK
        int AuditorId FK
        int AsignadoPorId FK
        string Comentario
        datetime FechaAsignacion
    }

    LOG_AUDITORIA {
        int Id PK
        int UsuarioId FK
        string Accion
        string Entidad
        int EntidadId
        string DetalleJson
        datetime Fecha
    }

    REFRESH_TOKEN {
        int Id PK
        int UsuarioId FK
        string Token
        datetime Expiracion
        bool Revocado
    }

    ROL ||--o{ USUARIO : "tiene"
    USUARIO ||--o{ REFRESH_TOKEN : "tiene"
    USUARIO ||--o{ LOG_AUDITORIA : "genera"
    ESTACION ||--o{ ALERTA : "genera"
    ESTACION ||--|| ESTACION_WATERMARK : "tiene"
    ESTACION ||--o{ TRANSACCION_STAGING : "contiene"
    EJECUCION_JOB ||--o{ ALERTA : "genera"
    ALERTA ||--o{ ASIGNACION_ALERTA : "tiene"
    USUARIO ||--o{ ASIGNACION_ALERTA : "asignado a"
```

## Flujo de deteccion de anomalias

```mermaid
sequenceDiagram
    participant HF as Hangfire (cada 5 min)
    participant ETL as EtlOrchestrator
    participant FB as Firebird (10 estaciones)
    participant PG as PostgreSQL
    participant DET as Detectores (x4)
    participant SE as ScoringEngine
    participant SR as SignalR Hub
    participant FE as Frontend React

    HF->>ETL: ExecuteAsync()
    loop Por cada estacion activa
        ETL->>PG: Leer watermark
        ETL->>FB: SELECT desde watermark (solo lectura)
        FB-->>ETL: Facturas, cierres, anulaciones, etc.
        ETL->>PG: INSERT en TransaccionStaging
        ETL->>PG: Actualizar watermark
    end
    ETL-->>HF: EtlResult

    HF->>PG: Obtener estaciones y reglas activas
    loop Por cada estacion
        HF->>PG: Leer staging no procesada
        par Ejecucion en paralelo
            HF->>DET: CashFraudDetector.DetectAsync()
            HF->>DET: InvoiceAnomalyDetector.DetectAsync()
            HF->>DET: PaymentFraudDetector.DetectAsync()
            HF->>DET: ComplianceViolationDetector.DetectAsync()
        end
        DET->>SE: Calcular score y nivel de riesgo
        SE-->>DET: DetectedAnomaly[]
        DET-->>HF: Anomalias detectadas

        loop Por cada anomalia
            HF->>PG: INSERT Alerta
            HF->>SR: NuevaAlerta (grupos: auditores, supervisores, estacion-N)
            SR->>FE: WebSocket push
        end
        HF->>PG: Marcar staging como procesada
    end
    HF->>PG: Registrar EjecucionJob con metricas
```

## Modelo de scoring

```mermaid
graph LR
    A[Anomalia detectada] --> B{Calcular Score}
    B --> C[RiesgoBase<br/>segun tipo de regla]
    C --> D[x Multiplicador Monto<br/>1.0 - 2.0]
    D --> E[x Multiplicador Reincidencia<br/>1.0 - 1.5]
    E --> F[x Multiplicador Estacion<br/>1.0 - 1.3]
    F --> G{Score final<br/>clamped 0-100}
    G -->|0-25| H[Bajo]
    G -->|26-50| I[Medio]
    G -->|51-75| J[Alto]
    G -->|76-100| K[Critico]
```
