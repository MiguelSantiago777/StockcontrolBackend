FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/StockControl.Domain/StockControl.Domain.csproj", "src/StockControl.Domain/"]
COPY ["src/StockControl.Application/StockControl.Application.csproj", "src/StockControl.Application/"]
COPY ["src/StockControl.Infrastructure/StockControl.Infrastructure.csproj", "src/StockControl.Infrastructure/"]
COPY ["src/StockControl.API/StockControl.API.csproj", "src/StockControl.API/"]
RUN dotnet restore "src/StockControl.API/StockControl.API.csproj"

COPY . .
RUN dotnet publish "src/StockControl.API/StockControl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "StockControl.API.dll"]
