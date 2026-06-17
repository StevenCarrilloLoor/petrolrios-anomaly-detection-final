# Guía de instalación y configuración — PetrolRíos

Sistema de detección de anomalías transaccionales. Consta de **dos aplicaciones**:

- **Servidor central** — la API + la aplicación web (panel para auditores, supervisores y administradores) + el motor de detección. Se instala **una vez** en un servidor.
- **Agente de estación** — un programa pequeño que se instala en **cada estación de servicio** (la máquina que tiene la base Firebird de Contaplus). Lee las transacciones y las envía al servidor central.

```
  Estación 1 ── Agente ─┐
  Estación 2 ── Agente ─┼──►  Servidor central (API + Web + PostgreSQL)  ──►  Auditores / Supervisores
  Estación N ── Agente ─┘
```

---

## 1. Requisitos

### Servidor central
- **Windows** (Server o 10/11) o Linux.
- **PostgreSQL 16** accesible (local, en red o en la nube — AWS RDS).
- No necesita tener .NET instalado si se usa el ejecutable autocontenido.
- Puertos: **5170** (API + web). PostgreSQL en **5432** (configurable).

### Agente de estación
- **Windows** en la máquina que aloja el **Firebird de Contaplus** (`CONTAC.FDB`).
- No necesita .NET instalado (ejecutable autocontenido).
- Acceso de red al servidor central (puerto 5170).
- Puerto local **5180** para su panel de control (solo accesible desde esa máquina).

---

## 2. Generar los ejecutables (publicación)

Desde el repositorio, en una máquina con el **SDK de .NET 9** y **Node.js**:

```bat
ejecutables\4-PUBLICACION\publicar.bat
```

Esto genera, sin necesidad de .NET en las máquinas destino:

- `dist\PetrolRios-Servidor\` — el servidor central (la API ya sirve la web integrada).
- `dist\PetrolRios-Agente\` — el agente de estación.
- Si tiene **Inno Setup 6** instalado, también crea los instaladores `dist\instaladores\PetrolRios-Servidor-Setup.exe` y `PetrolRios-Agente-Setup.exe`.

> Solo para el agente, también existe `scripts\publicar_agente.bat`, que genera únicamente `dist\agente\`.

> La carpeta `dist\` **no** se sube al repositorio (es un binario que se regenera). Para desplegar, regenérela con el script y copie la carpeta correspondiente.

---

## 3. Instalar el servidor central

### 3.1. Preparar PostgreSQL
Cree una base de datos y un usuario. Ejemplo:

```sql
CREATE DATABASE petrolrios;
CREATE USER petrolrios WITH PASSWORD 'UNA_CLAVE_FUERTE';
GRANT ALL PRIVILEGES ON DATABASE petrolrios TO petrolrios;
```

> El servidor **crea las tablas solo** (migraciones EF Core) y siembra los datos iniciales en el primer arranque.

### 3.2. Configurar (sin secretos quemados)
La configuración sensible **no va en el código**. Se entrega por variables de entorno o por `appsettings.Production.json` junto al ejecutable. Variables mínimas:

| Variable de entorno | Descripción |
|---|---|
| `ConnectionStrings__PostgreSQL` | Cadena de conexión a PostgreSQL |
| `ConnectionStrings__Hangfire` | Igual a la anterior (jobs) |
| `Jwt__SecretKey` | Clave JWT **robusta, ≥ 32 caracteres** (obligatoria en producción) |
| `Cors__FrontendUrl` | URL desde donde se sirve la web (si aplica) |
| `Seguridad__AdminPasswordInicial` | Contraseña inicial del admin (se obliga a cambiarla) |

Ejemplo (Windows, PowerShell):

```powershell
$env:ConnectionStrings__PostgreSQL = "Host=localhost;Port=5432;Database=petrolrios;Username=petrolrios;Password=UNA_CLAVE_FUERTE"
$env:ConnectionStrings__Hangfire   = $env:ConnectionStrings__PostgreSQL
$env:Jwt__SecretKey                = "CLAVE_LARGA_Y_ALEATORIA_DE_AL_MENOS_32_CARACTERES"
$env:Seguridad__AdminPasswordInicial = "UNA_CLAVE_INICIAL_FUERTE"
```

> **Importante:** en producción el servidor **se niega a arrancar** si `Jwt__SecretKey` falta, es corta (< 32) o es la clave de desarrollo. Es a propósito.

> Para conexiones flexibles (demo local, Docker o AWS RDS) solo cambie `ConnectionStrings__PostgreSQL`; no hay que recompilar.

### 3.3. Arrancar
```bat
dist\PetrolRios-Servidor\PetrolRios.Api.exe
```

Abra `http://IP-DEL-SERVIDOR:5170`. Verá la aplicación web.

### 3.4. Primer ingreso
- Usuario: `admin@petrolrios.com`
- Contraseña: la que puso en `Seguridad__AdminPasswordInicial` (o la de demo si no la configuró).
- El sistema **obliga a cambiar la contraseña** en el primer ingreso.

### 3.5. Crear usuarios
Como Administrador, en **Usuarios**, dé de alta a los auditores y supervisores con su rol. Roles disponibles: **Auditor**, **Supervisor**, **Administrador**.

