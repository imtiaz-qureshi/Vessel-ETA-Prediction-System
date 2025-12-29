# Deployment Guide

This guide covers deployment options for the Vessel ETA Prediction System, from local development to production environments.

## Quick Start (Local Development)

### Prerequisites
- .NET 8 SDK
- Docker and Docker Compose
- Node.js 18+ (for frontend)
- Git

### Option 1: Demo with Simulated Data

```bash
# Clone repository
git clone <repository-url>
cd vessel-eta-prediction

# Start infrastructure
docker-compose up -d zookeeper kafka kafka-ui

# Start simulator and services
docker-compose up --build ais-ingestion-simulator eta-engine api-gateway

# Start frontend (separate terminal)
cd VesselETAFrontend
npm install
ng serve
```

Access the system:
- Frontend: http://localhost:4200
- API Gateway: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Kafka UI: http://localhost:8080

### Option 2: Real AIS Data

```bash
# Set up AIS Stream API key
export AIS_STREAM_API_KEY="your-api-key-here"

# Or create .env file
echo "AIS_STREAM_API_KEY=your-api-key-here" > .env

# Start with real AIS ingestion
docker-compose up --build ais-ingestion eta-engine api-gateway
```

## Configuration

### AIS Stream API Key

1. Register at [AIS Stream](https://aisstream.io/) for a free API key
2. Configure the key using one of these methods:

**Environment Variable:**
```bash
export AIS_STREAM_API_KEY="your-api-key-here"
```

**Docker Compose Environment:**
```yaml
environment:
  - AisStream__ApiKey=${AIS_STREAM_API_KEY}
```

**appsettings.json:**
```json
{
  "AisStream": {
    "ApiKey": "your-api-key-here"
  }
}
```

### Geographic Coverage

Configure bounding boxes for AIS data collection in `appsettings.json`:

```json
{
  "AisStream": {
    "BoundingBoxes": [
      [
        [49.5, -8.0],  // Southwest corner (lat, lon)
        [61.0, 3.0]    // Northeast corner (lat, lon)
      ]
    ]
  }
}
```

Default configuration covers UK waters and major shipping lanes.

## Service-by-Service Deployment

### 1. Infrastructure Services

```bash
# Start Kafka and Zookeeper
docker-compose up -d zookeeper kafka kafka-ui
```

### 2. AIS Ingestion (Choose One)

**Simulator (for development/demo):**
```bash
cd src/Services/VesselETA.AisIngestionSimulator
dotnet run
```

**Real AIS (for production):**
```bash
cd src/Services/VesselETA.AisIngestion
export AIS_STREAM_API_KEY="your-api-key-here"
dotnet run
```

### 3. ETA Engine

```bash
cd src/Services/VesselETA.EtaEngine
dotnet run
```

### 4. API Gateway

```bash
cd src/Services/VesselETA.ApiGateway
dotnet run
```

### 5. Frontend

```bash
cd VesselETAFrontend
npm install
ng serve
```

## Production Deployment

### Docker Swarm

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.prod.yml vessel-eta
```

### Kubernetes

```yaml
# Example Kubernetes deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: ais-ingestion
spec:
  replicas: 2
  selector:
    matchLabels:
      app: ais-ingestion
  template:
    metadata:
      labels:
        app: ais-ingestion
    spec:
      containers:
      - name: ais-ingestion
        image: vessel-eta/ais-ingestion:latest
        env:
        - name: AIS_STREAM_API_KEY
          valueFrom:
            secretKeyRef:
              name: ais-secrets
              key: api-key
        - name: Kafka__BootstrapServers
          value: "kafka-service:9092"
```

### Cloud Deployment

#### Azure Container Instances

```bash
# Create resource group
az group create --name vessel-eta-rg --location eastus

# Deploy container group
az container create \
  --resource-group vessel-eta-rg \
  --file docker-compose.azure.yml
```

#### AWS ECS

```bash
# Create ECS cluster
aws ecs create-cluster --cluster-name vessel-eta-cluster

# Register task definition
aws ecs register-task-definition --cli-input-json file://task-definition.json

# Create service
aws ecs create-service \
  --cluster vessel-eta-cluster \
  --service-name vessel-eta-service \
  --task-definition vessel-eta-task
```

## Monitoring and Observability

### Health Checks

All services expose health check endpoints:

- AIS Ingestion: `GET /health`
- ETA Engine: `GET /health`
- API Gateway: `GET /health`

### Logging

Services use structured logging with Serilog:

```json
{
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/app-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

### Metrics

Monitor key metrics:

- **AIS Ingestion Rate**: Messages/second processed
- **ETA Calculation Latency**: Time from position to ETA
- **API Response Times**: REST endpoint performance
- **WebSocket Connections**: Active SignalR clients
- **Kafka Lag**: Consumer group lag monitoring

### Kafka Monitoring

Use Kafka UI (included in docker-compose):
- Topic throughput and partition distribution
- Consumer group lag and offset management
- Message inspection and debugging

## Scaling Considerations

### Horizontal Scaling

**AIS Ingestion:**
- Multiple instances with different geographic regions
- Load balance by bounding box coverage

**ETA Engine:**
- Kafka consumer groups for parallel processing
- Scale based on message throughput

**API Gateway:**
- Load balancer with sticky sessions for SignalR
- Scale based on concurrent connections

### Performance Tuning

**Kafka Configuration:**
```properties
# Increase throughput
batch.size=32768
linger.ms=10
compression.type=snappy

# Optimize for latency
acks=1
retries=3
```

**Database Considerations:**
- Current implementation uses in-memory storage
- For production, consider Redis or PostgreSQL for persistence
- Implement read replicas for high-availability

## Security

### API Security

```csharp
// Add authentication in production
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

### Network Security

- Use HTTPS for all external communication
- Implement API rate limiting
- Configure CORS appropriately for production
- Use secrets management for API keys

### Container Security

```dockerfile
# Use non-root user
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Scan for vulnerabilities
RUN apt-get update && apt-get upgrade -y
```

## Troubleshooting

### Common Issues

**Kafka Connection Issues:**
```bash
# Check Kafka connectivity
docker exec -it kafka kafka-topics --bootstrap-server localhost:9092 --list

# View consumer groups
docker exec -it kafka kafka-consumer-groups --bootstrap-server localhost:9092 --list
```

**AIS Stream Connection Issues:**
```bash
# Check API key configuration
docker logs ais-ingestion | grep "API key"

# Monitor WebSocket connection
docker logs ais-ingestion | grep "WebSocket"
```

**Memory Issues:**
```bash
# Monitor container resource usage
docker stats

# Adjust memory limits in docker-compose.yml
deploy:
  resources:
    limits:
      memory: 1G
```

### Log Analysis

```bash
# View service logs
docker-compose logs -f ais-ingestion
docker-compose logs -f eta-engine
docker-compose logs -f api-gateway

# Search for errors
docker-compose logs | grep ERROR

# Monitor real-time logs
docker-compose logs -f --tail=100
```

## Backup and Recovery

### Data Backup

```bash
# Backup Kafka topics
kafka-console-consumer --bootstrap-server localhost:9092 \
  --topic raw-ais-positions --from-beginning > ais-backup.json

# Backup configuration
tar -czf config-backup.tar.gz src/*/appsettings.json docker-compose.yml
```

### Disaster Recovery

```bash
# Restore Kafka topics
kafka-console-producer --bootstrap-server localhost:9092 \
  --topic raw-ais-positions < ais-backup.json

# Restore services
docker-compose down
docker-compose up --build
```

## Performance Benchmarks

### Expected Performance

| Component | Throughput | Latency | Resource Usage |
|-----------|------------|---------|----------------|
| AIS Simulator | 1000+ msg/sec | <100ms | 256MB RAM |
| Real AIS Ingestion | Variable | <500ms | 512MB RAM |
| ETA Engine | 500+ predictions/sec | <2s | 512MB RAM |
| API Gateway | 10k+ req/sec | <50ms | 256MB RAM |

### Load Testing

```bash
# Install artillery for load testing
npm install -g artillery

# Test API endpoints
artillery quick --count 100 --num 10 http://localhost:5000/api/vessels

# Test WebSocket connections
artillery run websocket-test.yml
```

## Support and Maintenance

### Regular Maintenance Tasks

1. **Log Rotation**: Configure log rotation to prevent disk space issues
2. **Kafka Topic Cleanup**: Set retention policies for Kafka topics
3. **Container Updates**: Regularly update base images for security
4. **Performance Monitoring**: Monitor resource usage and scaling needs
5. **API Key Rotation**: Rotate AIS Stream API keys periodically

### Monitoring Alerts

Set up alerts for:
- Service health check failures
- High memory/CPU usage
- Kafka consumer lag
- API error rates
- WebSocket connection drops

For additional support, refer to the project documentation or create an issue in the repository.