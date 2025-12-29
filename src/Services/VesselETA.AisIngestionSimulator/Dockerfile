FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/VesselETA.AisIngestionSimulator/VesselETA.AisIngestionSimulator.csproj", "src/Services/VesselETA.AisIngestionSimulator/"]
COPY ["src/Infrastructure/VesselETA.Infrastructure/VesselETA.Infrastructure.csproj", "src/Infrastructure/VesselETA.Infrastructure/"]
COPY ["src/Shared/VesselETA.Domain/VesselETA.Domain.csproj", "src/Shared/VesselETA.Domain/"]
RUN dotnet restore "src/Services/VesselETA.AisIngestionSimulator/VesselETA.AisIngestionSimulator.csproj"
COPY . .
WORKDIR "/src/src/Services/VesselETA.AisIngestionSimulator"
RUN dotnet build "VesselETA.AisIngestionSimulator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VesselETA.AisIngestionSimulator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VesselETA.AisIngestionSimulator.dll"]