---

## 4. Instalar el agente en cada estación

1. Copie la carpeta `dist\PetrolRios-Agente\` (o `dist\agente\`) a la computadora de la estación (la que tiene el `CONTAC.FDB`).
2. Ejecute **`PetrolRios.StationAgent.exe`**.
3. Abra en el navegador de esa máquina: **`http://localhost:5180`**.
4. **Primer arranque (setup inicial):** el panel permite la configuración inicial. Vaya a **Configuración** y complete:
   - **Identidad:** código de la estación (ej. `EST-003`) y **nombre** (ej. *Estación La Concordia*) — el nombre aparece en el panel central de Conexiones.
   - **Servidor central:** la URL (`http://IP-DEL-SERVIDOR:5170`) y las credenciales del usuario-agente.
   - **Firebird local:** ruta del `CONTAC.FDB`, host, puerto, usuario, contraseña, charset, dialect y **WireCrypt** (use **Disabled** para Firebird 2.5 / Legacy_Auth).
   - **Seguridad del panel:** defina una **contraseña local de respaldo** para poder entrar al panel aunque el servidor central no esté disponible.
   - Pulse **Probar Firebird** y **Probar servidor**, y luego **Guardar configuración**.
5. Listo: el agente empieza a enviar datos y la estación aparece **En línea** en el panel central.

### Seguridad del panel del agente
Una vez configurado, el panel **exige iniciar sesión**:
- Con una cuenta **Administrador** o **Supervisor** de PetrolRíos (se verifica contra el servidor central — el rango importa).
- Si el servidor central no está disponible, con la **contraseña local de respaldo** (cifrada con PBKDF2).

### Arranque automático con Windows (opcional)
Para que el agente arranque solo, instálelo como **servicio de Windows** o cree una **tarea programada** apuntando a `PetrolRios.StationAgent.exe`. El instalador (Inno Setup) facilita esto.

---

## 5. Notificaciones por correo (opcional)

El servidor puede enviar un email a supervisores/administradores ante **alertas críticas**. Está **apagado por defecto**. Para activarlo, configure la sección `Notificaciones:Email` (por variables de entorno o `appsettings`):

| Clave | Ejemplo |
|---|---|
| `Notificaciones__Email__Habilitado` | `true` |
| `Notificaciones__Email__Host` | `smtp.tuproveedor.com` |
| `Notificaciones__Email__Puerto` | `587` |
| `Notificaciones__Email__UsarSsl` | `true` |
| `Notificaciones__Email__Usuario` | `alertas@petrolrios.com` |
| `Notificaciones__Email__Password` | *(secreto — variable de entorno)* |
| `Notificaciones__Email__Remitente` | `alertas@petrolrios.com` |

---

## 6. Actualización remota del agente (control de versiones)

Los agentes consultan un **manifiesto de versión** y avisan en su panel cuando hay una versión nueva; se aplica **con un clic** (descarga, verifica el checksum, reemplaza el `.exe` y se reinicia).

Para publicar una nueva versión:
1. Suba el número de versión en `Directory.Build.props` y vuelva a publicar el agente.
2. Calcule el `sha256` del nuevo `PetrolRios.StationAgent.exe`.
3. Cree el archivo `config\agente-version.json` **junto al servidor central** (use `ejecutables\4-PUBLICACION\agente-version.example.json` como plantilla) con la versión, la URL de descarga y el `sha256`.
4. Los agentes lo detectan y muestran el aviso. El feed es **configurable**: por defecto el central, pero cada agente puede apuntar a una URL de GitHub si no hay servidor disponible.

---

## 7. Puertos y resumen rápido

| Componente | Puerto | Notas |
|---|---|---|
| API + Web central | 5170 | Acceso de usuarios y agentes |
| Panel del agente | 5180 | Solo localhost en la estación |
| PostgreSQL | 5432 | Configurable |
| Firebird (Contaplus) | 3050 / 3051 | Solo lectura |

| Acción | Comando |
|---|---|
| Publicar todo | `ejecutables\4-PUBLICACION\publicar.bat` |
| Publicar solo el agente | `scripts\publicar_agente.bat` |
| Arrancar todo (desarrollo) | `ejecutables\1-INICIO\INICIAR_TODO.bat` |
| Detener todo (desarrollo) | `ejecutables\1-INICIO\DETENER_TODO.bat` |

---

## 8. Solución de problemas

- **El servidor no arranca y se queja del JWT:** falta `Jwt__SecretKey` o es débil/de desarrollo. Configure una clave robusta (≥ 32 caracteres).
- **El agente no conecta a Firebird:** verifique la ruta del `.FDB`, que el servicio Firebird esté activo, el puerto y el **WireCrypt** (Disabled para FB 2.5).
- **El agente no contacta al servidor:** revise la URL/IP del central y que ambos estén en la misma red.
- **La estación no aparece En línea:** confirme que el agente está configurado y enviando; un agente está "En línea" si envió señal de vida en los últimos 3 minutos.
- **Olvidé la contraseña del admin:** un administrador puede restablecerla en **Usuarios**; si no hay acceso, vuelva a sembrar con `Seguridad__AdminPasswordInicial`.
