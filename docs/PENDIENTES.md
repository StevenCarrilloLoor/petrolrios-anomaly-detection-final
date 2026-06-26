# Backlog / pendientes — PetrolRíos

Lista viva de lo acordado en las sesiones, con estado. Orden = prioridad sugerida.
Última actualización: 25 de junio de 2026 ("Frecuencia del análisis" en Ajustes ahora es un desplegable en español a prueba de errores + cron validado de verdad; antes: agente ContaGober + arranque automático).

## 🛠️ Auditoría agente/reglas San Pío (25-jun)
- [x] **HECHO + gate verde** — **FIX 1 watermark por reloj de Firebird** (`FirebirdExtractor`/`CycleRunner`): la marca avanza con `CURRENT_TIMESTAMP` del servidor Firebird (no `DateTime.UtcNow`), serialización `Unspecified`, re-siembra de marcas viejas en UTC. Destranca los 4 detectores predeterminados en estaciones fuera de UTC. **FIX 2 tolerancia de nombres** en `GetValor` (amigable→crudo: `TotalNeto`→`TNI_DCTO`) + 5 pruebas. (CAMBIOS §65, `docs/DIAGNOSTICO-AGENTE-REGLAS.md`)
- [ ] **VALIDAR EN SAN PÍO (mañana):** copiar `dist/agente-windows` (portable nuevo) a la estación, auto-detectar la base (`C:\Programas\ContaGober1\Datosc\CONTAB.FDB`), **activar el arranque automático** (botón del panel), y confirmar que con FIX 1 la built-in `Factura` fluye al día (el desfase TZ solo se reproduce en estación real UTC-5). La fuente `Dcto` duplicada ya se borró.
- [x] **HECHO + gate verde** — **Agente: auto-detección de ContaGober** (`C:\Programas\ContaGober1\Datosc\CONTAB.FDB` + variantes `CONTAB.FDB` + escaneo de carpetas de instalación) y **arranque automático al encender**: botón en el panel sin admin (`.vbs` oculto en carpeta de Inicio) + servicio de Windows como opción avanzada. **Portable reconstruido (4 plataformas).** (CAMBIOS §70)
- [x] **HECHO + gate verde (FIX 3 guard):** el central **rechaza** registrar en el selector una tabla que ya extrae un built-in (DCTO→Factura, ANUL→Anulacion, …) y el agente **omite** las que quedaron de antes. `FuenteDatosPolicy.TablasBuiltIn` + 11 pruebas. (CAMBIOS §66)
- [x] **HECHO + gate verde + Chrome** — **sección "Datos recibidos"** (logs crudos de agentes, filtros + buscador + filas expandibles); verificada en vivo con 3.241 registros reales de San Pío. (CAMBIOS §67)
- [x] **HECHO + gate verde + Chrome** — **stress-test del creador de reglas** (25+ casos límite, inyección SQL/XSS, 1000 condiciones, fuente arbitraria): el sistema aguanta y escala. **Bug cazado y arreglado:** nombre/desc/expresión más largos que su columna provocaban **500 → ahora 400 limpio** (guards de longitud en `Validar()` + test de regresión `ReglasPersonalizadas_NombreDemasiadoLargo…`). Guard built-in re-verificado en vivo (TURN_DEPO/CRED_CABE/TURN_TARJ→400, DCTO→409). (CAMBIOS §68)
- [x] **HECHO + gate verde** — **"Datos recibidos" muestra el tipo como "Nombre natural (TABLA)"** (p. ej. `Factura (DCTO)`) en la columna y el desplegable: `CatalogoTiposTransaccion` (built-ins + variante `Anulaciones` + `Dcto`→DCTO; configurables vía catálogo) + `DatoRecibidoResponse.TipoNatural/Tabla` + `/tipos` con etiqueta. 15 tests nuevos (Detectors 150). *Falta el screenshot en vivo: la extensión de Chrome se desconectó al relanzar el sistema.* (CAMBIOS §69)


---

