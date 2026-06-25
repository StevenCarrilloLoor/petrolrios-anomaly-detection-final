# Conectar una estación remota al servidor central (ZeroTier)

Escenario: el **servidor central** corre en tu PC y una **estación** (la PC de tu amigo,
con un Firebird CONTAC.FDB) corre el **agente** y le envía datos por internet, sin abrir
puertos del router.

## Resumen

```
  PC del amigo (estación)                 Tu PC (central)
  ┌───────────────────────┐               ┌───────────────────────────┐
  │ Firebird (CONTAC.FDB)  │               │ PostgreSQL                │
  │ PetrolRios.Agente.exe ─┼──ZeroTier────►│ PetrolRios.Api (0.0.0.0)  │
  │ ServerUrl = 10.x:5170  │   (VPN mesh)  │ + Frontend (localhost)    │
  └───────────────────────┘               └───────────────────────────┘
```

## 1. Instalar ZeroTier en las dos PCs

1. Descarga ZeroTier One en **ambas** PCs: https://www.zerotier.com/download/
2. Crea una red gratis en https://my.zerotier.com (botón **Create A Network**). Copia el
   **Network ID** (16 caracteres).
3. En cada PC: abre ZeroTier → **Join Network** → pega el Network ID.
4. En my.zerotier.com, en **Members**, marca el check **Auth** de cada PC para autorizarlas.
5. Cada PC recibe una **IP virtual** (ej. `10.147.20.5`). Anota la **IP de tu PC** (la del central).

> Alternativa equivalente y aún más fácil: **Tailscale** (login con Google). La idea es la misma.

## 2. En tu PC (servidor central)

1. Asegúrate de que **PostgreSQL** esté corriendo. (En una máquina nueva la base de datos se
   **crea sola** en el primer arranque, gracias a las migraciones de EF Core.)
2. Ejecuta **como Administrador**: `ejecutables\1-INICIO\iniciar-central-accesible-por-red.bat`
   - Abre el puerto **5170** en el Firewall de Windows.
   - Arranca la API escuchando en **0.0.0.0:5170** (accesible por la red).
   - Te muestra tus IPs: usa la de **ZeroTier** (10.x.x.x).

## 3. En la PC de la estación (agente)

1. Copia la carpeta del agente (`dist\PetrolRios-Agente` o `dist\agente`) y ejecuta
   `PetrolRios.StationAgent.exe`.
2. Abre `http://localhost:5180` (panel del agente) → pestaña **Configuración**:
   - **URL del servidor central:** `http://TU-IP-DE-ZEROTIER:5170` (la IP de tu PC).
   - **Firebird:** la ruta/credenciales del `CONTAC.FDB` local de esa máquina.
   - **Nombre de la estación:** el que quieras que aparezca en Conexiones.
3. Pulsa **Probar servidor** y **Probar Firebird**, luego **Guardar**.
4. La estación aparecerá **En línea** en tu panel central (Conexiones) y empezará a enviar datos.

## 4. Verificación rápida

- Desde la PC de la estación, en un navegador: `http://TU-IP-DE-ZEROTIER:5170/api/v1/agente/version`
  debe responder (confirma que el central es alcanzable por la red).
- Si no responde: revisa que el central se arrancó con `iniciar-central-accesible-por-red.bat`, que el
  puerto 5170 está abierto (ejecutar el .bat como Administrador) y que ambas PCs están
  autorizadas (**Auth**) en la misma red de ZeroTier.

## Notas de seguridad

- El agente exige login **Admin/Supervisor** verificado contra el central (RBAC), así que aunque
  la estación esté en la red, nadie reconfigura el agente sin credenciales.
- ZeroTier ya cifra el tráfico entre las PCs. Para producción real, lo ideal es HTTPS + un
  dominio; ZeroTier es perfecto para pruebas y demos.
