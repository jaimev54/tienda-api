# ---- Build stage ----
# Imagen oficial del SDK de .NET 8 en Docker Hub (mantenida por Microsoft)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# ---- Runtime stage ----
# Imagen oficial de tiempo de ejecución optimizada (Chiseled Ubuntu) en Docker Hub
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "TiendaApi.dll"]