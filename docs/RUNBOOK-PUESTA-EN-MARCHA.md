# Runbook de puesta en marcha — PetrolRíos

Guía única para **levantar todo el sistema** y para el **despliegue en San Pío**.
Versión actual del sistema: **2.4.0** (fuente única: `Directory.Build.props`; fluye a los 3 ejecutables,
al heartbeat del agente, al manifiesto de actualización y a los instaladores).

Tres sistemas independientes:

| Sistema | Qué es | Puerto | Dónde corre |
|---|---|---|---|
| **Central** | API + tablero web + base de datos | 8080 | Una computadora servidor (con Docker) |
| **Agente** | Lee la Firebird de la estación y envía al central | 5180 (panel local) | Cada estación |
| **Monitor** | Vista de solo lectura de problemas operativos | 5190 | Opcional, por estación |

---

## 1. Central (la computadora servidor) + base de datos

**Requisito:** Docker Desktop instalado y corriendo.

1. Copia el proyecto (o al menos: `Dockerfile`, `docker-compose.prod.yml`, `.env.example`, carpeta `ejecutables/`, `src/`, `frontend/`) a la computadora servidor.
2. Doble clic en **`ejecutables/6-INSTALAR-EN-NUEVO-PC/instalar-central-windows.bat`**
   (en Linux/macOS: `instalar-central-linux-mac.sh`).
   - Verifica Docker, genera `.env` con **contraseñas aleatorias** (Postgres y JWT), detecta la IP de la red.
   - Pregunta el correo para avisos (Gmail + App Password). Puedes dejarlo vacío y ponerlo luego en Ajustes.
   - Levanta Postgres + el central con `docker compose -f docker-compose.prod.yml up -d --build`.
3. Entra:
   - En esa máquina: **http://localhost:8080**
   - Desde otra máquina de la red: **http://IP-DEL-SERVIDOR:8080**
   - Usuario **admin@petrolrios.com**, clave **Admin123!** (te pedirá cambiarla al entrar).

**Base de datos:** Postgres 16 en contenedor, volumen `pgdata` (persiste tras reinicios/cortes). Las
migraciones EF se aplican solas al arrancar. La configuración que se escribe desde Ajustes
(`connection.json`, `operacion.json`) y el manifiesto de actualización (`agente-version.json`) viven en los
volúmenes `central-config/` y `central-descargas/` (también persisten).

**Tras un corte de luz:** en Docker Desktop activa *"Start Docker Desktop when you log in"* y el sistema
vuelve solo (los contenedores tienen `restart: unless-stopped`).

> Alternativa sin Docker: `ejecutables/5-PUBLICACION-Y-DESPLIEGUE/publicar-servidor-agente-y-monitor.bat`
> genera un `.exe` autocontenido en `dist/PetrolRios-Servidor` (puerto 5170), pero necesitas un PostgreSQL
> aparte. La vía Docker es la recomendada.

---

## 2. Agente (cada estación)

**Generar el portable** (en tu máquina de desarrollo, una vez por release):
`ejecutables/5-PUBLICACION-Y-DESPLIEGUE/publicar-solo-el-agente-multiplataforma.bat`
→ deja `dist/agente-windows` (y linux/macOS). No necesita .NET en la estación.

**Instalar en una estación nueva:**
1. En el central: **Nueva estación** (crea la estación + un usuario-agente con sus credenciales).
2. Copia la carpeta `dist/agente-windows` a la PC de la estación (la que tiene la Firebird).
3. Ejecuta `PetrolRios.StationAgent.exe` y abre **http://localhost:5180**.
4. En **Configuración**:
   - Nombre de la estación + **URL del central** (`http://IP-DEL-SERVIDOR:8080`).
   - **Detectar Firebird automáticamente** (ya busca `CONTAC.FDB` y `CONTAB.FDB`, incluida
     `C:\Programas\ContaGober1\Datosc`). Si la base es Firebird 2.5 → WireCrypt **Disabled**.
   - **Probar Firebird** y **Probar servidor** → **Guardar**.
