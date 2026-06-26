# Prompt de reinicio de contexto — Proyecto PetrolRíos

> **Cómo usarlo:** cada vez que la conversación se compacte (o empieces una nueva),
> pégame este documento completo como primer mensaje. Reconstruye mis instructivos,
> el flujo de trabajo y el contexto del código para que no perdamos el hilo.

---

## 1. Quién eres aquí y cómo trabajamos (mi mandato)

Eres mi compañero de ingeniería en mi **proyecto de tesis** PetrolRíos. Trabajamos así, sin excepción:

- **No espero menos que la perfección.** Calidad de tesis: código limpio, probado y verificado.
- **Trabaja en etapas.** Después de **cada cambio grande**:
  1. Compruébalo en el **código** (compila + corre las pruebas; añade pruebas nuevas para lo nuevo).
  2. Compruébalo en la **interfaz** usando **Chrome** (prueba botones, flujos, inserciones reales).
  3. Haz un **commit** con descripción detallada.
- **Hazlo todo seguido**, en secuencia, **sin preguntarme si avanzas** entre etapas. Solo
  detente a preguntarme cuando yo lo pedí explícitamente (p. ej. antes de un refactor grande).
- **Agrupa cambios pequeños** para no repetir el ciclo de verificación por cada uno.
- Si encuentras **bugs**, arréglalos.
- **Actualiza SIEMPRE la documentación de contexto legado con CADA cambio** (no solo al final):
  `CAMBIOS.md` (bitácora con sección numerada + commits + verificación), `docs/PENDIENTES.md`
  (marcar hecho / agregar lo nuevo + fecha) y este mismo `docs/PROMPT-REINICIO-CONTEXTO.md`
  (estado actual, sección 6). Así, si el contexto se compacta, el siguiente arranque no pierde el hilo.

## 2. Primero, reconstruye el contexto (ANTES de cualquier cambio)

1. **Pide todos los permisos** que necesites (Chrome, control de la computadora, acceso a la carpeta).
2. Lee **`CLAUDE.md`** (reglas del repo; stack obligatorio; tablas reales; lo que NO debes hacer).
3. Lee **`CAMBIOS.md`** completo (bitácora de todo lo que ya hicimos, etapa por etapa).
4. Lee **`docs/contac-schema.sql`** = **fuente de verdad** de los nombres reales de tablas/columnas
   Firebird (Contaplus). El documento transaccional real es **`DCTO`** (no `FACT`). Verifica
   siempre las columnas ahí antes de escribir SQL o lógica de detector.
5. **`docs/tesis.md` está DESACTUALIZADA**: úsala solo como referencia, **NO la edites**. El
   **CÓDIGO es la fuente de verdad**, mi proyecto está mucho más avanzado que la tesis.
6. Revisa el historial: `git log --oneline -40`.
7. Lee el **código relevante a la tarea, archivo por archivo**, sin inventar ni asumir nombres.

## 3. Reglas de entorno y flujo de trabajo (críticas — si las ignoras, fallarás)

- **El sandbox Linux NO compila .NET**, y su **montaje sirve lecturas viejas/truncadas**. Para el
  estado real usa **SIEMPRE** las herramientas `Read`/`Write`/`Edit` (no `cat` del sandbox) y
  compila/pruebas/commits en **Windows**.
- **Build / tests / commits en Windows:** escribe un `.bat` y ejecútalo desde la **barra de
  direcciones del Explorador de archivos** (clic en la barra → escribe la ruta del `.bat` → Enter);
  luego **lee el `.log`** de salida con la herramienta `Read`. (Si el clic no entra en modo edición,
  clic en una zona vacía a la derecha de la barra y reintenta.)
- **Gate de verificación oficial:** `ejecutables/4-VERIFICACION-Y-PRUEBAS/verificar-todo-gate-oficial.bat`
  (build Release + todos los tests + chequeo de migración EF + lint frontend + build frontend). Úsalo antes de commitear.
  *(TODOS los scripts viven ahora bajo `ejecutables/`, ordenados por tipo y con un resumen al inicio; ver `ejecutables/LEEME.md`.)*
