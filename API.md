# API Documentation

This document describes the REST API and real-time communication interfaces for the Vessel ETA Prediction System.

## Base URL

- **Development**: `http://localhost:5000`
- **Production**: `https://your-domain.com`

## Authentication

Currently, the API is open for development purposes. In production, implement JWT-based authentication:

```http
Authorization: Bearer <jwt-token>
```

## REST API Endpoints

### Ports

#### Get All Ports

```http
GET /api/ports
```

**Response:**
```json
[
  {
    "code": "FXT",
    "name": "Felixstowe",
    "latitude": 51.9514,
    "longitude": 1.3062,
    "country": "United Kingdom",
    "isTidal": true,
    "tidalWindows": [
      {
        "startTime": "08:43",
        "endTime": "14:43"
      },
      {
        "startTime": "20:57",
        "endTime": "02:57"
      }
    ]
  }
]
```

#### Get Port Details

```http
GET /api/ports/{portCode}
```

**Parameters:**
- `portCode` (string): Port identifier (e.g., "FXT", "LGW", "LIV", "IMM")

**Response:**
```json
{
  "code": "FXT",
  "name": "Felixstowe",
  "latitude": 51.9514,
  "longitude": 1.3062,
  "country": "United Kingdom",
  "isTidal": true,
  "tidalWindows": [
    {
      "startTime": "08:43",
      "endTime": "14:43"
    }
  ],
  "vesselCount": 12,
  "averageEtaHours": 8.5
}
```

#### Get Vessels Approaching Port

```http
GET /api/ports/{portCode}/vessels
```

**Parameters:**
- `portCode` (string): Port identifier

**Query Parameters:**
- `limit` (int, optional): Maximum number of vessels to return (default: 50)
- `riskLevel` (string, optional): Filter by risk level ("LOW", "MEDIUM", "HIGH")

**Response:**
```json
[
  {
    "mmsi": "235012345",
    "vesselName": "CONTAINER_SHIP_1",
    "currentPosition": {
      "latitude": 51.5074,
      "longitude": 0.1278,
      "timestampUtc": "2024-12-24T10:30:00Z"
    },
    "eta": {
      "portCode": "FXT",
      "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
      "delayRisk": "MEDIUM",
      "distanceNauticalMiles": 45.2,
      "predictionTimestampUtc": "2024-12-24T10:30:00Z"
    }
  }
]
```

### Vessels

#### Get All Tracked Vessels

```http
GET /api/vessels
```

**Query Parameters:**
- `limit` (int, optional): Maximum number of vessels (default: 100)
- `hasEta` (bool, optional): Filter vessels with ETA predictions
- `portCode` (string, optional): Filter by destination port

**Response:**
```json
[
  {
    "mmsi": "235012345",
    "vesselName": "CONTAINER_SHIP_1",
    "lastPosition": {
      "latitude": 51.5074,
      "longitude": 0.1278,
      "speedKnots": 18.5,
      "course": 45.2,
      "timestampUtc": "2024-12-24T10:30:00Z"
    },
    "currentEta": {
      "portCode": "FXT",
      "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
      "delayRisk": "MEDIUM"
    }
  }
]
```

#### Get Vessel Details

```http
GET /api/vessels/{mmsi}
```

**Parameters:**
- `mmsi` (string): Maritime Mobile Service Identity

**Response:**
```json
{
  "mmsi": "235012345",
  "vesselName": "CONTAINER_SHIP_1",
  "lastPosition": {
    "latitude": 51.5074,
    "longitude": 0.1278,
    "speedKnots": 18.5,
    "course": 45.2,
    "timestampUtc": "2024-12-24T10:30:00Z"
  },
  "currentEta": {
    "portCode": "FXT",
    "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
    "delayRisk": "MEDIUM",
    "distanceNauticalMiles": 45.2,
    "averageSpeedKnots": 16.8,
    "weatherImpact": {
      "speedReductionFactor": 0.85,
      "conditions": "Moderate",
      "windSpeedKnots": 15.2,
      "waveHeightMeters": 1.8
    },
    "tidalConstraint": true,
    "predictionTimestampUtc": "2024-12-24T10:30:00Z"
  },
  "positionHistory": [
    {
      "latitude": 51.4074,
      "longitude": 0.0278,
      "speedKnots": 17.2,
      "course": 44.8,
      "timestampUtc": "2024-12-24T10:00:00Z"
    }
  ]
}
```

#### Get Vessel ETA

```http
GET /api/vessels/{mmsi}/eta
```

