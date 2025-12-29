FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/VesselETA.EtaEngine/VesselETA.EtaEngine.csproj", "src/Services/VesselETA.EtaEngine/"]
COPY ["src/Infrastructure/VesselETA.Infrastructure/VesselETA.Infrastructure.csproj", "src/Infrastructure/VesselETA.Infrastructure/"]
COPY ["src/Shared/VesselETA.Domain/VesselETA.Domain.csproj", "src/Shared/VesselETA.Domain/"]
RUN dotnet restore "src/Services/VesselETA.EtaEngine/VesselETA.EtaEngine.csproj"
COPY . .
WORKDIR "/src/src/Services/VesselETA.EtaEngine"
RUN dotnet build "VesselETA.EtaEngine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "VesselETA.EtaEngine.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "VesselETA.EtaEngine.dll"]