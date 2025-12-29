# System Architecture

## Overview

The Vessel ETA Prediction System follows a microservices architecture with event-driven communication patterns. The system is designed for scalability, reliability, and real-time processing of maritime data.

## Architecture Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Sample AIS    │    │  Live AIS       │    │  Weather API    │    │   Tidal Data    │
│   Data (CSV)    │    │  Stream API     │    │  (Open-Meteo)   │    │   (Static)      │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │                      │
          ▼                      ▼                      ▼                      ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│ AIS Ingestion   │    │ Real AIS        │    │ Weather Service │    │ Port Repository │
│ Simulator       │    │ Ingestion       │    │                 │    │                 │
└─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘    └─────────┬───────┘
          │                      │                      │                      │
          └──────────────────────┼──────────────────────┘                      │
                                 ▼                                             │
                       ┌─────────────────┐                                     │
                       │     Kafka       │                                     │
                       │ raw-ais-positions│                                     │
                       └─────────┬───────┘                                     │
                                 │                                             │
                                 ▼                                             ▼
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                           ETA Prediction Engine                                     │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐ ┌─────────────────────┐   │
│  │ Distance    │ │ Weather     │ │ Tidal Constraint    │ │ Delay Risk          │   │
│  │ Calculator  │ │ Adjustment  │ │ Processor           │ │ Assessment          │   │
│  └─────────────┘ └─────────────┘ └─────────────────────┘ └─────────────────────┘   │
└─────────────────────────────────────┬───────────────────────────────────────────────┘
                                      │
                                      ▼
                            ┌─────────────────┐
                            │     Kafka       │
                            │ eta-predictions │
                            └─────────┬───────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────────────┐
│                              API Gateway                                            │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────────────┐ ┌─────────────────────┐   │
│  │ REST API    │ │ SignalR Hub │ │ Vessel Service      │ │ ETA Stream Service  │   │
│  │ Controllers │ │             │ │                     │ │                     │   │
│  └─────────────┘ └─────────────┘ └─────────────────────┘ └─────────────────────┘   │
└─────────┬───────────────────────────────┬─────────────────────────────────────────────┘
          │                               │
          ▼                               ▼
