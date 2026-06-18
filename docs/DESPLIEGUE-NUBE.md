# Guía de despliegue en la nube (PostgreSQL gestionado)

Cómo llevar la base de datos central de PetrolRíos a la nube cuando se pase de desarrollo a
producción. **Es una guía de procedimiento; no hay que implementarla ahora.** El código no
cambia: PostgreSQL es PostgreSQL, así que solo se ajusta la cadena de conexión (vía variables de
entorno) y se apunta el servidor central a la base gestionada.

---

## 1. Arquitectura en producción

```
   Estaciones (agentes)                Central (API)              Nube
  ┌───────────────────┐   VPN/ZeroTier  ┌──────────────┐  TLS   ┌────────────────────────┐
  │ Firebird CONTAC.FDB│ ───────────────▶│ PetrolRios.Api│ ─────▶│ PostgreSQL gestionado   │
  │ + Station Agent   │   o internet    │ + Hangfire    │       │ (Azure Flexible Server  │
  └───────────────────┘                 │ + SignalR     │       │  o AWS RDS)             │
                                         └──────────────┘       └────────────────────────┘
```

Punto clave de seguridad: **solo el central habla con la base en la nube.** Los agentes nunca se
conectan a la nube — solo envían sus lotes al central (por VPN o por una URL pública). Esto
mantiene la base detrás de un único punto controlado.

Dónde corre el central en producción, dos opciones:
- **Una VM** (Azure VM / AWS EC2 / la misma máquina por ZeroTier): se ejecuta como hoy, con
  `REINICIAR_CENTRAL_RED` o como servicio. Más simple, control total.
- **Un servicio gestionado** (Azure App Service / AWS App Runner): publicas el `dotnet publish`
  y la plataforma lo hospeda. Más "plug-and-play", menos mantenimiento de SO.

Para la tesis, una VM + PostgreSQL gestionado es lo más directo y económico.

---

## 2. Elegir proveedor (Azure vs AWS)

El código es idéntico en ambos (Npgsql). La decisión es por **créditos** y comodidad:

- **Azure Database for PostgreSQL – Flexible Server.** **Azure for Students** da **USD 100 de
  crédito sin tarjeta**, solo con correo académico (.edu). Ideal para una tesis.
- **AWS RDS for PostgreSQL.** El **Free Tier** incluye ~750 horas/mes de una instancia
  Single-AZ pequeña + 20 GB SSD + 20 GB de backups, por 12 meses. También útil.

Precios equivalentes rondan los ~USD 50/mes fuera del crédito gratuito (instancia de 2 vCPU /
4 GB). **Recomendación:** Azure for Students si tienes correo universitario; si no, el Free Tier
de AWS RDS. La tesis menciona AWS RDS, así que cualquiera es defendible.

---

## 3. Pasos en Azure (Database for PostgreSQL – Flexible Server)

1. **Crear el servidor.** Portal de Azure → *Azure Database for PostgreSQL flexible server* →
   *Create*. Región cercana (p. ej. *Brazil South* / *East US*), **versión 16**, tamaño
   *Burstable B1ms* (suficiente para la tesis). Define usuario administrador y contraseña.
2. **Base de datos.** Crea una base llamada `petrolrios` (o deja que las migraciones la usen).
3. **Red / firewall.**
   - *Public access*: agrega la **IP pública del central** a las reglas de firewall (y, para
     pruebas, tu IP). Marca "Allow Azure services" solo si el central corre en Azure.
   - *Private access (VNet)*: más seguro si el central también está en Azure (misma red virtual).
4. **TLS obligatorio.** Azure exige conexión cifrada; en la cadena de conexión usa
   `SslMode=Require` (o `VerifyFull` con el certificado de CA para máxima seguridad).

El host queda como `tu-servidor.postgres.database.azure.com`, puerto `5432`.

---

## 4. Cambiar la cadena de conexión (sin tocar código)

El central lee la conexión de configuración/entorno. En producción se define por **variables de
entorno** (nunca en el repo). Formato Npgsql (.NET):

```
Host=tu-servidor.postgres.database.azure.com;Port=5432;Database=petrolrios;Username=adminpr;Password=********;SslMode=Require;Trust Server Certificate=true
```

Se setea como variable de entorno (las dos cadenas que usa el sistema):

```
ConnectionStrings__PostgreSQL=Host=...;Database=petrolrios;Username=...;Password=...;SslMode=Require
ConnectionStrings__Hangfire=Host=...;Database=petrolrios;Username=...;Password=...;SslMode=Require
```

En .NET, `ConnectionStrings__PostgreSQL` (doble guion bajo) sobreescribe
`ConnectionStrings:PostgreSQL` de `appsettings.json` sin editar archivos.

> **AWS RDS** es igual: cambia solo el host por el *endpoint* de la instancia RDS
> (`xxxx.rds.amazonaws.com`) y mantén `SslMode=Require`.

---

## 5. Secretos (regla innegociable)

- **Nunca** poner la contraseña de la base en el repositorio. Va por **variable de entorno**, o
  en `appsettings.Secrets.json` (git-ignorado), o en **Azure Key Vault** / **AWS Secrets
  Manager** para producción real.
- Rotar la contraseña del administrador tras las pruebas.
- El usuario de la aplicación debería tener permisos solo sobre la base `petrolrios` (no
  superusuario).

---

## 6. Crear el esquema en la nube (migraciones)

El sistema usa EF Core Code-First. Con la cadena apuntando a la nube:

- Automático: al arrancar, el central aplica las migraciones pendientes (`Database.Migrate()`),
  así que la primera ejecución crea todas las tablas en la base gestionada.
- Manual (opcional): `dotnet ef database update -p src\PetrolRios.Infrastructure -s src\PetrolRios.Api`
  con la variable de entorno de conexión apuntando a la nube.

---

## 7. Checklist de salida a producción

- [ ] PostgreSQL 16 gestionado creado (Azure Flexible Server o AWS RDS).
- [ ] Firewall: solo la IP del central (o VNet privada) puede conectarse.
- [ ] `SslMode=Require` en ambas cadenas (PostgreSQL y Hangfire).
- [ ] Cadenas por variable de entorno / Key Vault — nada de secretos en el repo.
- [ ] Migraciones aplicadas (tablas creadas) y `SeedData` ejecutado (roles, reglas, admin).
- [ ] El central accesible por los agentes (VPN/ZeroTier o URL pública con HTTPS).
- [ ] Backups automáticos activados en el proveedor (Azure/AWS los traen por defecto).
- [ ] Contraseña de admin rotada tras las pruebas.

---

## Fuentes
- Microsoft Learn — Configure TLS connection to Azure Database for PostgreSQL: https://learn.microsoft.com/en-us/azure/postgresql/security/security-tls-how-to-connect
- Microsoft Learn — Quickstart: Create a Flexible Server instance: https://learn.microsoft.com/en-us/azure/postgresql/configure-maintain/quickstart-create-server
- Azure — Free services (Azure for Students): https://azure.microsoft.com/en-us/pricing/free-services
- AWS — Free Tier with Amazon Aurora & RDS: https://aws.amazon.com/rds/free/
- Amazon RDS for PostgreSQL pricing: https://aws.amazon.com/rds/postgresql/pricing/