**Parameters:**
- `mmsi` (string): Maritime Mobile Service Identity

**Response:**
```json
{
  "mmsi": "235012345",
  "portCode": "FXT",
  "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
  "delayRisk": "MEDIUM",
  "distanceNauticalMiles": 45.2,
  "averageSpeedKnots": 16.8,
  "weatherImpact": {
    "speedReductionFactor": 0.85,
    "conditions": "Moderate",
    "windSpeedKnots": 15.2,
    "waveHeightMeters": 1.8
  },
  "tidalConstraint": true,
  "predictionTimestampUtc": "2024-12-24T10:30:00Z"
}
```

#### Get Vessel ETA History

```http
GET /api/vessels/{mmsi}/history
```

**Parameters:**
- `mmsi` (string): Maritime Mobile Service Identity

**Query Parameters:**
- `hours` (int, optional): Hours of history to retrieve (default: 24)
- `portCode` (string, optional): Filter by specific port

**Response:**
```json
[
  {
    "portCode": "FXT",
    "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
    "delayRisk": "MEDIUM",
    "distanceNauticalMiles": 45.2,
    "predictionTimestampUtc": "2024-12-24T10:30:00Z"
  },
  {
    "portCode": "FXT",
    "estimatedArrivalUtc": "2024-12-24T18:45:00Z",
    "delayRisk": "LOW",
    "distanceNauticalMiles": 48.1,
    "predictionTimestampUtc": "2024-12-24T10:00:00Z"
  }
]
```

### System Information

#### Health Check

```http
GET /health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "kafka": {
      "status": "Healthy",
      "duration": "00:00:00.0056789"
    },
    "weather_api": {
      "status": "Healthy",
      "duration": "00:00:00.0234567"
    }
  }
}
```

#### System Statistics

```http
GET /api/system/stats
```

**Response:**
```json
{
  "totalVessels": 1247,
  "vesselsWithEta": 892,
  "totalPorts": 4,
  "averageEtaAccuracy": 0.87,
  "systemUptime": "2.15:30:45",
  "messagesProcessedPerSecond": 156.7,
  "lastUpdated": "2024-12-24T10:30:00Z"
}
```

## Real-Time Communication

### SignalR Hub

The system provides real-time updates via SignalR WebSocket connection.

**Connection URL:** `/hubs/eta`

#### Client Connection (JavaScript)

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/eta")
    .build();

// Start connection
await connection.start();

// Subscribe to events
connection.on("EtaUpdate", (etaPrediction) => {
    console.log("ETA Update:", etaPrediction);
});

connection.on("VesselPositionUpdate", (vesselPosition) => {
    console.log("Position Update:", vesselPosition);
});

connection.on("PortEtaUpdate", (portCode, vessels) => {
    console.log(`Port ${portCode} Update:`, vessels);
});
```

#### Client Connection (C#)

```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hubs/eta")
    .Build();

// Subscribe to events
connection.On<EtaPrediction>("EtaUpdate", (etaPrediction) =>
{
    Console.WriteLine($"ETA Update: {etaPrediction.Mmsi} -> {etaPrediction.PortCode}");
});

connection.On<VesselPosition>("VesselPositionUpdate", (position) =>
{
    Console.WriteLine($"Position Update: {position.Mmsi} at {position.Latitude}, {position.Longitude}");
});

