# Backlog / pendientes — PetrolRíos

Lista viva de lo acordado en las sesiones, con estado. Orden = prioridad sugerida.
Última actualización: junio 2026.

---

## 🔴 Feedback del 18-jun (revisión en vivo de Steven)
1. [x] **HECHO (commit a63f64b, verificado en Chrome)** — Regla personalizada con ámbito: selector **Operativa/Auditoría** al crear y editar, `ReglaPersonalizada.Ambito` + migración, `CustomRuleDetector` setea `DetectedAnomaly.Ambito`, badge en la lista, tests (Domain + Detector). Probado: regla "PRUEBA Operativa - factura alta" creada con badge OPERATIVA.
2. [x] **HECHO (commit a63f64b, verificado en Chrome)** — La tabla registrada en el central ya aparece en el desplegable "Fuente de datos" del builder (Tanques/TANQ_REPO visible) aunque aún no haya datos en staging. *Falta su documentación de campos → punto 3.*
3. [~] **EN CURSO (a verificar)**: el agente ahora **reporta su esquema** (todas las tablas + columnas) al central cada arranque y cada 6 h (`POST /api/v1/esquema`, entidad `EsquemaTabla`). El registro de fuentes ya documenta los campos desde ese esquema, y el builder ofrece los campos de la fuente central.
4. [~] **EN CURSO (a verificar)**: **navegador de tablas con búsqueda en el central** — en Reglas → Fuentes de datos, el campo "Tabla" busca por nombre contra el esquema y muestra las columnas (auto-documentación). *Falta: quitar el explorador/fuentes locales del panel del agente (siguiente paso, una vez verificado el navegador central).*
5. [ ] **Editor de reglas del motor demasiado simple**: solo cambia umbral. Revisar si debe permitir más (activar/desactivar ya está; evaluar editar descripción u otros parámetros con cuidado de no romper la lógica fija del detector).
6. [ ] **No se probó el agente con datos reales**: insertar datos en Firebird (turno abierto, factura backdated, fila de tabla extra) para ver que el agente extrae y el central genera alertas **Operativa y Auditoría**.
7. [ ] **No se probó la interfaz nueva de gestión de notificaciones de la estación** ni todas las del agente en Chrome.
8. [ ] **Faltan tests** que validen los métodos nuevos (memoria de envío, fuentes centrales, ámbito de reglas personalizadas).

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

## 🔜 Subsistema de alertas por estación (terminar lo empezado)
- [ ] **Endpoint de agregación**: problemas operativos agrupados por estación, con conteo por día.
- [ ] **Pestaña "Problemas de estación"** en el frontend: tabla por estación → clic en el nombre despliega la lista (documentación), separada de la bandeja principal de auditoría.
- [ ] **Asociación usuario ↔ estación**: un administrador de estación ve/recibe solo lo de SU estación.
- [ ] **Correo de contacto por estación** + aviso por email de problemas operativos (decisión "Ambas": in-app/SignalR + correo).
- [ ] **Gobernanza/auditoría**: quién puede ver/gestionar cada carril, registrado en el log.

## 🔜 Detectores nuevos (patrones del ingeniero + tesis)
Operativos (carril Operativa → estación):
- [ ] **Turno sin cerrar** (TURN.EST_TURN abierto / FFI_TURN viejo). *Lo del inge: "se olvidan de cerrar turno".*
- [ ] **Despacho mal cerrado / no facturado** (DESP.FAC_DESP / EST_DESP). *"No lo colgó bien".*
- [ ] **Campos faltantes** (ya parcial; ampliar a más campos/condiciones).

De auditoría/fraude (carril Auditoría → central):
- [ ] **Cuadre de tanque** (TANQ_REPO.DIFERENCIA / TANQ_RIND.DIF_VEN_RIND > tolerancia).
- [ ] **Crédito no autorizado / sin garante / a cliente no habilitado** (CRED_CABE: COD_GARA, COD_SOCI). *Lo del inge: "autorizan créditos a quien no debe".*
- [ ] **Cancelar-reingresar recurrente (kiting)** (ANUL + re-creación al día siguiente). *Lo del inge.*
- [ ] **Placa bloqueada / sobre cupo** (PLACA_BLOQ / PLACA_CUPO).
- [ ] **Cuadre forzado** (faltan $X y aparece justo un crédito/ajuste de ~$X pegado al cierre que lleva el faltante a 0; recurrente/escalado). *Requiere estudiar primero cómo funciona el cuadre y el conteo de efectivo para no alertar de más.*

