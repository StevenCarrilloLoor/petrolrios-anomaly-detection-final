# Guía de instalación PetrolRíos — paso a paso (Windows · Linux · macOS)

Esta guía está pensada para que **cualquiera** pueda instalar el sistema sin saber de programación.
Sigue los pasos en orden. Funciona igual en Windows, Linux y macOS.

## Cómo está armado el sistema (en 1 minuto)

- **Sistema central** = una página web + la base de datos. Vive en **una computadora "servidor"**.
  La gente lo usa abriendo el **navegador** (no instalan nada).
- **Agente** = un programita que se instala en **cada estación** (donde está el Firebird/Contaplus).
  Lee los datos locales y los **envía** al central por internet.
- **Monitor** = visor local en cada estación; muestra los problemas operativos de esa estación.

Regla de oro: **la base de datos vive en UN solo lugar** (dentro del servidor central). Todo lo
demás se conecta a ese central por su dirección.

---

# PARTE A — Instalar el sistema central (el servidor)

Elige **una computadora** que quede encendida (será el servidor). Da igual el sistema operativo.

## Paso 1 — Instalar Docker

Docker es lo único que hay que instalar. Trae adentro la base de datos y todo lo demás.

- **Windows / macOS:** descarga **Docker Desktop** de <https://www.docker.com/products/docker-desktop>,
  instálalo y ábrelo. Espera a que diga **"Running"**.
- **Linux (Ubuntu/Debian):**
  ```bash
  curl -fsSL https://get.docker.com | sudo sh
  sudo usermod -aG docker $USER   # cierra sesión y vuelve a entrar
  sudo systemctl enable --now docker
  ```

## Paso 2 — Poner el proyecto en esa computadora

Copia la carpeta `petrolrios-anomaly-detection` (por USB o red), o clónala si la tienes en un
repositorio. Toda la carpeta, completa.

## Paso 3 — Ejecutar el instalador

Entra a la carpeta `INSTALACION` del proyecto:

- **Windows:** doble clic en **`instalar-windows.bat`**.
- **Linux / macOS:** abre una terminal en esa carpeta y ejecuta:
  ```bash
  chmod +x instalar-linux-mac.sh
  ./instalar-linux-mac.sh
  ```

El instalador hace todo solo: verifica Docker, **crea las contraseñas internas**, **detecta la IP**
de tu red, genera la configuración y levanta el sistema. La primera vez tarda unos minutos (está
construyendo todo). Solo te pregunta el **correo** para los avisos (puedes dejarlo vacío y ponerlo
después).

> ⚠️ Si la base de datos no respondiera, en vez de fallar aparece una **pantalla de Configuración
> inicial** en el navegador para indicar dónde está la base. (Normalmente no la verás.)

## Paso 4 — Entrar por primera vez

Cuando termine, el instalador te muestra la dirección. Abre el navegador en:

```
http://localhost:8080
```

- Usuario: **admin@petrolrios.com**
- Clave: **Admin123!**  → el sistema te obliga a cambiarla. Listo, ya está corriendo.

---

# PARTE B — Conectarse desde otras computadoras

El central es una **página web**: cualquiera en la red entra por el navegador, **sin instalar nada**.

1. Averigua la **IP del servidor** (la computadora donde instalaste el central):
   - **Windows:** abre `cmd` y escribe `ipconfig` → usa la "Dirección IPv4" (ej. `192.168.1.50`).
   - **Linux:** `hostname -I` → toma la primera (ej. `192.168.1.50`).
   - **macOS:** `ipconfig getifaddr en0`.
   - (El instalador también te la mostró al terminar.)
2. Desde cualquier otra computadora de la **misma red**, abre el navegador en:
   ```
   http://IP-DEL-SERVIDOR:8080      (ej. http://192.168.1.50:8080)
   ```
3. Cada persona entra con **su** usuario. El administrador los crea desde **Usuarios**.

> Si Windows pregunta por el firewall la primera vez, permite el acceso al puerto 8080.

---

# PARTE C — Las estaciones (agente y monitor), SIN VPN

Las estaciones están en distintos lugares y se conectan al central **por internet**. **No hace
falta VPN**: el agente **llama hacia afuera** al central (conexión saliente), así que solo necesitas
que el **central tenga una dirección pública**. La base de datos **nunca** se expone.

## C.1 — Darle al central una dirección pública (elige una)

- **Túnel (recomendado, gratis, sin tocar el router):** instala **Cloudflare Tunnel**
  (`cloudflared`) en el servidor y obtienes una URL `https://...` que apunta a tu central. Las
  estaciones usan esa URL. Guía: <https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/>
- **Central en la nube:** un servidor (VPS) con IP pública + dominio. Repites la PARTE A en el VPS.
- **Router de la oficina:** abre/redirige el puerto 8080 + un DNS dinámico. (Más configuración.)

En todos los casos, para internet usa **HTTPS** (el túnel o un reverse proxy lo dan). El agente se
autentica con su cuenta, así que el envío va seguro.