- **ANTES de compilar, DETÉN los servicios en ejecución** (bloquean los binarios y el build falla
  con `MSB3027`):
  `taskkill /F /IM PetrolRios.Api.exe` · `PetrolRios.StationAgent.exe` · `PetrolRios.StationMonitor.exe`.
- **Genera migraciones EF solo con build fresco** (nunca `--no-build` sobre binarios bloqueados/viejos:
  salen **vacías** y rompen el arranque por desajuste modelo↔esquema). El nombre del archivo de
  migración lleva timestamp nuevo: úsalo exacto al hacer `git add`.
- **Reinicia todo el sistema** con `ejecutables/1-INICIAR-Y-DETENER/iniciar-todo-el-sistema.bat` (Docker, PostgreSQL,
  Firebird, API, agente, monitor, frontend). Para solo el API: `ejecutables/1-INICIAR-Y-DETENER/reiniciar-solo-la-api.bat`.
- **Commits con mi identidad:**
  `git -c user.name="StevenCarrilloLoor" -c user.email="stevencarrilloloor@gmail.com" commit`.
  Haz `git add` **por ruta, solo de tus archivos**. **NUNCA** toques mis cambios sin commitear.
- **Limpia** los `.bat`/`.log` temporales que crees.
- Detalle conocido: los screenshots por CDP a veces dan timeout → **reintenta** (sale al 2º intento).

## 4. El sistema (qué es)

PetrolRíos detecta **anomalías transaccionales** en ~13–15 mil transacciones diarias de 10 estaciones
de servicio (cada una con un Firebird `CONTAC.FDB` en **solo lectura**). Tres aplicaciones:

- **Central** = API ASP.NET Core 9 en `:5170` + frontend React/Vite en `:5173` + PostgreSQL.
  Hangfire corre los detectores **cada minuto** en dev. SignalR para alertas en vivo.
- **Station Agent** (`:5180`): extrae de Firebird por marca de agua y envía al central; panel propio.
- **Station Monitor** (`:5190`): vista operativa de solo lectura por estación.

**5 detectores** (Strategy Pattern): `CashFraud`, `InvoiceAnomaly`, `PaymentFraud`,
`ComplianceViolation`, `CustomRule`. **Dos carriles de alerta** (enum `AmbitoAlerta`):
**Operativa** (error de estación → avisa al administrador, pestaña "Problemas de estación") y
**Auditoría** (fraude → bandeja del central). El carril de cada regla del motor **ya es editable**
(propiedad `Ambito` en `ReglaDeteccion`; los detectores la respetan).

**Roles (RBAC):** `Administrador`, `Supervisor`, `Auditor` (personas que usan la app central) y
**`Agente`** (cuenta de servicio de cada estación — usuarios `agent-*` que SOLO envían datos; la
política "Central" los excluye, no pueden entrar al dashboard/bandeja). **Escalable a >10 estaciones:**
el Administrador da de alta una estación + su usuario-agente desde "Nueva estación" (modal con
credenciales) o desde "Nuevo Usuario" (código de estación nuevo). El agente corre v2.3.0.

## 5. Mis credenciales y los permisos que te doy

- **admin@petrolrios.com / Oportunidad1234** (Administrador).
- **stevencarrilloloor@gmail.com / Oportunidad1234** (mi cuenta).
- Te **autorizo** a usar mi computadora, Chrome y a **ver mi Gmail abierto** para pruebas E2E
  (p. ej. leer los correos de recuperación/desbloqueo).
- Recuerda tus reglas de seguridad: **no escribes contraseñas en campos** → haz login por API
  (`POST /api/v1/auth/login` y guarda el token en `localStorage`).

## 6. Estado actual del trabajo (ACTUALÍZAME al avanzar)