5. En **Arranque automático al encender** → **Activar** (queda arrancando solo, oculto, al iniciar sesión).

---

## 3. Monitor (opcional, por estación)

Portable en `dist/PetrolRios-Monitor` (lo genera `publicar-servidor-agente-y-monitor.bat`). Configúralo
con la estación y el central; panel en **http://localhost:5190**. **Se auto-actualiza solo** (revisa el
central cada 6 h y se reinstala sin intervención).

---

## 4. Versionado y actualización (control de versiones)

La versión es única en `Directory.Build.props`. Para publicar una versión nueva:

1. Sube el número en `Directory.Build.props` (p. ej. `2.4.0` → `2.5.0`).
2. Reconstruye: `publicar-solo-el-agente-multiplataforma.bat` (agente) y, si aplica,
   `publicar-servidor-agente-y-monitor.bat`.
3. **Publica la actualización del agente con un clic:**
   `ejecutables/5-PUBLICACION-Y-DESPLIEGUE/publicar-actualizacion-del-agente.bat`
   → calcula el SHA256, copia el exe a `central-descargas/` y escribe `central-config/agente-version.json`.
4. Si el central corre con Docker, reinícialo para montar esas carpetas:
   `docker compose -f docker-compose.prod.yml up -d`.

**Cómo se actualiza cada sistema:**
- **Monitor:** **automático** (cada 6 h descarga, verifica el checksum, se reemplaza y reinicia).
- **Agente:** **avisa** en su panel (revisa el feed periódicamente) y se aplica **con un clic** desde el
  panel local de la estación (botón *"Aplicar actualización"*). Es a propósito: el agente toca la Firebird,
  por eso pide confirmación humana. La descarga verifica el **SHA256** antes de reemplazar el exe.
- **Central:** se actualiza reconstruyendo la imagen (`docker compose ... up -d --build`) o, en remoto, con
  `ejecutables/5-PUBLICACION-Y-DESPLIEGUE/actualizar-central-remoto.ps1` (deploy por SSH).

---

## 5. San Pío — este despliegue (dos caminos, decide en el sitio)

El agente que está hoy en San Pío es **antiguo** (le faltan built-ins y puede que no tenga la lógica de
actualización). Por eso lleva preparadas **las dos opciones**:

**Opción A — Reemplazo limpio (recomendada para este viaje).** Determinista, no depende de lo que tenga el
agente viejo:
1. Detén el agente viejo (ciérralo / `sc stop` si era servicio) y borra su carpeta.
2. Copia la carpeta `dist/agente-windows` (versión 2.4.0) a la PC.
3. Ejecuta el exe, configura (auto-detectar la base, URL del central, probar) y **Guarda**.
4. **Activa el arranque automático** en el panel.
5. (Ya hecho desde el central: se borró la fuente `Dcto` duplicada.)

**Opción B — Actualización con un clic.** Solo si el agente que está allá **ya tiene** la lógica de
actualización y su feed apunta al central:
1. En tu máquina: `publicar-actualizacion-del-agente.bat` (publica 2.4.0) y reinicia el central Docker.
2. En San Pío, abre el panel del agente (**http://localhost:5180**) → te mostrará *"Actualización 2.4.0
   disponible"* → pulsa **Aplicar actualización**. Descarga, verifica el checksum, se reemplaza y reinicia.

**Validación tras cualquiera de las dos:** en el panel del agente, *Probar Firebird* y *Probar servidor* en
verde; en el central, la estación **En línea** y, en **Datos recibidos**, que empiecen a llegar
**Factura (DCTO)** al día (el fix de watermark por reloj de Firebird destranca los built-ins).

---

## Checklist rápido

- [ ] Central levantado (`instalar-central-windows.bat`) y accesible en `http://IP:8080`.
- [ ] Estación creada en el central (**Nueva estación**) con su usuario-agente.
- [ ] Agente 2.4.0 en la estación: Firebird detectada, central conectado, **Guardado**.
- [ ] **Arranque automático activado** en el agente.
- [ ] (Opcional) Monitor instalado.
- [ ] La estación aparece **En línea** y llegan datos en **Datos recibidos**.
