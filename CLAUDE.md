# PetrolRíos — Sistema de Detección de Anomalías Transaccionales

Este es un proyecto de tesis de Ingeniería de Software. La fuente de verdad absoluta del proyecto
es el documento `docs/tesis.md`. Antes de tomar decisiones de diseño o implementar funcionalidad,
consulta ese documento.

## Contexto del proyecto

Sistema web para PetrolRíos S.A. que detecta anomalías transaccionales en ~13,000–15,000
transacciones diarias provenientes de 10 estaciones de servicio, cada una con una base de datos
Firebird (Contaplus, archivo `CONTAC.FDB`) en modo **solo lectura**. El sistema ejecuta 4
detectores cada 5–10 minutos mediante Hangfire y notifica alertas en tiempo real mediante SignalR.

## Stack obligatorio (NO cambiar, es una restricción de tesis)

- **Backend:** ASP.NET Core 9.0 (C# 13), EF Core 9, Dapper (queries de alto rendimiento),
  Hangfire (jobs batch), SignalR (notificaciones), JWT + RBAC, xUnit + FluentAssertions + Moq
- **Frontend:** React 18 + TypeScript 5, Vite, TailwindCSS, shadcn/ui, TanStack Query,
  React Router, @microsoft/signalr, Recharts, Zod
- **BD central:** PostgreSQL 16 (en AWS RDS en producción, local en desarrollo)
- **Fuentes:** 10 Firebird CONTAC.FDB vía `FirebirdSql.Data.FirebirdClient` en modo **solo lectura**
- **Arquitectura:** En capas (Clean Architecture ligera) con Repository, Unit of Work,
  Strategy Pattern (para detectores), Dependency Injection nativa de ASP.NET Core

## Estructura de solución esperada

```
PetrolRios.sln
├── src/
│   ├── PetrolRios.Domain/          # Entidades, enums, interfaces de dominio. Sin dependencias externas.
│   ├── PetrolRios.Application/     # Casos de uso, DTOs, interfaces de repositorios, IAnomalyDetector
│   ├── PetrolRios.Infrastructure/  # EF Core DbContext, repositorios, Dapper, clientes Firebird, Hangfire, SignalR hub
│   ├── PetrolRios.Api/             # Controllers, Program.cs, middlewares, JWT, configuración
│   └── PetrolRios.Detectors/       # Los 4 detectores (Strategy Pattern) + motor de scoring
├── tests/
│   ├── PetrolRios.Domain.Tests/
│   ├── PetrolRios.Detectors.Tests/ # CRÍTICO: cobertura > 80% aquí (OE5)
│   └── PetrolRios.Api.Tests/
└── frontend/                        # React 18 + TS + Vite
    ├── src/
    │   ├── components/
    │   ├── pages/
    │   ├── hooks/
    │   ├── services/                # API client + SignalR client
    │   ├── types/
    │   └── lib/
    └── ...
```

## Reglas de implementación innegociables

1. **Las bases Firebird son SOLO LECTURA.** Toda conexión usa `ReadOnly = true`. Nunca INSERT/UPDATE/DELETE.
2. **Sigue las tablas reales** del archivo `docs/contac-schema.sql`. No inventes nombres.
   Las tablas clave (Contaplus) son: `FACT` (facturas), `DCTO` (documentos/detalle), `TURN` (turnos),
   `TURN_DEPO` (depósitos turno), `TURN_TARJ` (tarjetas turno), `ANUL` (anulaciones),
   `CRED` / `CRED_CABE` / `CRED_MOVI` (créditos), `CLIE` (clientes), `EMPL` (empleados),
   `PLACA` (placas), `VENT` (ventas), `PREC` (precios), `TANQ` (tanques), `ESTA` (estaciones).
   **Siempre verifica los nombres reales de columnas en el schema antes de escribir SQL.**
3. **Los 4 detectores implementan `IAnomalyDetector`** con Strategy Pattern:
   - `CashFraudDetector`
   - `InvoiceAnomalyDetector`
   - `PaymentFraudDetector`
   - `ComplianceViolationDetector`
   Cada detector debe ser configurable vía `ReglaDeteccion` persistida en PostgreSQL (umbrales editables).
4. **Scoring de riesgo 0–100:** `Score = RiesgoBase × Multiplicadores`. Niveles: Bajo 0–25,
   Medio 26–50, Alto 51–75, Crítico 76–100.
5. **Watermark por estación:** cada ciclo ETL consulta solo transacciones posteriores a la última
   marca de agua de esa estación. Persiste en tabla `EstacionWatermark` en PostgreSQL.
6. **Tolerancia a fallos:** si una estación falla, el proceso continúa con las demás. Usa los
   reintentos automáticos de Hangfire con backoff exponencial.
7. **Hangfire ejecutado como job recurrente** cada 5–10 minutos (configurable desde `appsettings.json`).
8. **SignalR hub** en ruta `/hubs/alerts`. Grupos por rol (Auditor, Supervisor, Admin) y por estación.
9. **JWT con refresh tokens.** RBAC con 3 roles: `Auditor`, `Supervisor`, `Administrador`.
10. **Cobertura de pruebas unitarias > 80% en `PetrolRios.Detectors`** (requisito OE5 de la tesis).
11. **Nombres de código en inglés**, comentarios y mensajes de usuario en **español**.
12. **Todo inyectado por DI.** Nada de `new` para servicios, repositorios o detectores.
13. **EF Core Code-First** con migraciones. PostgreSQL como provider (`Npgsql.EntityFrameworkCore.PostgreSQL`).
14. **Configuración sensible en `appsettings.Development.json`** y variables de entorno. Nunca
    hardcodear connection strings, JWT secrets, etc.

## Casos de uso (resumen — ver tesis sección 4.1.2 para detalle)

- **Auditor (CU-01 a CU-10):** login, ver dashboard, listar alertas, ver detalle, marcar como
  revisada / falso positivo / confirmada, filtrar alertas, recibir notificación en tiempo real.
- **Supervisor (CU-11 a CU-14):** además, asignar alertas a auditores, generar reportes PDF/Excel,
  ver métricas/KPIs, configurar umbrales de detectores.
- **Administrador (CU-15 a CU-17):** gestionar usuarios y roles, configurar reglas, consultar logs de auditoría.
- **Sistema/Hangfire (CU-18 a CU-20):** extraer transacciones de Firebird, ejecutar los 4 detectores,
  persistir alertas y notificar por SignalR.

## Reglas de detección (ver tesis Tabla 3 — estas son SOLO los umbrales por defecto, deben ser editables)

**Cash Fraud:**
- Diferencia efectivo reportado vs sistema > $50 por turno
- Mismo empleado con faltantes > 3 veces en 30 días (patrón gineteo)

**Invoice Anomaly:**
- Anulaciones por empleado > 5% de transacciones diarias (umbral normal < 2%)
- `precio_aplicado > precio_autorizado` o `descuento > descuento_maximo_permitido`
- Campos obligatorios vacíos (placa, cédula) según configuración

**Payment Fraud:**
- Reversión de tarjeta > 30 minutos después de la venta original
- Crédito otorgado > `limite_cliente` sin `codigo_autorizacion`
- Transacciones duplicadas (misma tarjeta, mismo monto, < 5 min de diferencia)

**Compliance Violation:**
- `placa = 'ZZZ999949'` AND `galones > 5` (regulación ARCERNNR)
- Misma placa con diésel Y extra (gasolina) en el mismo día
- Operaciones fuera de horario configurado por estación

## Convenciones de código

- **C#:** nullable reference types activado, `file-scoped namespaces`, `record` para DTOs inmutables,
  `async`/`await` en todo el pipeline, `CancellationToken` en métodos async públicos.
- **TypeScript:** `strict: true`, sin `any`, tipos inferidos desde Zod schemas cuando sea posible.
- **Git:** commits en español en imperativo ("Añadir detector de Cash Fraud"). Conventional commits opcional.
- **Nombres:** `PascalCase` para clases/tipos, `camelCase` para variables/métodos,
  `SCREAMING_SNAKE_CASE` para constantes.

## Mantenimiento de documentación (contexto legado — OBLIGATORIO)

Con **CADA** cambio que se agregue, modifique o elimine (no solo al final de una sesión), actualiza la
documentación de contexto legado para que, si la conversación se compacta, el siguiente arranque no
pierda el hilo:

1. **`CAMBIOS.md`** — añade una sección numerada nueva (motivación, qué se hizo, **verificación** y los
   hashes de commit). Es la bitácora oficial del proyecto.
2. **`docs/PENDIENTES.md`** — marca lo hecho, agrega lo nuevo que quede pendiente y actualiza la fecha
   de "Última actualización".
3. **`docs/PROMPT-REINICIO-CONTEXTO.md`** — actualiza la sección 6 "Estado actual del trabajo" (commits
   recientes, conteos de pruebas, versión del agente).

Mantén también al día los docs que dependan del cambio (`README.md`, `INSTALACION.md`,
`ejecutables/LEEME.md`, etc.) cuando el cambio los afecte. La tesis (`docs/Tesis.md`) está
**desactualizada y NO se edita**: el código es la fuente de verdad.

## Lo que NO debes hacer

- No uses Machine Learning — la tesis explícitamente lo excluye (L02).
- No crees una app móvil nativa (L01).
- No hagas migración de datos históricos (L04).
- No modifiques las bases Firebird (R08).
- No inventes tablas o columnas que no estén en `docs/contac-schema.sql`.
- No uses Windows Forms, WPF, Blazor ni otra tecnología fuera del stack.
- No pongas lógica de negocio en los Controllers.
- No expongas entidades de dominio directamente en los endpoints; usa DTOs.
