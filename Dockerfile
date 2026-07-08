FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar archivos esenciales y restaurar
COPY FleetControl.sln ./
COPY src/FleetControl.Domain/*.csproj ./src/FleetControl.Domain/
COPY src/FleetControl.Application/*.csproj ./src/FleetControl.Application/
COPY src/FleetControl.Infrastructure/*.csproj ./src/FleetControl.Infrastructure/
COPY src/FleetControl.WebAPI/*.csproj ./src/FleetControl.WebAPI/

RUN dotnet restore

# Copiar todo el código y compilar
COPY . .
RUN dotnet publish src/FleetControl.WebAPI/FleetControl.WebAPI.csproj -c Release -o /out

# Imagen de producción
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

EXPOSE 8080
ENV PORT=8080

ENTRYPOINT ["dotnet", "FleetControl.WebAPI.dll"]