## 🔵 Lote del 25-jun (revisión del ingeniero — 6 mejoras, en etapas)
- **D** [x] **HECHO + Chrome** — abrir el detalle desde "Problemas de estación" sin perder la pestaña: la estación expandida + días viven en la URL (`?dias&g`) y el detalle vuelve a su origen con el botón "Volver a problemas de estación". (CAMBIOS §56)
- **F** [x] **HECHO + gate verde** — alertas de reglas personalizadas legibles: descripción de la regla + condición en lenguaje natural en la alerta (la condición técnica queda en la evidencia). (CAMBIOS §56)
- **C** [x] **HECHO + Chrome** — unidad del umbral (horas/$/%/galones/veces/"1=activado") con tooltip + **doble carril** `AmbitoAlerta.Ambos` (el chip cicla Operativa→Auditoría→Ambos; routing en `AnomalyDetectionJob` + `AlertaService`). Sin migración (enum int + unidad derivada). (CAMBIOS §57)
- **A** [x] **HECHO + Chrome** — correo por regla (motor + personalizadas): flag `NotificarCorreo` (migración `NotificarCorreoRegla`) estampado en `RuleBasedDetector`/`CustomRuleDetector` → el job llama `NotificarReglaPorCorreoAsync`; UI campana (motor) + casilla "Avisar por correo" (custom). (CAMBIOS §58)
- **B** [x] **HECHO + Chrome** — Ajustes: conexión BD ya era solo-admin (confirmado); nueva sección **Operación del sistema** (nivel mínimo de correo + cron del job → `config/operacion.json`, re-registra el job en vivo) y **tamaño de letra** Normal/Grande/Mayor para todos. (CAMBIOS §59)
- **E** [x] **HECHO + Chrome** — leído/no leído por usuario: entidad `AlertaVista` (índice único alerta×usuario) + migración `AlertaVistaPorUsuario` + `POST /alertas/{id}/marcar-vista` + `GET /alertas/vistas`; el detalle marca al abrir, la lista muestra punto azul "nueva para ti". (CAMBIOS §60)

> **Lote del 25-jun COMPLETO: A, B, C, D, E, F hechos y verificados en Chrome.**

## 🧪 Pase de QA (25-jun, en curso)
- [x] **Botón "Restablecer predeterminados"** por detector (`POST /reglas/restablecer/{tipo}`, defaults desde `IDetectionRule`) + confirmación en línea. **Bug cazado:** `window.confirm` congelaba el renderer → reemplazado por confirmación in-app. Verificado: 50→999→Restablecer→50. (CAMBIOS §61)
- [x] **HECHO + Chrome E2E** — regla custom "QA - Factura efectivo alto valor" (5 campos a mostrar) + inserción en **Firebird** (DCTO 9900060, $500 EF) → agente EST-001 la envió → alerta #33 con descripción legible, empleado resuelto y los **5 campos** en la evidencia. (CAMBIOS §62)
- [x] **HECHO** — detalle/clasificar (Tomar en Revisión → En Revisión) + comentar verificados; pipeline Firebird→agente→central confirmado. Sin bugs salvo el `window.confirm` (arreglado).

> **Pase de QA del 25-jun COMPLETO. Único bug = `window.confirm` del botón Restablecer, ya arreglado.**

## 🗂️ Reorganización de scripts (25-jun)
- [x] **HECHO + gate verde** — todos los `.bat`/`.ps1`/`.sh` movidos (con `git mv`) a `ejecutables/` en 6 carpetas por tipo, con nombres descriptivos y un **resumen al inicio** de cada uno; obsoletos borrados (`verificar_2fa`, `verificar_ronda_fuentes`, 3 wrappers); rutas/cross-refs arregladas; `.gitignore` cubre `firebird_data/` + `*.FDB`; docs e índice `ejecutables/LEEME.md` actualizados. El gate ahora es `ejecutables/4-VERIFICACION-Y-PRUEBAS/verificar-todo-gate-oficial.bat`. (CAMBIOS §64)