**Última ronda — Agente: ContaGober + arranque automático + portable nuevo (25-jun-2026):**
- **Auto-detección Firebird ampliada** (`FirebirdExtractor.RutasCandidatas`): añadidas rutas de ContaGober
  (`C:\Programas\ContaGober1\Datosc\CONTAB.FDB`) y variantes `CONTAB.FDB`; además **escanea** raíces de
  instalación (`C:\Programas`, Program Files, `C:\Conta`, `C:\` superficial) por `CONTA*.FDB`
  (IgnoreInaccessible, profundidad limitada).
- **Arranque automático al encender (las dos vías):** (a) botón en el panel **sin admin** —
  `InicioAutomatico` + `GET/POST /api/inicio-automatico` escribe un `.vbs` oculto en la carpeta de Inicio;
  (b) **servicio de Windows** (admin) con el `instalar_agente_servicio.bat` que ya viaja en el portable.
  Documentado en el panel (desplegable) y en `agente-LEEME-windows.txt`.
- **Portable reconstruido (4 plataformas)** con TODO lo acumulado (watermark TZ, nombres naturales,
  ContaGober, arranque automático) vía `publicar-solo-el-agente-multiplataforma.bat` → `dist/`.
- **Conteos:** Domain 40, Detectors 150, Monitor 2, Api 74 (gate verde con Docker). *(CAMBIOS §70)*
- **Pendiente San Pío (mañana):** copiar `dist/agente-windows` a la estación, configurar (auto-detectar la
  base), activar el arranque automático y validar que la built-in `Factura` fluye al día (fix de watermark).

**Ronda — "Datos recibidos" con nombre natural + tabla técnica (25-jun-2026):**
- En los logs el tipo se muestra como **"Nombre natural (TABLA)"** — `Factura (DCTO)`, `Anulación (ANUL)` —
  en la columna y en el desplegable del filtro. Backend: `CatalogoTiposTransaccion` (Application/Fuentes)
  resuelve tipo→(natural, tabla) para los 7 built-ins (+ variante `Anulaciones` y `Dcto`→DCTO), y para las
  fuentes configurables toma la tabla del catálogo (`FuentesDatos`). `DatoRecibidoResponse` ganó
  `TipoNatural`+`Tabla`; `GET /datos-recibidos/tipos` devuelve `{ tipo, etiqueta }`. 15 tests nuevos.
- Steven **borró la fuente `Dcto` duplicada** del selector (era el mismo DCTO que el built-in `Factura`).
- **Conteos:** Domain 40, **Detectors 150**, Monitor 2, Api 74. *Pendiente: screenshot en vivo (la
  extensión de Chrome se desconectó al relanzar el sistema; el gate y los 15 unit tests ya validan la lógica).*
  *(CAMBIOS §69)*

**Ronda — Robustez del creador de reglas + Logs + guard (25-jun-2026):**
- **Sección "Datos recibidos"** (logs crudos de agentes: tabla con tipo/estación/fecha/estado + **filas
  expandibles** con el JSON crudo, **filtros** tipo/estación/estado y **buscador**): `DatosRecibidosController`
  (`GET /api/v1/datos-recibidos` + `/tipos`) y página en Monitoreo. Verificada en vivo: 3.241 registros
  reales de San Pío (`DCTO` con COD_PAGO/COD_VEND/COD_CLIE reales). *(CAMBIOS §67, commit `d4b6fc4`)*
- **Guard anti-duplicación** del selector (no registrar tablas que ya extrae un built-in:
  DCTO→Factura, ANUL→Anulacion, …): `FuenteDatosPolicy.TablasBuiltIn` + 11 tests. Verificado en vivo:
  TURN_DEPO/CRED_CABE/TURN_TARJ→**400** ("ya se procesa como …"), DCTO/`dcto`→**409** ("ya registrada").
  *(CAMBIOS §66, commit `6f2d559`)*
- **Stress-test en Chrome del creador de reglas** (25+ casos: inyección SQL/XSS inertes — la tabla
  sobrevive y EF parametriza; 1000 condiciones y fuente configurable arbitraria → 201; aguanta y escala).
  **Bug cazado y arreglado:** un nombre/descripción/expresión más largo que su columna provocaba **HTTP 500**
  al guardar → ahora **400 limpio** (guards de longitud en `ReglasPersonalizadasController.Validar()`,
  espejo de la BD: Nombre 150 / Descripción 500 / Fuente 50 / Expresión 2000). Test de regresión
  `ReglasPersonalizadas_NombreDemasiadoLargo_DevuelveBadRequestNo500`. *(CAMBIOS §68)*
- **Autounidor `DescubridorRelacionesService`:** revisado (similitud de nombre + solape de valores en
  staging, umbrales prudentes) — bien implementado y escalable; sin cambios.
- **Conteos tras esta ronda:** Domain 40, Detectors 135, Monitor 2, **Api 74** (con Docker; +1 regresión).
  Agente v2.3.0.
- **Pendiente San Pío (mañana):** republicar el agente con FIX 1 (watermark por reloj de Firebird),
  validar que la built-in `Factura` fluye al día y la regla nueva dispara; luego **quitar del selector la
  fuente `Dcto` duplicada** (el agente ya la omite, pero conviene limpiar el catálogo).

**Ronda — Auditoría agente/reglas San Pío (25-jun-2026), commiteado:**
- Síntoma: en San Pío (estación real UTC-5) el agente envía datos (vía fuente configurable `Dcto`) pero la
  regla nueva no disparaba y los 4 detectores predeterminados se quedaban sin datos nuevos.
- **FIX 1 (watermark por reloj de Firebird):** el extractor built-in avanzaba la marca con `DateTime.UtcNow`
  pero `FEC_DCTO` es hora local → en UTC-5 saltaba 5 h y congelaba `Factura`/`DetalleFactura`/etc. Ahora la
  marca usa `CURRENT_TIMESTAMP` de Firebird (`ResultadoExtraccionAgente.RelojFirebird`), `Unspecified` +
  `RoundtripKind`, re-siembra de marcas viejas en UTC. (`FirebirdExtractor.cs`, `CycleRunner.cs`)
- **FIX 2 (tolerancia de nombres en reglas):** `CatalogoReglasPersonalizadas.GetValor` resuelve fuentes
  configurables con exacto → sin mayúsculas/espacios → puente amigable→crudo (`TotalNeto`→`TNI_DCTO`). +5 tests.
- Diagnóstico completo en `docs/DIAGNOSTICO-AGENTE-REGLAS.md`. Gate verde (Detectors **124**, +5). **Falta
  validar en San Pío mañana** (el desfase TZ solo aparece en estación real; el demo es UTC). *(CAMBIOS §65)*



**Última ronda — Reorganización de scripts (25-jun-2026), commiteado:**
- **TODOS** los scripts (`.bat`/`.ps1`/`.sh`) viven ahora bajo `ejecutables/` en 6 carpetas por tipo
  (`1-INICIAR-Y-DETENER`, `2-BASE-DE-DATOS-Y-DEMO`, `3-DIAGNOSTICO`, `4-VERIFICACION-Y-PRUEBAS`,
  `5-PUBLICACION-Y-DESPLIEGUE`, `6-INSTALAR-EN-NUEVO-PC`), con **nombres descriptivos** y un **bloque
  RESUMEN** al inicio de cada uno. Se vaciaron `scripts/`, `_arranque/` e `INSTALACION/`. Se borraron
  obsoletos (`verificar_2fa`, `verificar_ronda_fuentes` y 3 wrappers). `dist/` y `firebird_data/`
  quedan ignorados (build/datos). El **gate** ahora es
  `ejecutables/4-VERIFICACION-Y-PRUEBAS/verificar-todo-gate-oficial.bat`. Índice en `ejecutables/LEEME.md`. *(CAMBIOS §64)*

**Última ronda — Asignación de alertas "al 1000%" (25-jun-2026), commiteado:**
- Asignar una alerta ahora **avisa al asignado por correo** (`IEmailNotificacionService`, dirigido solo a
  él) y por **SignalR** (evento nuevo `AlertaAsignada` → toast personalizado "Te asignaron una alerta" en
  `NotificationProvider`), **registra quién la asignó** (`AsignacionAlerta.AsignadoPorId`, migración
  `AsignacionAsignadoPor`) y **muestra a quién está/fue asignada**: en el **detalle** un banner
  "Asignada a X (rol) · asignada por Y · fecha" + la tarjeta cambia a **"Reasignar"**, y en la **lista**
  el responsable bajo el estado. `AlertaResponse` ganó 5 campos de asignación; `AsignarAsync` recibe el
  asignador (claim → `ClaimsPrincipal.GetUsuarioId()`) y **devuelve 200** con la alerta actualizada.
  4 pruebas nuevas (`AlertaServiceAsignacionTests`). Verificado en Chrome (asigné #32 a Maria Fernanda
  Auditora; #33 histórica ya muestra su responsable). *(CAMBIOS §63)*
- **Conteos de pruebas tras esta ronda:** Domain 40, Detectors 119, Monitor 2, **Api 73** (con Docker
  arriba corren las de integración; +4 de asignación). Agente v2.3.0.

**Ronda previa — preparación de producción (24-jun-2026), commiteado:**
- **Nombre del empleado en las alertas** (no solo el código): catálogo central `Empleado` que el
  agente sincroniza desde Firebird (`VEND`/`EMPL`); `IEmpleadoDirectorio` resuelve `(estación,código)→
  nombre` en alertas, dashboard y reportes, sin tocar las 25 reglas. *(CAMBIOS §43–44)*
- **Escalabilidad >10 estaciones + rol `Agente`** propio (seguridad): alta de estación + usuario-agente
  desde "Nueva estación" (modal con credenciales) y desde "Nuevo Usuario" (código nuevo). Los `agent-*`
  ya NO son Auditor; la política "Central" excluye al rol Agente. *(CAMBIOS §45, commits `2d3d12a`,`bf8782a`)*
- **Robustez del agente y del gate:** el contador de enviadas cuenta los reenvíos; guardado del panel
  endurecido; `verificar-mejoras.bat` detiene servicios, asegura `dotnet-ef` y muestra progreso en vivo.
  *(CAMBIOS §46, commits `a50a79f`,`e4146c7`)*
- **Validación de contraseña del agente (≥6)** al crear estación (front+back). Prueba E2E **EST-777**
  en Chrome: rechazo de `1234`, creación con `123456`, agente conectado (182 ms, 2 enviadas,
  Sincronizada). *(CAMBIOS §47, commit `c66fb56`)*
- **Fix de producción CET/Shadow Stack:** el `.exe` abortaba en el servidor (assert
  `AreShadowStacksEnabled`, threads.cpp) en CPUs con CET + Windows con protección de pila por hardware.
  Solución: **`<CETCompat>false</CETCompat>`** en los 3 proyectos `.exe` (StationAgent, StationMonitor,
  Api) y republicar. Workaround sin recompilar: `Set-ProcessMitigation -Name <exe> -Disable UserShadowStack`.
  *(CAMBIOS §49)*
- **Conectividad/VPN:** `docs/CONECTIVIDAD-VPN.md` (subredes distintas + NAT en la red POS; ZeroTier
  queda en REQUESTING_CONFIGURATION; recomendado **Tailscale** por su relay sobre 443). Pendiente:
  levantar la VPN real en la estación.
- **Creador de reglas más usable (pedido del ingeniero), 2 partes:**
  - **Parte 1 (HECHA, commit `353740c`, CAMBIOS §50):** documentación automática de campos — el builder
    muestra ícono + nombre legible + descripción (`DiccionarioCamposContaplus`: glosario + inferencia por
    prefijo + tipo del esquema) en vez de códigos crudos.
  - **Parte 2 (HECHA, commits `74b84f9` backend + `cd4e3df` frontend, CAMBIOS §51):** juntar tablas +
    elegir qué campos relacionados se muestran en la alerta (placa/cliente/vendedor/factura). Entidad
    `RelacionTabla` + migración `EnriquecimientoReglasYRelaciones` + seed Despacho→Factura +
    `CamposMostrarJson` + enriquecimiento en `CustomRuleDetector` (cruza en memoria) + `/catalogo` con
    campos relacionados + CRUD `/api/v1/relaciones-tabla` + selector en el builder. Verificado en vivo
    (migración aplicada, relación sembrada, selector muestra 🚗 Placa / vendedor / cliente de "Factura
    del despacho").
  - **Pulido (CAMBIOS §52, commit `fe725a7`):** tooltip con el código técnico del campo + filtro por
    tipo en el modo avanzado + glosario ampliado (sin inventar códigos opacos de Contaplus).
  - **Autodescubridor de relaciones (CAMBIOS §53, commit `4135873`):** `ConceptosRelacion` +
    `DescubridorRelacionesService` emparejan llaves compartidas (concepto + nombre) y validan por
    solapamiento de valores en staging; corre al arrancar el central y por `POST /relaciones-tabla/descubrir`;
    flag `EsAutomatica` + migración `RelacionAutomatica`. Verificado en vivo: crea las relaciones solo.
    Opcional pendiente: pantalla Admin de relaciones y botón "Descubrir" en la UI.
- **Conteos de pruebas actuales:** Domain 40, Detectors 119, Monitor 2, Api 53 (+16 de integración
  saltadas sin Docker). Cobertura de `PetrolRios.Detectors` ≈ 96% líneas (OE5). Agente v2.3.0.
- **Pendiente para go-live:** republicar dist del Servidor y Monitor; prueba física en otra PC con la
  red real; TLS/HTTPS + secretos definitivos; watermark por ID monotónico. (Ver `docs/PENDIENTES.md`.)

**Hecho y commiteado (rondas previas):**
- 5 etapas previas: arreglo de login/correo/verificación; sacar alertas operativas de la bandeja de
  auditoría; separar Reglas y Fuentes de datos + rediseño; página de Ajustes (QOL); rediseño del Monitor.
- Función de **desbloqueo de cuenta** por autoservicio desde el login (con demo en vivo end-to-end).
- **Revisión E2E** de los 3 paneles con Chrome: sin bugs funcionales.
- **Informe de seguridad**: `docs/ANALISIS-SEGURIDAD.md` (sin vulnerabilidades críticas; 7
  recomendaciones de endurecimiento).
- **Etapa 1a de reglas** (`a2080c2`): carril **Operativa/Auditoría editable** por regla
  (entidad + migración + los 4 detectores lo leen); clasificación correcta sembrada; **recalibración**
  de *Tasa de anulaciones* 5%→3%.
- **Etapa 2** (`4be0564`): **rediseño de la UI de Reglas** — buscador, grupos colapsables, filas
  compactas y **carril editable con un clic** (chip).
- **Etapa 3** (`89b0be5`): **tablas estándar** visibles en Fuentes (central) y en el panel del agente
  (DCTO, DESP, TURN, TURN_DEPO, ANUL, CRED_CABE, TURN_TARJ); contador **Leídas/Enviadas acumulativo**.
- **Etapa 1b** (`1a0713c`): **2 reglas nuevas** (ComplianceViolation, Auditoría): *Venta sin
  identificación del cliente* (RUC/cédula, SRI) y *Despacho de alto volumen sin placa* (desvío, ARCERNNR).
- **Etapa 4 / limpieza** (`a4334c7`): borrados scripts scratch (`_z`, `_c`, `_diag`) y arreglado el
  PID fijo de `reiniciar_api.bat`.
- Verificación: suites en verde por etapa (hasta **206 pruebas**), lint + frontend build OK.

**Pendiente / opcional (lo demás ya está hecho):**
- **Deduplicar `SeedData`** (cosmético): consolidar las definiciones de reglas en una sola fuente
  (`SeedReglasDeteccionAsync` y `EnsureReglasNuevasAsync` repiten ~6 reglas). El código funciona;
  hacerlo en un pase enfocado y verificar que las 25 reglas (sus parámetros) sigan presentes vía la API.
- **Code-splitting del frontend** (bundle ~970 KB): `manualChunks` para separar vendor.
- **Endurecimiento de seguridad** (`docs/ANALISIS-SEGURIDAD.md`): rate-limit por IP en endpoints
  anónimos, lista blanca de identificadores en fuentes dinámicas, etc.
- Reglas adicionales que quedaron descartadas por ahora por ruido/datos: cupo mensual de galones por
  placa (requiere agregación histórica, no per-ciclo) y saltos de secuencia de facturación (falsos
  positivos entre lotes).

## 7. Recordatorio final

Hazlo todo seguido, en etapas, verificando (código + Chrome + pruebas) y commiteando cada una con mi
identidad, sin tocar mis cambios sin guardar. **No espero menos que la perfección.**
pero primero lee todos los scrips, .bats, archivos y cualquier cosa que este en mi proyecto de todas las carpetas para que armes un contexto completo y sepas que hacer cada cosa nates de que continues