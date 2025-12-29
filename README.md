# Vessel ETA Prediction System

A real-time maritime ETA prediction and port-operations visibility platform focused on UK ports, built with C# .NET Core, Kafka, and modern cloud-native patterns.

## Business Problem

UK ports face significant challenges with:
- Unreliable vessel ETAs leading to operational inefficiencies
- Port congestion and berth allocation issues
- Weather and tidal constraints affecting vessel schedules
- Limited real-time situational awareness for stakeholders

This system addresses these challenges by providing accurate, real-time ETA predictions with risk assessment for maritime operations.

## Architecture Overview

```
AIS Data → Ingestion Service → Kafka → ETA Engine → API Gateway → Dashboard
                                ↓
                           Weather Service
```

### Core Components

- **AIS Ingestion Services**: 
  - **Simulator**: Processes sample vessel position data for demo purposes
  - **Real AIS Ingestion**: Connects to live AIS Stream API for real vessel data
- **ETA Prediction Engine**: Calculates ETAs using distance, weather, and tidal constraints
- **API Gateway**: Exposes REST APIs and real-time WebSocket streams
- **Weather Integration**: Fetches marine weather data from Open-Meteo API
- **Delay Risk Assessment**: Evaluates arrival risk based on multiple factors

## Technology Stack

- **.NET 8**: Core framework for all services
- **Kafka**: Event streaming and message processing
- **SignalR**: Real-time web communication
- **Docker**: Containerization and orchestration
- **Serilog**: Structured logging
- **Open-Meteo API**: Weather data integration

## UK Ports Supported

- **Felixstowe (FXT)**: Container operations with tidal constraints
- **London Gateway (LGW)**: Deep-water container terminal
- **Liverpool (LIV)**: Multi-purpose port with tidal access
- **Immingham (IMM)**: Bulk cargo operations

## Quick Start

### Prerequisites

- .NET 8 SDK
- Docker and Docker Compose
- Git

