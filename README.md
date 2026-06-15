# DarkStore - .NET 8 Web API

## Requisitos
- .NET 8 SDK
- PostgreSQL instalado localmente

## Configuracion local

1. Edita appsettings.json con tus datos de PostgreSQL:
   "DefaultConnection": "Host=localhost;Port=5432;Database=tienda_db;Username=postgres;Password=TU_PASSWORD"

2. Instala herramientas EF Core (solo una vez):
   dotnet tool install --global dotnet-ef

3. Crea la migracion inicial:
   dotnet ef migrations add InitialCreate

4. Aplica la migracion (crea la BD y los datos de prueba):
   dotnet ef database update

5. Ejecuta la API:
   dotnet run

6. Abre Swagger en: http://localhost:5000/swagger

## Usuarios de prueba
- Admin:   admin@tienda.com   / Admin123!
- Cliente: cliente@tienda.com / Cliente123!
- superbase : Cliente__123!

## Despliegue en Render
1. Sube a GitHub
2. Crea nuevo Web Service en render.com
3. Build command:  dotnet publish -c Release -o out
4. Start command:  dotnet out/TiendaApi.dll
5. Agrega variable de entorno: ASPNETCORE_ENVIRONMENT=Production
6. Edita appsettings.Production.json con la URL de Supabase