// Start connection
await connection.StartAsync();
```

### SignalR Events

#### EtaUpdate

Fired when a vessel's ETA prediction is updated.

**Payload:**
```json
{
  "mmsi": "235012345",
  "portCode": "FXT",
  "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
  "delayRisk": "MEDIUM",
  "distanceNauticalMiles": 45.2,
  "predictionTimestampUtc": "2024-12-24T10:30:00Z"
}
```

#### VesselPositionUpdate

Fired when a vessel's position is updated.

**Payload:**
```json
{
  "mmsi": "235012345",
  "vesselName": "CONTAINER_SHIP_1",
  "latitude": 51.5074,
  "longitude": 0.1278,
  "speedKnots": 18.5,
  "course": 45.2,
  "timestampUtc": "2024-12-24T10:30:00Z"
}
```

#### PortEtaUpdate

Fired when ETAs for vessels approaching a specific port are updated.

**Payload:**
```json
{
  "portCode": "FXT",
  "vessels": [
    {
      "mmsi": "235012345",
      "vesselName": "CONTAINER_SHIP_1",
      "estimatedArrivalUtc": "2024-12-24T18:30:00Z",
      "delayRisk": "MEDIUM"
    }
  ],
  "updateTimestamp": "2024-12-24T10:30:00Z"
}
```

## Error Handling

### HTTP Status Codes

- `200 OK`: Request successful
- `400 Bad Request`: Invalid request parameters
- `404 Not Found`: Resource not found
- `429 Too Many Requests`: Rate limit exceeded
- `500 Internal Server Error`: Server error

### Error Response Format

```json
{
  "error": {
    "code": "VESSEL_NOT_FOUND",
    "message": "Vessel with MMSI 235012345 not found",
    "details": "The specified vessel is not currently being tracked",
    "timestamp": "2024-12-24T10:30:00Z"
  }
}
```

### Common Error Codes

- `VESSEL_NOT_FOUND`: Specified vessel MMSI not found
- `PORT_NOT_FOUND`: Specified port code not found
- `INVALID_MMSI`: MMSI format is invalid
- `INVALID_PORT_CODE`: Port code format is invalid
- `NO_ETA_AVAILABLE`: No ETA prediction available for vessel
- `RATE_LIMIT_EXCEEDED`: Too many requests

## Rate Limiting

API endpoints are rate-limited to prevent abuse:

- **Default**: 100 requests per minute per IP
- **Burst**: Up to 20 requests per second
- **WebSocket**: 1000 messages per minute per connection

Rate limit headers:
```http
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1640995200
```

## Data Models

### VesselPosition

```typescript
interface VesselPosition {
  mmsi: string;
  vesselName?: string;
  latitude: number;
  longitude: number;
  speedKnots: number;
  course: number;
  timestampUtc: string;
}
```

### EtaPrediction

```typescript
interface EtaPrediction {
  mmsi: string;
  portCode: string;
  estimatedArrivalUtc: string;
  delayRisk: 'LOW' | 'MEDIUM' | 'HIGH';
  distanceNauticalMiles: number;
  averageSpeedKnots: number;
  weatherImpact?: WeatherImpact;
  tidalConstraint: boolean;
  predictionTimestampUtc: string;
}
```

### WeatherImpact

```typescript
interface WeatherImpact {
  speedReductionFactor: number;
  conditions: string;
  windSpeedKnots: number;
  waveHeightMeters: number;
}
```

### Port

```typescript
interface Port {
  code: string;
  name: string;
  latitude: number;
  longitude: number;
  country: string;
  isTidal: boolean;
  tidalWindows?: TidalWindow[];
}
```

### TidalWindow

```typescript
interface TidalWindow {
  startTime: string; // HH:mm format
  endTime: string;   // HH:mm format
}
```

## SDK and Client Libraries

### JavaScript/TypeScript

```bash
npm install @vessel-eta/client
```

```typescript
import { VesselEtaClient } from '@vessel-eta/client';

const client = new VesselEtaClient('http://localhost:5000');

// Get all vessels
const vessels = await client.getVessels();

// Subscribe to real-time updates
client.onEtaUpdate((eta) => {
  console.log('ETA Update:', eta);
});
```

### C# NuGet Package

```bash
dotnet add package VesselETA.Client
```

```csharp
using VesselETA.Client;

var client = new VesselEtaClient("http://localhost:5000");

// Get vessel ETA
var eta = await client.GetVesselEtaAsync("235012345");

// Subscribe to updates
await client.SubscribeToEtaUpdatesAsync(eta => 
{
    Console.WriteLine($"ETA Update: {eta.Mmsi}");
});
```

## Testing

### API Testing with curl

```bash
# Get all ports
curl -X GET "http://localhost:5000/api/ports"

# Get specific vessel
curl -X GET "http://localhost:5000/api/vessels/235012345"

# Get vessels approaching Felixstowe
curl -X GET "http://localhost:5000/api/ports/FXT/vessels?limit=10"
```

### Load Testing

```bash
# Install artillery
npm install -g artillery

# Run load test
artillery quick --count 100 --num 10 http://localhost:5000/api/vessels
```

### WebSocket Testing

```javascript
// Test SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/hubs/eta")
    .build();

connection.start().then(() => {
    console.log("Connected to ETA Hub");
}).catch(err => console.error(err));
```

## Changelog

### v1.0.0 (Current)
- Initial API release
- REST endpoints for ports and vessels
- SignalR real-time updates
- Support for both simulated and real AIS data
- Basic authentication placeholder

### Planned Features
- JWT authentication
- GraphQL endpoint
- Webhook notifications
- Historical data API
- Advanced filtering and search
- Mobile-optimized endpoints