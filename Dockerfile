# ---- Build stage ----
# Usamos el SDK de .NET 8 empaquetado oficialmente por Ubuntu en Docker Hub
FROM docker.io/ubuntu:24.04 AS build
WORKDIR /src

# Instalar el SDK de .NET 8 y herramientas necesarias usando el gestor de paquetes de Ubuntu
RUN apt-get update && apt-get install -y \
    dotnet-sdk-8.0 \
    && rm -rf /var/lib/apt/lists/*

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# ---- Runtime stage ----
# Usamos el entorno de ejecución ligero de Ubuntu en Docker Hub
FROM docker.io/ubuntu:24.04 AS runtime
WORKDIR /app

# Instalar solo el ASP.NET Core Runtime para que sea ligero
RUN apt-get update && apt-get install -y \
    aspnetcore-runtime-8.0 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "TiendaApi.dll"]