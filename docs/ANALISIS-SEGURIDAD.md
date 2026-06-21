# Análisis de Seguridad — PetrolRíos (Sistema de Detección de Anomalías)

> Revisión de código orientada a inyección, interceptación de datos, manejo de secretos
> y superficie de ataque. Fundamentada en el código real del repositorio (junio 2026).
> Postura general: **sólida para un sistema interno**; los hallazgos son en su mayoría de
> severidad baja y de tipo "endurecer para producción".

## 1. Resumen ejecutivo

| Área | Estado | Riesgo residual |
|---|---|---|
| Autenticación / autorización | JWT + refresh revocables, RBAC, bloqueo, 2FA, BCrypt | **Bajo** |
| Inyección SQL (Firebird) | Solo lectura + parámetros + identificadores validados contra catálogo | **Bajo** |
| Inyección SQL (PostgreSQL) | EF Core + Dapper parametrizado | **Bajo** |
| Motor de reglas personalizadas | Evaluador AST propio, sin `eval` ni generación de SQL | **Bajo** |
| Interceptación (TLS / wire) | HTTPS forzado + WireCrypt en Firebird | **Bajo–Medio** (confirmar en prod) |
| Gestión de secretos | `.gitignore` excluye secretos; nada sensible en el repo | **Bajo** |
| Superficie anónima (login/reset/unlock) | Respuestas neutras + bloqueo | **Medio–Bajo** (falta rate-limit por IP) |

No se detectaron vulnerabilidades críticas ni de severidad alta. Las recomendaciones de la
sección 8 son mejoras de endurecimiento.

## 2. Autenticación y autorización

- **Hash de contraseñas:** `BCrypt.Net.BCrypt` (salting + factor de trabajo). Nunca se guardan
  contraseñas en claro ni reversibles.
- **JWT + refresh tokens:** el access token caduca a los 60 min (`Jwt:ExpirationMinutes`); el
  refresh es persistente, **revocable** (`RefreshToken.Revoked`) y se invalida al rotarlo. Logout
  revoca el refresh.
- **RBAC:** tres roles (`Auditor`, `Supervisor`, `Administrador`) aplicados con `[Authorize(Roles=…)]`
  en el backend **y** `ProtectedRoute` en el frontend (defensa en profundidad). Las rutas de
  gestión (Usuarios, Logs) son solo Administrador; Reglas/Reportes son Supervisor+Admin.
- **Bloqueo anti fuerza bruta:** 5 intentos fallidos → bloqueo de 15 min (`MaxIntentosLogin`,
  `MinutosBloqueo`). Verificado en vivo.
- **2FA (TOTP):** opcional por cuenta (`ITotpService`), con enrolamiento en dos pasos y validación
  de código.
- **Aislamiento por estación:** las cuentas de agente se ciñen a su estación vía claims del JWT; el
  hub SignalR las une **solo** a su grupo `estacion-{id}` (no confía en query string). El acceso
  cruzado entre estaciones está bloqueado.

## 3. Inyección

### 3.1 Firebird (fuentes de estación) — **solo lectura**
- **Todas** las conexiones usan `ReadOnly=true` (restricción R08): aunque se lograra inyectar, no
  hay escritura posible.
- Las consultas estándar (DCTO, DESP, TURN, ANUL, CRED_CABE, TURN_DEPO, TURN_TARJ) son cadenas
  **fijas** con el valor de marca de agua **parametrizado** (`@Watermark`). No hay concatenación de
  entrada de usuario.
- Las **fuentes configurables** (tabla elegida desde el panel) sí interpolan el nombre de tabla y de
  columna en el SQL — pero **antes** se valida que la tabla exista (`RDB$RELATIONS`) y que la
  columna de marca de agua exista y sea de tipo fecha (`RDB$RELATION_FIELDS`); el valor sigue
  parametrizado y los identificadores van entre comillas dobles. Un atacante no puede inyectar SQL
  arbitrario porque el identificador debe coincidir con un objeto real del catálogo.
- **Recomendación menor:** además de la verificación de existencia, aplicar una lista blanca de
  caracteres (`^[A-Za-z0-9_$]+$`) sobre tabla/columna como segunda barrera (defensa en profundidad).

### 3.2 PostgreSQL (central)
- Acceso principal vía **EF Core** (consultas parametrizadas por diseño). Los pocos usos de **Dapper**
  emplean parámetros nombrados (`@…`). No se observó interpolación de entrada de usuario en SQL.

