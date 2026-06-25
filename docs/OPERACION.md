# Operación — PetrolRíos (corte de luz y actualizaciones)

Guía corta para el día a día: qué pasa cuando se va la luz y cómo se actualiza cada cosa **sin
reinstalar**.

## 1. ¿Qué pasa si se va la luz?

No se "daña" nada y todo vuelve solo, **si quedó instalado como toca**:

| Componente | Cómo vuelve | Requisito |
|---|---|---|
| **Central + base** (Docker) | Los contenedores reinician solos (`restart: unless-stopped`). | Activar en **Docker Desktop**: *Settings → General → "Start Docker Desktop when you log in"*. |
| **Agente** de estación | Arranca solo al prender la PC (servicio de Windows). | Instalado con `instalar_agente_servicio.bat`. |
| **Monitor** de estación | Arranca solo al prender la PC (servicio de Windows). | Instalado con `instalar_monitor_servicio.bat`. |

Además, el **agente es store-and-forward**: si el central estuvo caído, guarda las transacciones
en su carpeta `pending/` y las **reenvía** cuando el central vuelve. **No se pierden datos.**

> Verificación rápida tras un corte: abre `http://localhost:8080` (central) y el panel del agente
> en `http://localhost:5180` de cada estación. Si el central tardó en volver, el agente reintenta
> solo cada pocos segundos.

## 2. Actualizar el sistema central

El central corre en **una sola máquina**, así que se actualiza en un solo lugar:

```bash
git pull            # o copia el código nuevo
docker compose -f docker-compose.prod.yml up -d --build
```

`--build` reconstruye la imagen con el código nuevo y reinicia el contenedor. **No se reinstala
nada** y la base (su volumen `pgdata`) se conserva. Caída de servicio: segundos.

## 3. Actualizar los agentes de estación (las 10, sin ir a cada una)

El agente tiene **auto-actualizador**. El flujo es: publicas **una** versión nueva y cada agente
la baja, verifica el checksum, intercambia su `.exe` y reinicia su servicio — solo.

Pasos para publicar una versión:

1. Sube el número en `Directory.Build.props` (`<Version>`).
2. Corre `ejecutables/5-PUBLICACION-Y-DESPLIEGUE/publicar-servidor-agente-y-monitor.bat` → genera los `.exe` en `dist/`.
3. Publica el **manifiesto** y el binario en el feed. El agente lee por defecto
   `{central}/api/v1/agente/version`; usa `agente-version.example.json` como plantilla:
   ```json
   { "version": "2.3.0", "url": "https://.../PetrolRios.StationAgent.exe",
     "sha256": "<checksum>", "notas": "Mejoras", "obligatoria": false }
   ```
4. Listo: los agentes detectan la versión nueva y se actualizan (desde su panel `:5180`, botón de
   actualizar, o automáticamente según configuración).

El checksum SHA256 garantiza que el binario no llegó corrupto ni manipulado.

## 4. Actualizar el monitor de estación

El monitor también tiene auto-actualizador (mismo mecanismo del agente): publica la versión nueva
y el monitor la aplica desde su panel. Si prefieres, también puedes reinstalarlo con su `.exe` nuevo.

## 5. Datos útiles

- **IP del servidor** (para conectarte desde otra máquina): en el servidor corre `ipconfig`
  (Windows) o `ip a` (Linux) y usa la IPv4 de la red local (ej. `192.168.1.50`). El acceso es
  `http://<esa-ip>:8080`. El instalador guiado (`INSTALAR.bat`) te la muestra al terminar.
- **Primer ingreso:** `admin@petrolrios.com` / `Admin123!` (te obliga a cambiarla). En una base que
  ya existe, conserva la contraseña que ya pusiste.
- **Logs del central:** `docker compose -f docker-compose.prod.yml logs -f central`.
- **Detener / arrancar el central:** `docker compose -f docker-compose.prod.yml down` /
  `up -d` (los datos persisten en el volumen).
