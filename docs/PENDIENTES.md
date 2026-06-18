# Backlog / pendientes — PetrolRíos

Lista viva de lo acordado en las sesiones, con estado. Orden = prioridad sugerida.
Última actualización: junio 2026.

---

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

## 🔜 Watermark robusto (cerrar puntos ciegos)
- [ ] **Watermark por ID monotónico** (generadores GEN_*_ID) + ventana de solapamiento, para que registros viejos/backdated o con reloj desfasado no se salten.
- [ ] **Memoria de envío en el agente** (que no reenvíe el mismo registro cada ciclo aunque la idempotencia del central ya lo blinde — ahorra red).

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