## 🤝 Asignación de alertas "al 1000%" (25-jun)
- [x] **HECHO + Chrome E2E** — asignar ahora **avisa al asignado por correo** y por **SignalR** (evento `AlertaAsignada`), **registra quién asignó** (`AsignadoPorId`, migración `AsignacionAsignadoPor`) y **muestra a quién está/fue asignada** en el **detalle** (banner "Asignada a X (rol) · por Y · fecha" + tarjeta "Reasignar") y en la **lista** (bajo el estado). `AlertaResponse` ampliado; `AsignarAsync` devuelve la alerta y recibe el asignador. 4 pruebas nuevas (`AlertaServiceAsignacionTests`). (CAMBIOS §63)

---

## 🔴 Feedback del 18-jun (revisión en vivo de Steven)
1. [x] **HECHO (commit a63f64b, verificado en Chrome)** — Regla personalizada con ámbito: selector **Operativa/Auditoría** al crear y editar, `ReglaPersonalizada.Ambito` + migración, `CustomRuleDetector` setea `DetectedAnomaly.Ambito`, badge en la lista, tests (Domain + Detector). Probado: regla "PRUEBA Operativa - factura alta" creada con badge OPERATIVA.
2. [x] **HECHO (commit a63f64b, verificado en Chrome)** — La tabla registrada en el central ya aparece en el desplegable "Fuente de datos" del builder (Tanques/TANQ_REPO visible) aunque aún no haya datos en staging. *Falta su documentación de campos → punto 3.*
3. [x] **HECHO Y VERIFICADO**: el agente reportó en vivo **239 tablas** al central; el esquema y las columnas quedan disponibles para fuentes y reglas.
4. [x] **HECHO Y VERIFICADO**: navegador de tablas con búsqueda y auto-documentación en el central. El explorador local se conserva como diagnóstico de respaldo.
5. [ ] **Editor de reglas del motor demasiado simple**: solo cambia umbral. Revisar si debe permitir más (activar/desactivar ya está; evaluar editar descripción u otros parámetros con cuidado de no romper la lógica fija del detector).
6. [ ] **No se probó el agente con datos reales**: insertar datos en Firebird (turno abierto, factura backdated, fila de tabla extra) para ver que el agente extrae y el central genera alertas **Operativa y Auditoría**.
7. [x] **HECHO (19-jun-2026)**: se creó y probó en Chrome el tercer subsistema independiente **PetrolRios.StationMonitor** (`localhost:5190`), de solo lectura. También se recorrieron central y agente; Firebird fue levantado y TANQ_REPO quedó `Sincronizada`.
8. [x] **HECHO**: cobertura para memoria de envío, fuentes, reglas por ámbito, aislamiento por estación, JWT y grupos SignalR. Totales actuales: Domain 34, Detectors 110, API 47, Monitor 2.

## 🟡 Hecho en esta ronda (a verificar/commitear con `scripts/verificar_ronda_fuentes.bat`)
- **Memoria de envío del agente** (`SentMemory`): huella de contenido por registro, persistida en `enviados.huellas`. Corta el bucle de "1 transacción enviada" cada ciclo (el turno abierto `EST_TURN='0'` ya no se reenvía). Si el contenido cambia (turno se cierra) vuelve a enviarse una sola vez. El almacenamiento del central siempre estuvo a salvo por la idempotencia; esto evita el tráfico/ruido inútil.
- **Reglas diferenciadas por carril** en la UI de Reglas: badge **Operativa** vs **Auditoría** (derivado del parámetro en `ReglaService`). Las operativas (turno sin cerrar, despacho no facturado, fuera de horario) quedan visibles y editables como las demás.
- **Buscador de tablas escribible** en el panel del agente: el desplegable de ~200 tablas pasó a un `input + datalist` donde se escribe el nombre y se filtra; describe la tabla al coincidir.
- **Registro CENTRAL de fuentes (tablas extra)** — el cambio grande: entidad `FuenteDatos` + migración `FuentesDatos`, controlador `/api/v1/fuentes-datos` (CRUD admin + `/activas` para agentes), sección **"Fuentes de datos (tablas)"** en Reglas (admin gestiona, demás ven). El agente descarga el catálogo central cada ciclo (`ObtenerFuentesCentralAsync`) y lo combina con sus fuentes locales (central tiene prioridad). Así el ingeniero registra una tabla **una sola vez** y todas las estaciones la reciben; cada agente verifica que la tabla/columna exista en SU base antes de extraer. Test de dominio `FuenteDatosTests`.

