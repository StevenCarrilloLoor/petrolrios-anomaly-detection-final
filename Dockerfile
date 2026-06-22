# syntax=docker/dockerfile:1
# Imagen del SISTEMA CENTRAL (API + frontend en un solo servicio). Multiplataforma:
# la misma imagen corre en Windows, Linux y macOS con Docker. La base PostgreSQL vive
# aparte (en esta máquina, otra de la red o la nube) y se indica por variable de entorno
# ConnectionStrings__PostgreSQL — o desde Ajustes → Conexión a la base.

# ===== Etapa 1: build del frontend (React + Vite) =====
FROM node:22 AS frontend
WORKDIR /app/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# ===== Etapa 2: publish del backend (.NET 9) =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend
WORKDIR /src
COPY Directory.Build.props ./
COPY src/ ./src/
RUN dotnet restore src/PetrolRios.Api/PetrolRios.Api.csproj
RUN dotnet publish src/PetrolRios.Api/PetrolRios.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false
# El frontend compilado se sirve desde wwwroot: un solo servicio expone API + SPA.
COPY --from=frontend /app/frontend/dist /app/publish/wwwroot

# ===== Etapa 3: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=backend /app/publish ./
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PetrolRios.Api.dll"]
