# ---- Build stage ----
# Cambiado a la imagen oficial de Microsoft en Docker Hub
FROM docker.io/microsoft/dotnet-sdk:8.0 AS build
WORKDIR /src

COPY *.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app

# ---- Runtime stage ----
# Cambiado a la imagen oficial de Microsoft en Docker Hub
FROM docker.io/microsoft/dotnet-aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "TiendaApi.dll"]