┌─────────────────┐              ┌─────────────────┐
│ Angular Client  │              │  Mobile App     │
│ (Dashboard)     │              │  (Future)       │
└─────────────────┘              └─────────────────┘
```

## Service Responsibilities

### AIS Ingestion Services

#### AIS Ingestion Simulator
- **Purpose**: Replay sample vessel position data for demonstration and testing
- **Input**: CSV files with simulated vessel movements
- **Output**: Normalized vessel positions to Kafka
- **Technology**: .NET Worker Service, CsvHelper
- **Use Case**: Development, demos, and testing scenarios

#### Real AIS Ingestion Service
- **Purpose**: Ingest live vessel position data from AIS Stream API
- **Input**: WebSocket stream from AIS Stream (aisstream.io)
- **Output**: Normalized vessel positions to Kafka
- **Technology**: .NET Worker Service, WebSocket client, Newtonsoft.Json
- **Features**:
  - Real-time WebSocket connection to AIS Stream
  - Configurable geographic bounding boxes
  - Automatic reconnection and error handling
  - Support for Class A and Class B position reports
- **Use Case**: Production deployment with live vessel tracking

### ETA Prediction Engine
- **Purpose**: Calculate vessel ETAs with weather and tidal adjustments
- **Input**: Vessel positions from Kafka
- **Output**: ETA predictions to Kafka
- **Components**:
  - Distance Calculator (Haversine formula)
  - Weather Service integration
  - Tidal constraint processor
  - Delay risk assessment engine
- **Technology**: .NET Worker Service, HttpClient
- **Scaling**: Kafka consumer groups for parallel processing

### API Gateway
- **Purpose**: Expose system data via REST APIs and real-time streams
- **Input**: ETA predictions from Kafka
- **Output**: HTTP responses and WebSocket messages
- **Components**:
  - REST Controllers
  - SignalR Hub
  - In-memory data store
  - ETA Stream Service
- **Technology**: ASP.NET Core, SignalR
- **Scaling**: Load balancer with sticky sessions for SignalR

## Data Flow

### 1. AIS Data Ingestion

#### Simulator Flow
```
CSV File → AIS Simulator Worker → Validation → Kafka Topic (raw-ais-positions)
```

#### Real AIS Flow
```
AIS Stream API → WebSocket Connection → JSON Parsing → Validation → Kafka Topic (raw-ais-positions)
```

### 2. ETA Calculation
```
Kafka Consumer → Distance Calc → Weather API → Tidal Rules → Risk Assessment → Kafka Topic (eta-predictions)
```

### 3. API Exposure
```
Kafka Consumer → In-Memory Store → REST API / SignalR → Client Applications
```

## AIS Data Sources

### Simulator Data Source
- **Type**: CSV file replay
- **Content**: Simulated vessel movements toward UK ports
- **Vessels**: Container ships, bulk carriers, tankers
- **Routes**: Realistic approach routes to Felixstowe, Liverpool, London Gateway
- **Configuration**: Replay speed, data volume, vessel types
- **Use Case**: Development, testing, demonstrations

### Real AIS Data Source
- **Provider**: AIS Stream (aisstream.io)
- **Connection**: WebSocket (wss://stream.aisstream.io/v0/stream)
- **Coverage**: Global AIS data with configurable geographic filters
- **Message Types**: 
  - Class A Position Reports (Message Types 1, 2, 3)
  - Class B Position Reports (Message Type 18)
  - Static Data Reports (Message Types 4, 5, 24)
- **Rate Limits**: Based on API subscription tier
- **Geographic Filtering**: Configurable bounding boxes
- **Authentication**: API key required (free tier available)

### Data Quality Considerations
- **Simulator**: Consistent, predictable data for testing
- **Real AIS**: Variable quality, potential gaps, duplicate messages
- **Validation**: Both sources validate MMSI format and coordinate ranges
- **Deduplication**: Timestamp-based duplicate detection for real AIS data

## Event Schema

### Vessel Position Event
```json
{
  "mmsi": "235012345",
  "latitude": 51.5074,
  "longitude": 0.1278,
  "speedKnots": 18.5,
  "course": 45.2,
  "timestampUtc": "2024-12-20T10:30:00Z",
  "vesselName": "CONTAINER_SHIP_1"
}
```

### ETA Prediction Event
```json
{
  "mmsi": "235012345",
  "portCode": "FXT",
  "estimatedArrivalUtc": "2024-12-20T14:30:00Z",
  "delayRisk": "Medium",
  "distanceNauticalMiles": 45.2,
  "averageSpeedKnots": 16.8,
  "weatherImpact": {
    "speedReductionFactor": 0.85,
    "conditions": "Moderate",
    "windSpeedKnots": 15.2,
    "waveHeightMeters": 1.8
  },
  "tidalConstraint": true,
  "predictionTimestampUtc": "2024-12-20T10:30:00Z"
}
```

## Scalability Patterns

### Horizontal Scaling
- **AIS Ingestion Simulator**: Multiple workers processing different CSV data sources
- **Real AIS Ingestion**: Multiple instances with different geographic bounding boxes
- **ETA Engine**: Kafka consumer groups for parallel processing
- **API Gateway**: Load balancer with multiple instances

### Data Partitioning
- **Kafka Topics**: Partitioned by vessel MMSI for ordered processing
- **Consumer Groups**: Balanced partition assignment

### Caching Strategy
- **Weather Data**: Cached for 15 minutes to reduce API calls
- **Port Data**: Static data cached in memory
- **ETA History**: In-memory sliding window (24 hours)

## Reliability Patterns

### Error Handling
- **Retry Logic**: Exponential backoff for external API calls
- **Circuit Breaker**: Prevent cascading failures
- **Dead Letter Queue**: Failed messages for manual review

### Data Consistency
- **At-Least-Once Delivery**: Kafka guarantees with manual commits
- **Idempotent Processing**: Handle duplicate messages gracefully
- **Event Sourcing**: Complete audit trail of ETA changes

### Monitoring
- **Health Checks**: ASP.NET Core health endpoints
- **Structured Logging**: Serilog with correlation IDs
- **Metrics**: Custom performance counters

## Security Considerations

### API Security
- **CORS**: Configured for development (restrict in production)
- **Rate Limiting**: Prevent API abuse
- **Authentication**: JWT tokens (future enhancement)

### Data Protection
- **No PII**: MMSI is public vessel identifier
- **Encryption**: HTTPS for all external communication
- **Secrets Management**: Configuration-based secrets

## Performance Characteristics

### Throughput
- **AIS Ingestion Simulator**: 1000+ positions/second (configurable replay speed)
- **Real AIS Ingestion**: Depends on AIS Stream API rate limits and geographic coverage
- **ETA Calculation**: 500+ predictions/second
- **API Gateway**: 10,000+ requests/second

### Latency
- **End-to-End (Simulator)**: < 2 seconds from CSV to API
- **End-to-End (Real AIS)**: < 3 seconds from AIS Stream to API
- **Weather API**: < 500ms average response
- **SignalR Broadcast**: < 100ms to connected clients

### Resource Usage
- **Memory**: 512MB per service instance
- **CPU**: 1 core per service instance
- **Storage**: Minimal (in-memory processing)

## Deployment Architecture

### Development
- **Local**: Docker Compose with all services
- **IDE**: Individual service debugging

### Production (Recommended)
- **Container Orchestration**: Kubernetes or Docker Swarm
- **Message Broker**: Kafka cluster (3+ nodes)
- **Load Balancer**: NGINX or cloud provider
- **Monitoring**: Prometheus + Grafana

## Future Enhancements

### Technical
- **CQRS**: Separate read/write models
- **Event Store**: Persistent event history
- **GraphQL**: Flexible API queries
- **gRPC**: High-performance service communication

### Business
- **Machine Learning**: Predictive analytics
- **Real-time AIS**: Live data integration
- **Mobile Apps**: Native iOS/Android
- **Advanced Weather**: Specialized marine forecasts