FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/VesselETA.ApiGateway/VesselETA.ApiGateway.csproj", "src/Services/VesselETA.ApiGateway/"]
COPY ["src/Infrastructure/VesselETA.Infrastructure/VesselETA.Infrastructure.csproj", "src/Infrastructure/VesselETA.Infrastructure/"]
COPY ["src/Shared/VesselETA.Domain/VesselETA.Domain.csproj", "src/Shared/VesselETA.Domain/"]
RUN dotnet restore "src/Services/VesselETA.ApiGateway/VesselETA.ApiGateway.csproj"
COPY . .
WORKDIR "/src/src/Services/VesselETA.ApiGateway"
RUN dotnet build "VesselETA.ApiGateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VesselETA.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VesselETA.ApiGateway.dll"]