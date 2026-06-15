# ---- Build stage ----
# Usamos la imagen oficial de SDK de .NET 8 mantenida por Canonical/Ubuntu en Docker Hub
FROM ubuntu.azurecr.io/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# ---- Runtime stage ----
# Usamos la versión de ejecución (runtime) de .NET 8 sobre Ubuntu
FROM ubuntu.azurecr.io/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "TiendaApi.dll"]