## C.2 — Instalar el AGENTE en cada estación

El agente corre **en la PC de la estación** (la que tiene el `CONTAC.FDB` de Contaplus). Hay
ejecutables para los 3 sistemas (se generan con `ejecutables/4-PUBLICACION/publicar.bat` y quedan en
`dist/`):

1. Copia la carpeta del agente a la estación (`dist/PetrolRios-Agente` en Windows; el binario
   `agente-linux` / `agente-macos-*` en Linux/macOS).
2. Ejecútalo y abre su panel local en **`http://localhost:5180`**.
3. En la pestaña **Configuración** pon:
   - **Servidor (ServerUrl):** la dirección pública del central (la del túnel, ej. `https://central.tuempresa.com`).
   - **Código de estación** y las **credenciales** de esa estación (`agent-est-00X@...`).
   - La conexión al **Firebird local** (normalmente `localhost`, `C:\CONTAC\CONTAC.FDB`).
4. Instálalo como **servicio** para que arranque solo al prender la PC:
   - **Windows:** `ejecutables/4-PUBLICACION/instalar_agente_servicio.bat`
   - **Linux:** `ejecutables/4-PUBLICACION/instalar_agente_servicio.sh`
   - **macOS:** `ejecutables/4-PUBLICACION/instalar_agente_servicio_macos.sh`

El agente es **store-and-forward**: si el central está caído, guarda y reenvía cuando vuelve. No se
pierden datos.

## C.3 — Instalar el MONITOR en cada estación

Igual que el agente, pero es solo un visor. Copia `dist/PetrolRios-Monitor`, ábrelo en
**`http://localhost:5190`**, configúralo con el mismo **ServerUrl** y credenciales, e instálalo como
servicio con `instalar_monitor_servicio.bat` (Windows) o el equivalente `.sh` en Linux/macOS.

---

# PARTE D — Si se va la luz

No se "daña" nada y todo vuelve solo, **si quedó instalado como servicio**:

- **Central:** Docker reinicia los contenedores solo. Solo asegúrate de que Docker arranque con la
  máquina: en Windows/macOS activa *Docker Desktop → "Start when you log in"*; en Linux
  `sudo systemctl enable docker`.
- **Agente y monitor:** instalados como servicio (PARTE C) → arrancan solos al prender la PC.
- El agente reenvía lo que quedó pendiente. **No se pierden datos.**

---

# PARTE E — Actualizar (sin reinstalar en cada estación)

- **Central — desde el propio servidor:** trae el código nuevo y corre:
  ```bash
  git pull
  docker compose -f docker-compose.prod.yml up -d --build
  ```
  Reconstruye y reinicia, conservando la base. Es **un solo lugar**.

- **Central — de forma REMOTA (desde tu computadora):** no necesitas ir al servidor. Desde tu PC,
  con tus cambios ya commiteados, ejecuta:
  - **Windows:** `INSTALACION\actualizar-central.ps1` (o `powershell -File ...`)
  - **Linux/macOS:** `./INSTALACION/actualizar-central.sh`

  La primera vez te pregunta el **servidor SSH** (`usuario@ip-o-dominio`) y la **ruta** del proyecto
  allá (lo guarda para la próxima). Entonces: sube tus commits a GitHub, entra por **SSH** al
  servidor, hace `git pull` y reconstruye el central con Docker — todo desde tu máquina. La base se
  conserva. (Requisito: que el servidor tenga **SSH** accesible: en la misma red, por VPN/túnel, o
  con IP pública. En un servidor Windows, habilita **OpenSSH Server**.)
- **Agentes y monitores (las 10 estaciones, sin visitarlas):** publicas **una** versión nueva y
  cada estación la baja sola, verifica el checksum, intercambia su ejecutable y reinicia su servicio.
  Pasos: sube `<Version>` en `Directory.Build.props`, corre `publicar.bat`, y publica el
  **manifiesto** (plantilla `ejecutables/4-PUBLICACION/agente-version.example.json`) en
  `{central}/api/v1/agente/version` (agente) y `{central}/api/v1/monitor-estacion/version` (monitor).

---

# PARTE F — Problemas comunes

| Síntoma | Qué hacer |
|---|---|
| "Docker no responde" | Abre Docker Desktop (Win/mac) o `sudo systemctl start docker` (Linux). |
| No abre desde otra PC | Verifica la IP, que estén en la misma red y el **firewall** del puerto 8080. |
| Olvidé la contraseña del admin | Pon `ADMIN_PASSWORD_INICIAL` en `.env`, reinicia el central, entra, y vuelve a dejarlo vacío. |
| Ver qué está pasando (logs) | `docker compose -f docker-compose.prod.yml logs -f central` |
| Apagar / prender el central | `docker compose -f docker-compose.prod.yml down` / `up -d` (los datos se conservan). |

Más detalle de operación en `docs/OPERACION.md` y de despliegue en `docs/DESPLIEGUE.md`.