## ✅ Hecho y commiteado
- Login del agente opt-in + "re-sincronizar desde fecha" (QoL del panel).
- Publicación multiplataforma del agente (Windows / Linux / macOS) + instaladores de servicio (sc / systemd / launchd) + LEEME por SO.
- Central accesible por red: `REINICIAR_CENTRAL_RED` (bind 0.0.0.0, firewall, arranca Docker/Postgres, perfil "red"). Conexión del compañero por ZeroTier funcionando.
- **Idempotencia de ingesta** (huella SHA-256): el central ya no duplica ni re-alerta un registro reenviado (incluye el fechado al futuro).
- Login QR oculto tras bandera; login móvil cubierto por TOTP.
- Correo de verificación con URL configurable (`App__FrontendUrl`), no atado a localhost.
- **Investigación** del esquema Firebird (`docs/investigacion-deteccion-anomalias.md`) + plan de 6 fases.
- Detector **"fecha fuera de rango plausible"** (backdating) con pruebas.
- **Subsistema de alertas por ámbito (Increment A):** enum `AmbitoAlerta` (Operativa/Auditoría), `Alerta.Ambito` + migración + índices, detectores etiquetan el carril, SignalR enruta operativas al grupo de la estación (`ProblemaEstacion`).

---

## ✅ Subsistema de alertas por estación
- [x] Endpoint de agregación para el central y modo `soloActivos` para el monitor local.
- [x] Pestaña global "Problemas de estación" en el central.
- [x] Asociación usuario ↔ estación editable desde Usuarios.
- [x] Correo de contacto y avisos operativos.
- [x] Aislamiento real por JWT en API, SignalR e ingesta: una cuenta de estación no puede forzar otra estación ni consultar el dashboard/bandeja central.
- [x] Tercer ejecutable **Monitor de estación**: consulta periódica, avisos del navegador, historial local y configuración editable.

## 🔜 Detectores nuevos (patrones del ingeniero + tesis)
Operativos (carril Operativa → estación):
- [x] **Turno sin cerrar** (TURN.EST_TURN abierto / FFI_TURN viejo). *Lo del inge: "se olvidan de cerrar turno".*
- [x] **Despacho mal cerrado / no facturado** (DESP.FAC_DESP / EST_DESP). *"No lo colgó bien".*
- [ ] **Campos faltantes** (ya parcial; ampliar a más campos/condiciones).

De auditoría/fraude (carril Auditoría → central):
- [x] **Cuadre de tanque** mediante TANQ_REPO como fuente configurable + regla por DIFERENCIA.
- [x] **Crédito no autorizado / sin garante** (CRED_CABE: COD_GARA). *Lo del inge: "autorizan créditos a quien no debe".*
- [x] **Cancelar-reingresar recurrente (kiting)** (ANUL + repetición en varios días).
- [ ] **Placa bloqueada / sobre cupo** (PLACA_BLOQ / PLACA_CUPO).
- [ ] **Cuadre forzado** (faltan $X y aparece justo un crédito/ajuste de ~$X pegado al cierre que lleva el faltante a 0; recurrente/escalado). *Requiere estudiar primero cómo funciona el cuadre y el conteo de efectivo para no alertar de más.*

## 🔜 LA IDEA GRANDE — Plataforma de detección configurable (lo que más me recalcaste)
> "que el agente pueda enviar info de múltiples tablas o las que selecciones… mejorar el creador de reglas para que funcione afuera de CONTAC… agregar una tabla nueva, verificar que exista, devolver la documentación automática de sus campos, y crear reglas sin tocar código ni llamar a un ingeniero."

