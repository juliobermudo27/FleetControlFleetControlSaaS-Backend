FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar solo las capas del proyecto y restaurar apuntando directo a la API
COPY src/FleetControl.Domain/*.csproj ./src/FleetControl.Domain/
COPY src/FleetControl.Application/*.csproj ./src/FleetControl.Application/
COPY src/FleetControl.Infrastructure/*.csproj ./src/FleetControl.Infrastructure/
COPY src/FleetControl.WebAPI/*.csproj ./src/FleetControl.WebAPI/

RUN dotnet restore src/FleetControl.WebAPI/FleetControl.WebAPI.csproj

# Copiar todo el código fuente restante y compilar
COPY . .
RUN dotnet publish src/FleetControl.WebAPI/FleetControl.WebAPI.csproj -c Release -o /out

# Imagen final para producción
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /out ./

EXPOSE 8080
ENV PORT=8080

ENTRYPOINT ["dotnet", "FleetControl.WebAPI.dll"]
