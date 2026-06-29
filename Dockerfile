# ---- Build stage ----
# Descargamos la imagen de Ubuntu con el SDK de .NET 8 para compilar el proyecto
FROM docker.io/ubuntu:24.04 AS build
WORKDIR /src

# Instalamos el SDK de .NET 8 para poder compilar y publicar la API
RUN apt-get update && apt-get install -y \
    dotnet-sdk-8.0 \
    && rm -rf /var/lib/apt/lists/*

# Copiamos solo el archivo de proyecto primero para aprovechar el cache de Docker
COPY *.csproj .

# Restauramos los paquetes NuGet (dependencias del proyecto)
RUN dotnet restore

# Copiamos el resto del código fuente
COPY . .

# Compilamos y publicamos la API en modo Release (optimizado para producción)
RUN dotnet publish -c Release -o /app

# ---- Runtime stage ----
# Usamos una imagen limpia de Ubuntu sin el SDK (más ligera) solo para ejecutar la API
FROM docker.io/ubuntu:24.04 AS runtime
WORKDIR /app

# Instalamos solo el runtime de ASP.NET Core (no el SDK completo) para mantener la imagen ligera
# ca-certificates es necesario para que .NET pueda hacer conexiones HTTPS externas (ej. Stripe)
RUN apt-get update && apt-get install -y \
    aspnetcore-runtime-8.0 \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# Copiamos los archivos compilados desde el stage de build
COPY --from=build /app .

# Le decimos a la API en qué puerto escuchar (Render usa el 10000 por defecto)
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

# Comando que se ejecuta cuando el contenedor arranca
ENTRYPOINT ["dotnet", "TiendaApi.dll"]