- [x] **Fuentes de extracción configurables (multi-tabla)** con catálogo central y respaldo local.
- [x] **Registro dinámico de tablas con verificación** contra el esquema reportado por el agente.
- [x] **Auto-documentación de la tabla** desde `RDB$RELATION_FIELDS` + `RDB$FIELDS`.
- [x] **Creador de reglas genérico (fuera de CONTAC)** básico y avanzado.
- [x] **Seguridad/gobernanza de la plataforma:** solo lectura, nombres validados, evaluador acotado, RBAC y auditoría.
- [ ] **Investigación tabla-por-tabla exhaustiva:** profundizar el catálogo (hoy revisé las ~15 tablas de más valor; faltan el resto de las ~200 para no dejar señales útiles fuera).

## 🔜 Centralizar fuentes — mejoras pendientes (la base ya está hecha esta ronda)
- [x] **Selector de tabla en el central:** el central solicita/reutiliza el esquema reportado por un agente y ofrece buscador + columnas.
- [x] **Verificación por estación:** la matriz de sincronización informa tabla inexistente, cursor inválido, versión aplicada, filas y error.
- [ ] **Migrar fuentes locales existentes** de algún agente al catálogo central (utilidad de una sola vez), si llegan a usarse.

## 🔜 Watermark robusto (cerrar puntos ciegos)
- [ ] **Watermark por ID monotónico** (generadores GEN_*_ID) + ventana de solapamiento, para que registros viejos/backdated o con reloj desfasado no se salten.
- [x] **Memoria de envío en el agente** — HECHO esta ronda (`SentMemory`): ya no reenvía el mismo registro cada ciclo.

## 🔜 Empaquetado plug-and-play + limpieza
- [x] **Purgar/arreglar los .bat** obsoletos o redundantes.
- [x] **Setup/.exe plug-and-play** del central, agente y monitor, con instaladores independientes.

## 🔜 Despliegue en la nube (solo explicar por ahora, no implementar)
- [ ] **Decidir Azure vs AWS** (la tesis dice AWS RDS; tú mencionaste Azure). Recomendación: por créditos gratis (Azure for Students si hay correo .edu).
- [ ] **Guía de procedimiento real:** crear el PostgreSQL gestionado, SSL obligatorio, reglas de firewall/IP, connection string por variables de entorno, correr migraciones, y dónde corre el central. (Los agentes solo hablan con el central; solo el central habla con la nube.)

## 🔜 Otros
- [ ] **Pruebas de seguridad sin huecos** (revisión de seguridad pendiente de antes).

---

## Actualización (junio 2026) — avances

**Hecho desde la lista original:**
- ✅ Plataforma flexible completa (explorador de tablas, fuentes configurables multi-tabla,
  motor de reglas genérico, y builder que auto-descubre fuentes/campos).
- ✅ Detectores del ingeniero: turno sin cerrar, crédito sin garante, despacho no facturado,
  kiting (anulaciones recurrentes). Cuadre de tanque → vía fuente configurable + regla.
- ✅ Subsistema de ámbito (Operativa/Auditoría) + pestaña "Problemas de estación" + usuario↔estación
  + correo de contacto.
- ✅ Detector de fecha fuera de rango (backdating) + idempotencia de ingesta.
- ✅ **Limpieza de scripts .bat** (purga de commit_*/scratch/verify redundantes; queda solo lo reutilizable).
- ✅ **Guía de despliegue en la nube** (`docs/DESPLIEGUE-NUBE.md`, Azure/AWS).
- ✅ Cobertura de pruebas para todo lo nuevo (Domain 16, Detectors 108, Api 29).

**Pendiente real después de la verificación del 19-jun:**
- [ ] Watermark por ID monotónico + ventana de solapamiento (punto ciego de fecha).
- [ ] Ejecutar una prueba física en otra PC de estación con el instalador publicado y la red real.
- [ ] Despliegue productivo con TLS/HTTPS y secretos definitivos.

---

## Nuevos pedidos (junio 2026)

