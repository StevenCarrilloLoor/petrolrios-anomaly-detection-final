# Cómo configurar el agente de una estación (alta + conexión)

> Para cuando instales el agente en una estación y necesites que **se conecte al
> central y empiece a enviar**. Pensada para hacerla tú solo. Si algo falla, abajo
> hay una tabla de "síntoma → causa → solución" con los errores más comunes.

---

## Idea clave (leer esto primero)

El central **solo acepta datos de una estación que ya esté registrada** y de un
**usuario-agente que pertenezca a esa estación**. Es el candado de seguridad
(aislamiento por estación). Por eso, si pones un código de estación inventado, el
agente lee Firebird sin problema pero el central lo **rechaza con 403** y no se
inserta nada (las transacciones quedan en cola "store-and-forward").

Las estaciones y sus usuarios ya vienen creados (sembrados):

| Estación (código) | Usuario-agente (email) | Contraseña |
|---|---|---|
| `EST-001` … `EST-010` | `agent-est-001@petrolrios.com` … `agent-est-010@petrolrios.com` | `Agent123!` |

> ⚠️ **Producción:** esa `Agent123!` es la de demo. Antes del go-live, cambia la
> contraseña de cada usuario-agente (desde Usuarios en el central). Ver
> `docs/ANALISIS-SEGURIDAD.md`.

---

## Pasos

### 1. Asegúrate de que el central esté encendido

- El servidor central (`PetrolRios.Api`) debe estar corriendo y accesible en
  `http://IP-DEL-SERVIDOR:5170`.
- Prueba rápida desde la estación: abre esa URL + `/swagger` en el navegador. Si no
  carga, el central está apagado o la IP/puerto están mal (no sigas hasta resolverlo).

### 2. Abre el panel del agente

- Ejecuta `PetrolRios.StationAgent.exe` (o el servicio, si lo instalaste).
- Abre **`http://localhost:5180`** → pestaña **Configuración**.

### 3. Llena la configuración

- **Código de estación:** uno **registrado** (p. ej. `EST-002`). No inventes códigos.
- **Nombre / Zona:** descriptivos (libres).
- **Servidor central:** `http://IP-DEL-SERVIDOR:5170` (en tu PC de pruebas:
  `http://localhost:5170`).
- **Email / Contraseña:** el usuario-agente **de esa estación**
  (p. ej. `agent-est-002@petrolrios.com` / `Agent123!`). El email debe corresponder
  al mismo código de estación de arriba.
- **Firebird (Contaplus):**
  - **Host / Puerto:** dónde está el `CONTAC.FDB` (en la demo: `localhost` : `3051`;
    en una estación real, normalmente `localhost` : `3050`).
  - **Base:** la ruta del `.FDB` (demo: `/firebird/data/CONTAC.FDB`; estación real:
    `C:\CONTAC\CONTAC.FDB`).
  - **Usuario / Contraseña:** `SYSDBA` / `masterkey` (o lo que use esa estación).
  - **WireCrypt:** `Disabled` para Firebird 2.5; `Enabled` para Firebird 3+.
  - **Solo lectura:** déjalo activado (el agente nunca escribe en Firebird).
- **Guarda.**

### 4. Verifica (los tres botones, en orden)

1. **Probar conexión Firebird** → debe decir *"Conexión exitosa — N documentos en DCTO"*.
2. **Probar conexión al servidor** → debe decir *"Autenticado contra http://… en N ms"*.
3. **Sincronizar ahora** → en **Monitoreo** debe subir **"Transacciones enviadas"** y
   bajar **"Pendientes (store-and-forward)"** a 0.

Si los tres pasan, la estación quedó conectada y enviando. Confírmalo en el central:
entra a la app, filtra alertas por esa estación y verás su actividad.

---

## Si algo falla — tabla de diagnóstico

| Lo que ves en el panel / log | Qué significa | Cómo se arregla |
|---|---|---|
| **403** en el log, o "no autorizado" | El código de estación **no está registrado** o **no coincide** con el usuario-agente | Usa un código real (`EST-001…010`) y su `agent-est-00X@petrolrios.com` |
| **401 (Unauthorized)** | Email o contraseña del usuario-agente **incorrectos** | Verifica email = `agent-est-00X@petrolrios.com` y la contraseña |
| **"No se pudo contactar al servidor"** / *connection refused (…:5170)* | El central **está apagado** o la URL/IP/puerto están mal | Enciende el central; revisa "Servidor central" = `http://IP:5170` |
| **"Pendientes" sube y "Enviadas" = 0** | El central está **rechazando** los envíos (por alguna de las filas de arriba) | Corrige la config; los lotes en cola **se vacían solos** al reconectar (el central descarta los repetidos, no se duplica nada) |
| Error al **Probar conexión Firebird** | `WireCrypt`, credenciales o ruta del `CONTAC.FDB` mal | `WireCrypt` Disabled (FB 2.5) / Enabled (FB 3+); revisa usuario/clave y la ruta |
| La cola crece sin parar por horas | Estación quedó mal configurada y nadie la corrigió | Es esperado (no pierde datos), pero corrige la config cuanto antes; si quieres, borra los `batch_*.json` de la carpeta `pending\` para empezar limpio |

---

## Nota sobre la cola "store-and-forward"

Cuando el central rechaza o está caído, el agente **guarda** cada lote en su carpeta
`pending\` y reintenta — así **no se pierde nada**. Apenas la conexión se arregla, los
manda todos. Si no quieres mandar un backlog viejo, baja el agente, borra los
`batch_*.json` de `pending\`, corrige la configuración y vuelve a subirlo.

---

## Resumen rápido

```
1. Central encendido y accesible (http://IP:5170/swagger carga)
2. Agente -> http://localhost:5180 -> Configuración
3. Código estación = uno REGISTRADO (EST-00X)
   Email/clave  = agent-est-00X@petrolrios.com / Agent123!
   Firebird     = host/puerto/ruta/usuario/clave/WireCrypt
   Guardar
4. Probar Firebird (OK) -> Probar servidor (Autenticado) -> Sincronizar ahora
5. Confirmar en el central que llega la actividad de esa estación
```