### Running the System

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd vessel-eta-prediction
   ```

2. **Start infrastructure services**
   ```bash
   docker-compose up -d zookeeper kafka kafka-ui
   ```

3. **Build and run services locally**
   ```bash
   # Terminal 1 - AIS Ingestion (choose one)
   # For demo with simulated data:
   cd src/Services/VesselETA.AisIngestionSimulator
   dotnet run
   
   # For real AIS data (requires API key):
   cd src/Services/VesselETA.AisIngestion
   dotnet run

   # Terminal 2 - ETA Engine
   cd src/Services/VesselETA.EtaEngine
   dotnet run

   # Terminal 3 - API Gateway
   cd src/Services/VesselETA.ApiGateway
   dotnet run
   ```

4. **Or run everything with Docker**
   ```bash
   # For demo with simulated data:
   docker-compose up --build ais-ingestion-simulator eta-engine api-gateway
   
   # For real AIS data (set API key first):
   export AIS_STREAM_API_KEY="your-api-key-here"
   docker-compose up --build ais-ingestion eta-engine api-gateway
   ```

### Accessing the System

- **API Gateway**: http://localhost:5000
- **Swagger UI**: http://localhost:5000/swagger
- **Kafka UI**: http://localhost:8080
- **SignalR Hub**: ws://localhost:5000/hubs/eta

## API Endpoints

### Ports
- `GET /api/ports` - List all UK ports
- `GET /api/ports/{portCode}` - Get specific port details
- `GET /api/ports/{portCode}/vessels` - Get vessels heading to port

### Vessels
- `GET /api/vessels` - List all tracked vessels
- `GET /api/vessels/{mmsi}/eta` - Get latest ETA for vessel
- `GET /api/vessels/{mmsi}/history` - Get ETA history

### Real-time Updates
- **SignalR Hub**: `/hubs/eta`
- **Events**: `EtaUpdate`, `VesselEtaUpdate`, `PortEtaUpdate`

## Data Sources

### AIS Data
- **Simulator**: Sample CSV data generated for demo purposes with simulated vessel movements toward UK ports
- **Real AIS Stream**: Live vessel position data from [AIS Stream API](https://aisstream.io/)
  - Requires free API key registration
  - Configurable geographic bounding boxes
  - Supports Class A and Class B vessel position reports

### Weather Data
- **Open-Meteo Marine API**: Free marine weather forecasts
- **Data Points**: Wind speed, wave height, visibility
- **Coverage**: Global marine areas including UK waters

### Tidal Data
- Static tidal windows for demonstration
- Based on typical UK port tidal patterns
- Configurable access windows per port

## ETA Calculation Logic

1. **Distance Calculation**: Haversine formula for great-circle distance
2. **Base ETA**: Distance ÷ Current Speed
3. **Weather Adjustment**: Speed reduction based on conditions (max 30%)
4. **Tidal Constraints**: Delay arrival to next available tide window
5. **Risk Assessment**: Multi-factor analysis (weather, drift, distance, tidal)

## Delay Risk Factors

- **Weather Risk**: Based on wind speed and wave height
- **ETA Drift**: Stability of predictions over time
- **Distance Risk**: Proximity to destination port
- **Tidal Risk**: Constraints from tidal access windows

Risk levels: **LOW**, **MEDIUM**, **HIGH**

## Configuration

### Kafka Settings
```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "vessel-eta-group"
  }
}
```

### AIS Ingestion Settings

**Simulator (Demo Data)**
```json
{
  "AisDataFilePath": "Data/ais-sample.csv",
  "ReplayIntervalMs": 2000
}
```

**Real AIS Stream**
```json
{
  "AisStream": {
    "ApiKey": "your-api-key-here",
    "BoundingBoxes": [
      [
        [49.5, -8.0],  // Southwest corner (lat, lon)
        [61.0, 3.0]    // Northeast corner (lat, lon)
      ]
    ]
  }
}
```

To use real AIS data:
1. Register for a free API key at [AIS Stream](https://aisstream.io/)
2. Set the `AIS_STREAM_API_KEY` environment variable
3. Configure bounding boxes for your areas of interest

## Monitoring and Observability

- **Structured Logging**: Serilog with console output
- **Kafka Monitoring**: Kafka UI for topic and consumer monitoring
- **Health Checks**: Built-in ASP.NET Core health checks
- **Metrics**: Performance counters and custom metrics

## Development

### Project Structure
```
src/
├── Services/
│   ├── VesselETA.AisIngestionSimulator/ # Simulated AIS data for demo
│   ├── VesselETA.AisIngestion/          # Real AIS data ingestion
│   ├── VesselETA.EtaEngine/             # ETA calculation engine
│   └── VesselETA.ApiGateway/            # REST API and SignalR
├── Infrastructure/
│   └── VesselETA.Infrastructure/        # Shared infrastructure
└── Shared/
    └── VesselETA.Domain/                # Domain models
```

### Building
```bash
dotnet build VesselETA.sln
```

### Testing
```bash
dotnet test
```

## Deployment

### Docker Compose (Recommended for Demo)
```bash
docker-compose up --build
```

### Individual Services
Each service can be deployed independently with proper Kafka connectivity.

## Future Enhancements

- **Machine Learning**: Historical pattern analysis for improved predictions
- **Real AIS Integration**: Live AIS data feeds
- **Advanced Weather Models**: Integration with specialized marine weather services
- **Port Capacity Management**: Berth availability and scheduling
- **Mobile Dashboard**: React Native or Progressive Web App
- **Alerting System**: Automated notifications for high-risk arrivals

##  Highlights

This project demonstrates:

- **Event-Driven Architecture**: Kafka-based microservices
- **Real-time Processing**: Stream processing and WebSocket communication
- **Domain Modeling**: Maritime domain expertise and business logic
- **External Integration**: Weather APIs and data enrichment
- **Scalable Design**: Horizontal scaling with Kafka consumers
- **Production Patterns**: Logging, error handling, and monitoring


## Documentation

- **[Project Overview](PROJECT_OVERVIEW.md)**: Executive summary and comprehensive project description
- **[Architecture Guide](ARCHITECTURE.md)**: Detailed system architecture and design patterns
- **[API Documentation](API.md)**: REST API and SignalR real-time communication reference
- **[Deployment Guide](DEPLOYMENT.md)**: Local development and production deployment instructions
- **[Technical Specifications](__specifications.md)**: Detailed technical requirements and AI prompts
- **[Frontend README](VesselETAFrontend/README.md)**: Angular dashboard setup and development guide

## Contact

For questions about this maritime analytics system, please reach out via GitHub issues or email: imtiaz.qureshi@gmail.com.