### 3.3 Motor de reglas personalizadas
- Las expresiones avanzadas se compilan con un **evaluador AST propio**
  (`EvaluadorExpresion`/`ExpresionAst`): tokeniza → parsea → evalúa sobre el registro, con un
  conjunto cerrado de operadores. **No** usa `eval`, Roslyn, `DataTable.Compute` ni genera SQL desde
  la entrada del usuario, y valida que los campos referenciados existan. Es el diseño correcto y
  seguro.
- **Recomendación menor:** acotar longitud/profundidad de la expresión al compilar para evitar DoS
  por expresiones gigantes.

## 4. Interceptación de datos

- **HTTPS forzado** (`Seguridad:ForzarHttps = true`): redirección + HSTS. Confirmar que en producción
  el certificado sea válido y que el agente→central viaje siempre por TLS (la URL del central es
  configurable: en prod debe ser `https://`).
- **Firebird con `WireCrypt=Enabled`:** el tráfico agente↔Firebird va cifrado a nivel de protocolo.
- **JWT firmado** con clave de `Jwt:Key` tomada de configuración/entorno (no hardcodeada). El token
  no contiene secretos, solo claims de identidad/rol/estación.
- **SignalR** exige autenticación explícita; sin token no hay conexión ni aparición como usuario
  conectado.

## 5. Gestión de secretos

- `.gitignore` excluye `**/appsettings.Secrets.json`, `appsettings.Production.json` y `.env*`. La
  App Password de Gmail (SMTP) y los secretos viven fuera del repositorio.
- `appsettings.Development.json` **sí** contiene credenciales de la BD de desarrollo en claro
  (`petrolrios_dev_2025`). Es aceptable para entorno local, pero conviene moverlas a *user-secrets*
  o variables de entorno para no normalizar el patrón.

## 6. Superficie de ataque adicional

- **Endpoints anónimos** (login, refresh, olvidé/restablecer-contraseña, solicitar/desbloquear-cuenta,
  verificar/reenviar-correo, QR): todos devuelven **respuestas neutras** que no revelan si un correo
  existe → evitan enumeración de usuarios. El login está protegido por el bloqueo.
- **Tokens de reset/desbloqueo:** en memoria, de un solo uso y con caducidad de 1 h. No se persisten
  (no añaden superficie en BD). Al validarse se consumen.
- **Auditoría:** las acciones sensibles quedan en `LogAuditoria` (CU-17): inicios de sesión, cambios
  de estado, creación de reglas, desbloqueo de cuenta, etc.

## 7. Lo que se probó en vivo

- Bloqueo por 5 intentos fallidos y rechazo de la contraseña correcta mientras está bloqueada.
- Desbloqueo por enlace de un solo uso (token consumido tras el primer uso).
- Conexión Firebird en modo solo lectura operativa (40.033 documentos en DCTO).

## 8. Hallazgos y recomendaciones priorizadas

| # | Hallazgo | Severidad | Recomendación |
|---|---|---|---|
| 1 | Sin rate-limit por IP en endpoints anónimos (login/reset/unlock) | Media-Baja | Añadir throttling por IP (p. ej. `AspNetCoreRateLimit`) para evitar bombardeo de correos de reset/desbloqueo. |
| 2 | Identificadores de fuentes dinámicas validados solo por existencia | Baja | Añadir lista blanca de caracteres como segunda barrera. |
| 3 | Credenciales de BD dev en `appsettings.Development.json` | Baja | Mover a user-secrets / variables de entorno. |
| 4 | JWT en `localStorage` (frontend) | Baja | Evaluar cookies `httpOnly`+`SameSite` para mitigar robo por XSS; mantener CSP estricta. |
| 5 | Expresiones de reglas sin límite de tamaño/profundidad | Baja | Acotar longitud y profundidad al compilar. |
| 6 | Desbloqueo anónimo se atribuye a `admin` en el log | Informativa | Registrar como "Sistema" para acciones de autoservicio. |
| 7 | CORS / clave JWT en producción | A confirmar | Verificar CORS restringido al origen del frontend y `Jwt:Key` ≥ 32 bytes desde entorno. |

**Conclusión:** el sistema aplica las prácticas correctas en los puntos de mayor riesgo
(solo lectura en las fuentes, parámetros en las consultas, evaluador sin ejecución de código,
hashing fuerte, JWT revocable, bloqueo, secretos fuera del repo). Las recomendaciones son de
endurecimiento para el despliegue en producción, no correcciones de vulnerabilidades activas.
