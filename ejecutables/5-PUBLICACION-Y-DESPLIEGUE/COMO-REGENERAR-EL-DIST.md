# Cómo regenerar el `dist` (publicar los ejecutables) — paso a paso

> Úsalo cada vez que hagas cambios en el código y necesites generar de nuevo los
> ejecutables para producción (agente, servidor y monitor). Está pensado para que
> lo hagas tú solo, sin ayuda. Sigue los pasos **en orden**.

Los ejecutables son **self-contained**: la estación NO necesita tener .NET instalado.
La carpeta `dist` está en `.gitignore` (no se sube a git) y es **100 % regenerable**:
puedes borrarla cuando quieras, estos scripts la vuelven a crear.

---

## 0. Requisitos (una sola vez)

- **.NET SDK 9** instalado. Verifícalo abriendo una terminal y escribiendo:
  ```
  dotnet --version
  ```
  Debe responder `9.x.x`.
- (Opcional) **Inno Setup 6**, solo si además quieres generar instaladores `.exe`
  con asistente. Si no lo tienes, no pasa nada: se generan las carpetas portables igual.

---

## 1. Sube el número de versión (si este release lo amerita)

La versión es **única para todo el sistema** y vive en un solo archivo:

- Abre **`Directory.Build.props`** (en la raíz del proyecto) y sube `<Version>`:
  ```xml
  <Version>2.4.0</Version>   <!-- antes 2.3.0 -->
  ```
- Ese número fluye solo al `.exe`, al heartbeat del agente, al manifiesto de
  actualización y a los instaladores.
- **Regla:** si solo recompilas **sin** cambiar la versión, los agentes que ya estén
  instalados en las estaciones **no se enterarán** de que hay una versión nueva
  (ver §5). Para un primer despliegue no importa; para una actualización, sí.

---

## 2. Detén los servicios (para que no queden archivos "en uso")

Si hay un agente / servidor / monitor corriendo **desde `dist`** o instalado como
**servicio de Windows**, bloquea sus `.exe` y el borrado de `dist` falla. Ciérralos
o, en una terminal, ejecuta:

```
taskkill /F /IM PetrolRios.Api.exe
taskkill /F /IM PetrolRios.StationAgent.exe
taskkill /F /IM PetrolRios.StationMonitor.exe
```

(Si alguno no estaba corriendo, dirá "no encontrado" — es normal, continúa.)

---

## 3. Publica los AGENTES (los 4 sistemas operativos) — **va PRIMERO**

Doble clic en:

```
scripts\publicar_agente.bat
```

- ⚠️ **Este script borra TODA la carpeta `dist`** al empezar (`rmdir /S /Q dist`) y la
  regenera desde cero. Por eso tiene que ir **antes** que el del servidor/monitor.
- Tarda unos minutos (compila 4 veces, una por sistema operativo).
- Deja listo:

  | Carpeta | Para |
  |---|---|
  | `dist\agente-windows` | Windows 64-bit (las estaciones Contaplus) |
  | `dist\agente-linux` | Linux x64 |
  | `dist\agente-macos-intel` | macOS Intel |
  | `dist\agente-macos-arm` | macOS Apple Silicon (M1/M2/M3) |

  Cada carpeta incluye su `LEEME.txt`, su `agent-config.example.json` y su
  instalador de servicio (`instalar_agente_servicio.bat` en Windows, `.sh` en
  Linux/macOS).

---

## 4. Publica el SERVIDOR y el MONITOR — **va DESPUÉS**

Doble clic en:

```
ejecutables\4-PUBLICACION\publicar.bat
```

- Este **NO borra** `dist` (solo escribe sus carpetas), por eso va **después** del
  de agentes — si lo corres al revés, el de agentes borraría esto.
- Deja listo:

  | Carpeta | Para |
  |---|---|
  | `dist\PetrolRios-Servidor` | El central: API + frontend integrado (un solo `.exe`) |
  | `dist\PetrolRios-Monitor` | El monitor de problemas operativos de la estación |
  | `dist\PetrolRios-Agente` | Copia del agente (win-x64) que usa el instalador Inno |

