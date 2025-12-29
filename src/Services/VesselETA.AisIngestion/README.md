# VesselETA Real AIS Ingestion Service

This service connects to the real AIS Stream API to ingest live vessel position data.

## Configuration

### API Key
You need to obtain an API key from [AIS Stream](https://aisstream.io/) and configure it:

1. **Environment Variable**: Set `AIS_STREAM_API_KEY` environment variable
2. **Configuration**: Set `AisStream:ApiKey` in appsettings.json
3. **Docker**: The docker-compose.yml uses the environment variable approach

### Bounding Boxes
Configure the geographic areas to monitor in `appsettings.json`:

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

Default configuration covers UK waters and major shipping lanes.

## Running the Service

### Local Development
```bash
dotnet run --project src/Services/VesselETA.AisIngestion
```

### Docker
```bash
# Set your API key
export AIS_STREAM_API_KEY="your-api-key-here"

# Run with docker-compose
docker-compose up ais-ingestion
```

## Message Types Supported

The service processes the following AIS message types:
- **PositionReport**: Standard Class A vessel position reports
- **StandardClassBPositionReport**: Class B vessel position reports

Other message types (ShipStaticData, StaticDataReport) are received but not currently processed for position data.

## Output

Processed vessel positions are published to the Kafka topic `raw-ais-positions` with the following structure:

```json
{
  "mmsi": "235012345",
  "vesselName": "EXAMPLE VESSEL",
  "latitude": 51.5074,
  "longitude": -0.1278,
  "speedKnots": 12.5,
  "course": 180.0,
  "timestampUtc": "2024-12-24T10:30:00Z"
}
```

## Monitoring

The service logs:
- Connection status to AIS Stream
- Number of messages processed
- Any errors or connection issues
- Debug information for each vessel position (when debug logging is enabled)

## Error Handling

- Automatic reconnection on WebSocket disconnection
- 30-second retry delay on connection failures
- Graceful handling of malformed JSON messages
- Continues processing even if individual messages fail to parse