- [x] **Central — apartado de configuración de conexiones (solo Admin):** sección para gestionar y
  configurar las conexiones/estaciones (nombre, zona, horario, correo de contacto, activa) desde la
  interfaz; restringido a Administrador.
- [x] **Central — Monitoreo: usuarios conectados:** sección que muestre los usuarios actualmente
  conectados al sistema central (vía SignalR: rastrear conexiones del hub y exponerlas).
- [x] **Agente — autodetección de Firebird:** botón junto a la conexión Firebird que busque
  automáticamente la base CONTAC.FDB (host/puerto/ruta comunes) sin escribir la dirección a mano.
  (Nombre propuesto: "Detectar Firebird automáticamente".)
- [x] **Pruebas rigurosas en Chrome (19-jun-2026):** central, formulario usuario↔estación,
  problemas globales, Station Agent, fuente dinámica sincronizada y Monitor de estación con
  actualización automática al insertar una alerta nueva.

---

## Actualización (24 de junio de 2026) — preparación de producción

**Hecho y commiteado:**
- [x] **Nombre del empleado en las alertas** (no solo el código): catálogo central `Empleado`
  sincronizado por el agente desde Firebird (`VEND`/`EMPL`), `IEmpleadoDirectorio` resuelve
  `(estación, código) → nombre` en alertas, dashboard y reportes, sin tocar las 25 reglas.
  Datos demo coheridos (`VEND` con códigos reales de 3 caracteres). *(CAMBIOS §43–44)*