- Si tienes **Inno Setup 6** instalado, además genera los `setup.exe` en
  `dist\instaladores\`. Si no, omite ese paso sin error.

> **ORDEN CORRECTO, siempre:** primero el paso 3 (agentes) y luego el paso 4
> (servidor/monitor). Nunca al revés.

---

## 5. (Solo si YA hay agentes instalados en estaciones y quieres que se auto-actualicen)

Para un **primer despliegue** sáltate este paso: las instalaciones nuevas ya traen la
versión nueva.

Si ya hay agentes en campo, para que se actualicen solos desde su panel:

1. Asegúrate de haber **subido la versión** en el paso 1 (si no, no detectan el cambio).
2. Calcula el SHA256 del nuevo `.exe` del agente:
   ```
   certutil -hashfile dist\agente-windows\PetrolRios.StationAgent.exe SHA256
   ```
3. Copia el `.exe` a un lugar **descargable** por las estaciones. La forma más simple:
   ponlo dentro del servidor publicado, en `dist\PetrolRios-Servidor\wwwroot\descargas\`
   (el central sirve esa carpeta como `http://IP-DEL-SERVIDOR:5170/descargas/...`).
4. Junto al `PetrolRios.Api.exe` del servidor, crea la carpeta `config` y dentro el
   archivo **`agente-version.json`** (usa `agente-version.example.json` de esta carpeta
   como plantilla):
   ```json
   {
     "version": "2.4.0",
     "url": "http://IP-DEL-SERVIDOR:5170/descargas/PetrolRios.StationAgent.exe",
     "sha256": "<el hash del paso 2, en minúsculas>",
     "notas": "Qué cambió en esta versión.",
     "obligatoria": false
   }
   ```
5. El central publica eso en `GET /api/v1/agente/version`; cada agente lo compara con
   su versión y muestra "actualización disponible" en su panel (`:5180`). Se aplica con
   un clic (descarga, verifica el hash, intercambia el `.exe` y se reinicia).

---

## 6. Verifica antes de desplegar (recomendado)

1. Entra a `dist\agente-windows`, doble clic en `PetrolRios.StationAgent.exe`.
2. Abre `http://localhost:5180` — debe cargar el panel del agente.
3. Confirma la versión: en el panel (o por el heartbeat en el central) debe verse la
   versión que pusiste en el paso 1.
4. Ciérralo cuando termines.

---

## 7. Despliega en cada estación

1. Copia a la estación **solo la carpeta de su sistema operativo** (p. ej.
   `dist\agente-windows`).
2. Lee el `LEEME.txt` de esa carpeta (tiene los detalles finos por SO).
3. Para que arranque solo con el equipo, ejecuta **como administrador** el instalador
   de servicio de esa carpeta:
   - Windows: `instalar_agente_servicio.bat`
   - Linux: `instalar_agente_servicio.sh`
   - macOS: `instalar_agente_servicio_macos.sh`
4. Configura la estación desde su panel `http://localhost:5180` (código de estación,
   URL del servidor central y conexión al Firebird local). **No** hace falta editar
   archivos a mano.

---

## Resumen rápido (cuando ya te lo sepas)

```
1. Directory.Build.props  -> sube <Version> (si aplica)
2. taskkill de los 3 .exe (si están corriendo desde dist/servicio)
3. scripts\publicar_agente.bat            (PRIMERO; borra y regenera dist)
4. ejecutables\4-PUBLICACION\publicar.bat (DESPUÉS; añade servidor y monitor)
5. (solo si hay agentes en campo) agente-version.json + .exe descargable
6. Probar dist\agente-windows\PetrolRios.StationAgent.exe -> http://localhost:5180
7. Copiar la carpeta del SO a la estación + instalar_agente_servicio.*
```