## 🔜 LA IDEA GRANDE — Plataforma de detección configurable (lo que más me recalcaste)
> "que el agente pueda enviar info de múltiples tablas o las que selecciones… mejorar el creador de reglas para que funcione afuera de CONTAC… agregar una tabla nueva, verificar que exista, devolver la documentación automática de sus campos, y crear reglas sin tocar código ni llamar a un ingeniero."

- [ ] **Fuentes de extracción configurables (multi-tabla):** reemplazar el SQL fijo del agente por una config editable (`FuenteExtraccion`: tabla, columnas, columna de watermark, filtro). Enviar las tablas que se elijan sin recompilar. (Hoy el agente solo manda 7 consultas fijas de CONTAC.)
- [ ] **Registro dinámico de tablas con verificación:** al agregar una tabla nueva, comprobar que existe en Firebird (`RDB$RELATIONS`); si no, rechazar con mensaje claro.
- [ ] **Auto-documentación de la tabla:** leer `RDB$RELATION_FIELDS` + `RDB$FIELDS` y mostrar campos, tipos, longitud y nullabilidad — un "diccionario" de la tabla en pantalla.
- [ ] **Creador de reglas genérico (fuera de CONTAC):** que una regla apunte a cualquier fuente registrada y referencie sus campos por nombre; evaluador acotado y seguro.
- [ ] **Seguridad/gobernanza de la plataforma:** solo lectura, lista blanca de tablas/columnas (anti-inyección), evaluador sin código arbitrario, solo Supervisor/Admin registran fuentes/reglas, todo auditado.
- [ ] **Investigación tabla-por-tabla exhaustiva:** profundizar el catálogo (hoy revisé las ~15 tablas de más valor; faltan el resto de las ~200 para no dejar señales útiles fuera).

## 🔜 Centralizar fuentes — mejoras pendientes (la base ya está hecha esta ronda)
- [ ] **Selector de tabla en el central tirando de un agente conectado:** hoy el admin escribe el nombre de la tabla a mano en "Fuentes de datos". Falta un endpoint puente (central → agente conectado) que liste/describa tablas para elegirla con buscador y ver sus campos sin salir del central. Mientras tanto se usa el explorador del panel del agente.
- [ ] **Verificación al registrar en el central:** avisar si la tabla no existe en alguna estación (requiere el puente anterior). Hoy cada agente la omite en silencio si no existe.
- [ ] **Migrar fuentes locales existentes** de algún agente al catálogo central (utilidad de una sola vez), si llegan a usarse.

## 🔜 Watermark robusto (cerrar puntos ciegos)
- [ ] **Watermark por ID monotónico** (generadores GEN_*_ID) + ventana de solapamiento, para que registros viejos/backdated o con reloj desfasado no se salten.
- [x] **Memoria de envío en el agente** — HECHO esta ronda (`SentMemory`): ya no reenvía el mismo registro cada ciclo.

## 🔜 Empaquetado plug-and-play + limpieza
- [ ] **Purgar/arreglar los .bat** obsoletos o redundantes (incluye limpiar los `commit_*.bat`, `diag_*`, `build_errors.txt`, `*.done` que fui dejando).
- [ ] **Setup/.exe plug-and-play** del central y agente, fácil de actualizar (el agente ya tiene feed de actualización; falta rematar el central y un instalador único).

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

**Pendiente:**
- [ ] Watermark por ID monotónico + ventana de solapamiento (punto ciego de fecha).
- [ ] UI para asignar estación/correo a un usuario desde el formulario (hoy por API).
- [ ] Pruebas de seguridad sin huecos (revisión dedicada).
- [ ] Empaquetado final del instalador (.iss) del central, si se quiere un setup único.

---

## Nuevos pedidos (junio 2026)

- [ ] **Central — apartado de configuración de conexiones (solo Admin):** sección para gestionar y
  configurar las conexiones/estaciones (nombre, zona, horario, correo de contacto, activa) desde la
  interfaz; restringido a Administrador.
- [ ] **Central — Monitoreo: usuarios conectados:** sección que muestre los usuarios actualmente
  conectados al sistema central (vía SignalR: rastrear conexiones del hub y exponerlas).
- [ ] **Agente — autodetección de Firebird:** botón junto a la conexión Firebird que busque
  automáticamente la base CONTAC.FDB (host/puerto/ruta comunes) sin escribir la dirección a mano.
  (Nombre propuesto: "Detectar Firebird automáticamente".)
- [ ] **Pruebas rigurosas en Chrome:** recorrer todas las interfaces y procesos ya implementados,
  verificar que reflejan y funcionan según lo previsto, y corregir cualquier bug de inmediato.
