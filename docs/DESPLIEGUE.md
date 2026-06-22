# Guía de despliegue — PetrolRíos (producción)

Esta guía explica cómo poner el sistema en producción de forma rápida, segura y
multiplataforma (Windows, Linux y macOS), con la **base de datos alojada en un solo lugar**
y una o varias instancias del sistema central conectándose a ella.

## 1. Arquitectura del despliegue

El sistema tiene tres componentes, todos en .NET 9 (multiplataforma), más una base central:

| Componente | Qué hace | Dónde corre |
|---|---|---|
| **Sistema central** (API + panel web) | Detección, alertas, panel, reportes | 1 o varias instancias (servidor / nube / PCs) |
| **Agente de estación** | Lee el `CONTAC.FDB` (Firebird) en **solo lectura** y envía datos | Junto a cada estación |
| **Monitor de estación** | Vista local de problemas operativos de la estación | En cada estación |
| **Base central** (PostgreSQL 16) | Único almacén: alertas, usuarios, reglas, staging | **Un solo lugar** (servidor, otra PC o nube) |

Principio clave: **la base vive en un solo lugar y el central solo necesita su _cadena de
conexión_.** Por eso la base puede estar en cualquier máquina o sistema operativo, y la
conexión se ajusta sin tocar el código (variable de entorno o **Ajustes → Conexión a la base**).

## 2. La forma más rápida: Docker (universal en los 3 SO)

Docker borra las diferencias de sistema operativo: la misma imagen corre igual en Windows,
Linux o macOS. Requisito: **Docker Desktop** (Windows/macOS) o **Docker Engine** (Linux).

### 2.a Todo en una máquina (recomendado para empezar)

Levanta la base **y** el central juntos en un host. "Llego, instalo y queda listo."

```bash
cp .env.example .env
# edite .env: ponga POSTGRES_PASSWORD y JWT_SECRET robustos
docker compose -f docker-compose.prod.yml up -d --build
```

Listo: el panel queda en `http://<host>:8080`. Las migraciones se aplican solas en el primer
arranque y se siembran los datos base. Para detener: `docker compose -f docker-compose.prod.yml down`
(los datos persisten en el volumen `pgdata`).

### 2.b Base en una máquina, central en otra(s)

1. En la **máquina de la base** (servidor):

   ```bash
   cp .env.example .env   # defina POSTGRES_PASSWORD
   docker compose -f docker-compose.db.yml up -d
   ```

   La base queda escuchando en `IP_DEL_SERVIDOR:5432`.

2. En **cada máquina del central**:

   ```bash
   cp .env.example .env
   # defina DB_CONNECTION apuntando a la base del paso 1, y JWT_SECRET
   docker compose -f docker-compose.central.yml up -d --build
   ```

   La conexión también puede cambiarse después desde **Ajustes → Conexión a la base** (probar
   y guardar, sin recompilar).

## 3. Conexión flexible (la base donde sea, sin tocar código)

El central resuelve la cadena de conexión con esta prioridad:

1. **Variable de entorno** `ConnectionStrings__PostgreSQL` (o `PETROLRIOS_DB`) — ideal para
   Docker, servicios y nube.
2. **Archivo** `config/connection.json` — lo escribe el botón **Guardar** de Ajustes.
3. **`appsettings`** — solo desarrollo.

Desde **Ajustes → Conexión a la base** (rol Administrador) se puede **ver** la conexión actual
(con la contraseña oculta), **probar** otra (campos simples o cadena cruda + SSL) y **guardarla**.
No se guarda una conexión que falla la prueba. Si una variable de entorno fija la conexión, la
interfaz lo avisa (esa variable manda sobre el archivo).

Admite cualquier destino: otra PC de la red, el servidor de la empresa o una base gestionada en
la nube (AWS RDS, Neon, Supabase…), ajustando host/puerto/SSL o pegando la cadena completa.

## 4. Topología de red (LAN e internet)

- **Misma red local (LAN):** las instancias del central apuntan a la base por su IP local.
  Es lo más simple y rápido.
- **Repartido por internet:** use una red privada/VPN (p. ej. **ZeroTier**, ya documentado en
  `CONEXION_RED.md`) para que el central y las estaciones lleguen a la base sin abrir puertos.
- **Exposición pública:** **no** exponga PostgreSQL directamente a internet. Si publica el panel,
  ponga un **reverse proxy con TLS** (Nginx/Caddy/Traefik) delante del central y deje
  `Seguridad__ForzarHttps` activo detrás del proxy.

## 5. Agente y monitor de estación (también multiplataforma)

Son .NET 9 y se conectan al Firebird `CONTAC.FDB` por TCP (solo lectura). Opciones:

- **Windows (lo habitual en las estaciones):** use los instaladores existentes en
  `ejecutables/4-PUBLICACION/` (`publicar.bat` genera los `.exe` self-contained; los `.iss` de
  Inno Setup instalan agente y monitor; `instalar_monitor_servicio.bat` lo deja como servicio).
- **Linux/macOS:** publique self-contained con `dotnet publish -c Release -r <rid>` (por ejemplo
  `linux-x64`, `osx-arm64`) y configure la conexión Firebird y la URL del central.

El agente apunta al central por su URL; tolera caídas (store-and-forward) y reintenta.

## 6. Seguridad (lista de verificación)

- [ ] `POSTGRES_PASSWORD` y `JWT_SECRET` robustos en `.env` (nunca los de ejemplo). El central
      en Producción **rechaza** arrancar con una clave JWT débil o la de desarrollo.
- [ ] `.env` y `config/connection.json` quedan **fuera de git** (ya en `.gitignore`).
- [ ] PostgreSQL **no** expuesto a internet; solo LAN o VPN.
- [ ] TLS por reverse proxy si el panel sale a internet.
- [ ] Cambie la contraseña inicial del Administrador en el primer ingreso (el sistema la exige).

## 7. Sin Docker (binarios nativos por sistema)

Si prefiere no usar Docker en alguna máquina:

```bash
# Central (elija el RID del SO destino): win-x64 | linux-x64 | osx-x64 | osx-arm64
dotnet publish src/PetrolRios.Api/PetrolRios.Api.csproj -c Release -r linux-x64 --self-contained -o ./publish-central
```

El frontend compilado (`frontend/dist`) debe copiarse a `publish-central/wwwroot` para que el
central sirva el panel (un solo proceso). Configure la conexión por variable de entorno
`ConnectionStrings__PostgreSQL` o por `config/connection.json`, y `Jwt__SecretKey`. PostgreSQL
puede instalarse nativo en cualquier SO o correr en Docker solo para la base
(`docker-compose.db.yml`).

## 8. Primer arranque

En el primer arranque, con una conexión válida, el central **aplica las migraciones
automáticamente** y siembra roles, estaciones, reglas por defecto y la cuenta de Administrador.
No hay pasos manuales de base de datos.