- [x] **Escalabilidad real >10 estaciones:** alta de estación + usuario-agente de **dos formas**
  (botón "Nueva estación" con modal de credenciales, y código de estación nuevo desde "Nuevo
  Usuario"). El sistema ya no está amarrado a 10 estaciones fijas. *(CAMBIOS §45)*
- [x] **Rol "Agente" propio (seguridad):** los agentes dejaron de usar el rol Auditor; nuevo rol
  `Agente` sin acceso a la app central (política "Central" lo excluye). Repunte idempotente de los
  `agent-*` existentes (Auditor→Agente). *(CAMBIOS §45)*
- [x] **Robustez del agente y del gate:** contador de enviadas cuenta los reenvíos del
  store-and-forward; guardado del panel endurecido (`/api/fuentes`, errores visibles);
  `verificar-mejoras.bat` detiene servicios, asegura `dotnet-ef` y muestra progreso en vivo.
  *(CAMBIOS §46)*
- [x] **Validación de contraseña del agente (≥6) al crear estación** (frontend + backend) — antes
  dejaba crear un agente con clave que el login luego rechazaba. Prueba E2E **EST-777** en Chrome:
  rechazo de `1234`, creación con `123456`, agente conectado (Autenticado 182 ms, 2 transacciones
  enviadas, fuentes Sincronizada). *(CAMBIOS §47)*
- [x] **Dist del agente republicado** (v2.3.0) desde el código actual, listo para instalar por estación.
- [x] **Fix de producción CET / Shadow Stack:** el `.exe` no arrancaba en el servidor (assert
  `AreShadowStacksEnabled`, threads.cpp) por CPU con CET + Windows con protección de pila por hardware.
  Solución: `<CETCompat>false</CETCompat>` en los 3 proyectos exe (Agent, Monitor, Api) + republicar.
  *(CAMBIOS §49)*
- [x] **Conectividad VPN documentada** (`docs/CONECTIVIDAD-VPN.md`): diagnóstico (subredes distintas +
  NAT + red POS hostil), por qué falla ZeroTier (REQUESTING_CONFIGURATION), solución recomendada
  **Tailscale** (relay 443) y alternativas. Pendiente real: levantar la VPN en el servidor de la estación.

---

## Mejora del creador de reglas (pedido del ingeniero, 24-jun) — EN CURSO

Hacer el motor de reglas usable por gente no técnica, escalable y procedural. Dos partes:

**Parte 1 — Documentación automática de campos: [x] HECHA Y COMMITEADA (CAMBIOS §50, commit `353740c`).**
Cada campo se muestra con ícono + nombre legible + descripción (glosario Contaplus + inferencia por
prefijo + tipo del esquema). Verificado: build 0/0, tests verde, frontend build OK. *(Falta verlo en
Chrome en vivo — se hará junto al E2E de la Parte 2.)*

**Parte 2 — Juntar tablas + enriquecer la alerta: [x] HECHA (CAMBIOS §51, commits `74b84f9` backend +
`cd4e3df` frontend).** Entidad `RelacionTabla` + migración `EnriquecimientoReglasYRelaciones` + seed
(Despacho→Factura) + `CamposMostrarJson` en la regla + enriquecimiento en `CustomRuleDetector` +
`/catalogo` con campos relacionados + CRUD `/api/v1/relaciones-tabla` + selector "Información a mostrar
en la alerta" en el builder. Verificado: build+tests verde, migración sin cambios pendientes, y **en
vivo** (la migración se aplicó al arrancar, la relación se sembró, y el selector muestra los campos
relacionados — 🚗 Placa, Código de vendedor, Código de cliente, etc. — de "Factura del despacho").
Falta opcional: pantalla de **Admin** para CRUD de relaciones desde la UI (la API ya existe) y un E2E
de inserción real (despacho+factura en Firebird) para ver la alerta ya enriquecida.

**Autodescubrimiento de relaciones: [x] HECHO (CAMBIOS §53, commit `4135873`).** `ConceptosRelacion` +
`DescubridorRelacionesService`: empareja llaves compartidas (concepto + nombre) y valida por solapamiento
de valores en staging; corre al arrancar el central y por `POST /api/v1/relaciones-tabla/descubrir`;
relaciones marcadas `EsAutomatica`; migración `RelacionAutomatica`. **Verificado en vivo:** al arrancar
creó las relaciones solo y "Cierres de turno" ahora ofrece campos relacionados de Factura y TarjetaTurno
(por turno) sin definir nada a mano. Opcional: botón "Descubrir" en la UI (hoy corre al arrancar / por API).

Diseño original (referencia):
1. **Entidad `RelacionTabla`** (Domain) + EF config + DbSet + **migración** + seed de relaciones clave
   (Despacho→Factura por cliente/manguera; Factura→Cliente por COD_CLIE; Factura→Vendedor por COD_VEND…).
   Patrón a copiar: `FuenteDatos` (BaseEntity, private setters, `Create`/`Actualizar`).
2. **`ReglaPersonalizada`**: nueva columna `CamposMostrarJson` (lista de campos propios+relacionados a
   exponer en la alerta) + **migración**.
3. **`CustomRuleDetector`**: tras hacer match, resolver las relaciones (el `DetectionContext` ya tiene
   TODAS las tablas en memoria — ver `PlacaGenericaRule` que une `context.Facturas`+`context.Detalles`)
   y agregar los campos relacionados + seleccionados a `DetectedAnomaly.Metadata` (la "Evidencia").
4. **API**: CRUD de relaciones (`/relaciones-tabla`, solo Admin) + el `/catalogo` ofrece los campos
   relacionados por fuente; `GuardarReglaPersonalizadaRequest` acepta `CamposMostrar`.
5. **Frontend**: en el builder, campos relacionados (para condiciones) + selector "Información a mostrar
   en la alerta"; pantalla de administración de relaciones (Admin).
6. **E2E en Chrome**: regla sobre Despachos que muestre placa+vendedor+cliente en la alerta; insertar en
   Firebird (no Postgres) y confirmar la alerta enriquecida. Commit + CAMBIOS §51.

**Pendiente real (sin cambios):**
- [ ] Watermark por ID monotónico + ventana de solapamiento (punto ciego de fecha).
- [ ] Prueba física en otra PC de estación con el instalador publicado y la red real.
- [ ] Despliegue productivo con TLS/HTTPS y secretos definitivos.
- [ ] Republicar dist del **Servidor** y **Monitor** antes del go-live (`ejecutables\4-PUBLICACION\publicar.bat`).
- [ ] Revisión de seguridad final sin huecos.
