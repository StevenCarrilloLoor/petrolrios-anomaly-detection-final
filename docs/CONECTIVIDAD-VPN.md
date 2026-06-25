# Conectividad agente ↔ central por VPN (producción)

Guía de solución para conectar el **Station Agent** (en la PC del POS de cada estación, junto a
Firebird) con el **servidor central**, a través de redes distintas y hostiles (NAT, sin DHCP,
firewall de POS). Nace del problema real encontrado en la estación **SanPio (EST-012)**.

---

## 1. El problema (diagnóstico)

- El central (laptop) y el agente (servidor del POS) están en **subredes distintas**:
  laptop en `192.168.11.x` (gateway `.11.10`), servidor en `192.168.0.x / 192.168.10.x`
  (gateway `192.168.0.1`). No comparten red.
- Entre ellas hay **NAT/aislamiento**: el central alcanza al servidor (sale hacia afuera),
  pero el servidor **no** puede iniciar conexión hacia el central. Y el agente necesita
  justo eso: **iniciar la conexión hacia el central**.
- La red del POS es **hostil para VPN**: sin DHCP (IP fija obligatoria), uno de los
  adaptadores sin gateway (`0.0.0.0`), y firewall que bloquea puertos no estándar.

> El agente es **push / saliente**: lee Firebird y hace `POST /api/v1/ingesta` al central.
> Nunca recibe conexiones entrantes. Por eso la solución correcta es darle al central una
> **dirección estable y alcanzable**, y que el agente **salga** hacia ella.

---

## 2. Por qué falló ZeroTier en la máquina de Firebird

ZeroTier quedó atascado en **`REQUESTING_CONFIGURATION`** (el nodo no recibió su configuración
del controlador). En una PC de POS con Windows Server, suelen apilarse varias causas:

1. **UDP 9993 bloqueado por la red del POS.** ZeroTier necesita salir por **UDP 9993** a sus
   root servers. Muchas redes de POS bloquean puertos UDP no estándar, y el *fallback* a TCP de
   ZeroTier **a menudo no se activa** → se queda pidiendo config.
2. **Conflicto del puerto 9993 con el servicio "IP Helper" de Windows.** "IP Helper" (túnel
   IPv6-sobre-IPv4) puede **tomar el 9993 antes** que ZeroTier. Es más frecuente si la máquina
   tiene **Docker o WSL** (reservan rangos de puertos vía IP Helper).
3. **Nodo no autorizado** en my.zerotier.com (red privada → el controlador no envía config
   hasta marcar "Auth").
4. **Antivirus con "Internet Security"** o el **firewall de Windows Server** (más estricto)
   bloqueando ZeroTier.

---

## 3. Solución recomendada: **Tailscale** (en vez de ZeroTier)

Para una red de POS hostil, **Tailscale es notablemente más confiable** que ZeroTier:

- Cuando la conexión directa por UDP falla (POS bloquea UDP), Tailscale **reenvía el tráfico
  cifrado (WireGuard) por sus relays DERP sobre HTTPS, puerto 443**. Como el POS **sí** permite
  HTTPS/443 (lo necesita para tener internet), **Tailscale conecta aunque el UDP esté bloqueado**.
- Tasa de éxito >95% incluso con NAT simétrico y firewalls corporativos.
- Sin baile de autorización: inicias sesión con Google/Microsoft y los dispositivos aparecen.
- **Plan gratuito** de sobra para 10 estaciones + central. Corre en Windows Server.
- Cada dispositivo recibe una IP estable `100.x.x.x` (rango CGNAT) y un nombre por MagicDNS.

### Pasos (una sola vez por máquina)

**En el central (tu laptop):**
1. Descarga Tailscale de `https://tailscale.com/download/windows` e instálalo.
2. Inicia sesión (crea el *tailnet* con tu cuenta Google/Microsoft).
3. Anota la IP que te asigna: algo como `100.x.y.z` (con `tailscale ip -4` o en la app).
4. Deja el central escuchando en `0.0.0.0:5170` (ya lo hace con `reiniciar-central-accesible-por-red.bat`).

**En el servidor del POS (donde está el agente + Firebird):**
1. Instala Tailscale.
2. Inicia sesión con **la misma cuenta** (mismo tailnet).
3. Verifica: `tailscale status` debe listar el central; `ping 100.x.y.z` (la IP del central)
   debe responder.

**En el panel del agente (`localhost:5180` → Configuración):**
- URL del servidor = **`http://100.x.y.z:5170`** (la IP Tailscale del central).
  Alternativa con MagicDNS: `http://nombre-del-central:5170`.
- "Probar servidor" → debe autenticar.

Esa IP `100.x.y.z` **no cambia** aunque el central o la estación salten de red física. Es la
forma estable y definitiva.

---

## 4. Alternativa: arreglar ZeroTier (si se decide mantenerlo)

En el servidor del POS, PowerShell **como administrador**:

1. **Autorizar el nodo:** my.zerotier.com → red `VPN_JUEGOS` (`35c192ce9ba4c951`) → **Members**
   → marca **Auth** en el nodo de la estación.
2. **Resolver el conflicto del puerto 9993 (IP Helper):**
   - `Services.msc` → detén **"IP Helper"** → reinicia el servicio **ZeroTier One** →
     vuelve a iniciar **IP Helper**.
   - Si la máquina tiene Docker/WSL, revisa que no estén reservando el 9993.
3. **Permitir ZeroTier en el firewall** (salida UDP 9993) y excluirlo del antivirus.
4. **Confirmar internet** en esa máquina (abre `google.com`). Sin internet, ninguna VPN funciona.
5. Si sigue fallando: reinstalar ZeroTier y reiniciar.
6. Verifica: `zerotier-cli listnetworks` → estado **OK** con IP `10.144.x.x`.

> Aun arreglándolo, en una red que bloquea UDP, ZeroTier puede quedarse relevando con más
> latencia o no conectar. Por eso **Tailscale es la apuesta segura** en este entorno.

---

## 5. Arquitectura de producción recomendada

Como el agente solo hace conexiones **salientes** al central:

- **Opción A (con VPN, lo pedido):** malla **Tailscale** uniendo central + las 10 estaciones.
  Cada agente apunta a la IP `100.x.x.x` del central. Seguro (tráfico cifrado WireGuard, el API
  no queda expuesta a internet público) y robusto ante el NAT de cada estación.
- **Opción B (la más simple):** central en la **nube** (AWS/Azure) con HTTPS; cada agente sale
  por 443 hacia él. No requiere VPN porque el agente solo sale. (La tesis menciona AWS RDS.)

Recomendación: **Tailscale** para cumplir el requisito de VPN con mínima fricción; mantener la
opción de nube como evolución.

---

## 6. Para destrabar la prueba de HOY (sin depender de la VPN)

Si necesitas probar el flujo ya, sin pelear con la VPN: pon el central y el agente en la **misma
subred** con **IP estática** (la red del POS no da DHCP):

```
# En la laptop (central), PowerShell admin — ajusta a la subred real del servidor:
netsh interface ip set address name="Ethernet" static 192.168.0.200 255.255.255.0 192.168.0.1
# Volver a DHCP luego:
netsh interface ip set address name="Ethernet" dhcp
```

Luego el agente usa `http://192.168.0.200:5170`. Esto es solo para la prueba local; la VPN
(Tailscale) es lo definitivo para producción.
