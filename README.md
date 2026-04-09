# PetrolRios - Sistema de Deteccion de Anomalias Transaccionales

Sistema web para PetrolRios S.A. que detecta anomalias transaccionales en ~13,000-15,000
transacciones diarias provenientes de 10 estaciones de servicio.

## Arquitectura

```
PetrolRios.sln
 src/
   PetrolRios.Domain/          Entidades, enums, interfaces
   PetrolRios.Application/     Casos de uso, DTOs, interfaces de repositorios
   PetrolRios.Infrastructure/  EF Core, repositorios, Firebird, Hangfire, SignalR
   PetrolRios.Api/             Controllers, JWT, middlewares
   PetrolRios.Detectors/       4 detectores (Strategy Pattern)
 tests/
   PetrolRios.Domain.Tests/
   PetrolRios.Detectors.Tests/
   PetrolRios.Api.Tests/
 frontend/                     React 18 + TypeScript + Vite + Tailwind
```

## Prerrequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker](https://www.docker.com/) (para PostgreSQL)

## Inicio rapido

### 1. Levantar PostgreSQL

```bash
docker compose up -d
```

### 2. Backend

```bash
dotnet restore
dotnet build
cd src/PetrolRios.Api
dotnet run
```

El API arranca en `http://localhost:5000`. Swagger disponible en `/swagger`.

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Accede a `http://localhost:5173`.

## Variables de entorno

Las configuraciones de desarrollo estan en `src/PetrolRios.Api/appsettings.Development.json`.
Para produccion, usa variables de entorno o `appsettings.Production.json` (no comiteado).

| Variable | Descripcion |
|---|---|
| `ConnectionStrings__PostgreSQL` | Connection string de PostgreSQL |
| `Jwt__SecretKey` | Clave secreta JWT (min. 32 caracteres) |
| `Cors__FrontendUrl` | URL del frontend |

## Pruebas

```bash
dotnet test
```

## Stack tecnologico

- **Backend:** ASP.NET Core 9.0, EF Core 9, Dapper, Hangfire, SignalR, JWT
- **Frontend:** React 18, TypeScript 5, Vite, TailwindCSS, shadcn/ui, TanStack Query, Recharts
- **BD Central:** PostgreSQL 16
- **Fuentes:** Firebird (solo lectura) via FirebirdSql.Data.FirebirdClient
