# Start all Vessel ETA services
# This script starts each service in a separate PowerShell window

Write-Host "=== Starting Vessel ETA Prediction Services ===" -ForegroundColor Cyan
Write-Host ""

# Check if Kafka is running
Write-Host "Checking Kafka..." -ForegroundColor Yellow
$kafkaRunning = netstat -ano | findstr ":9092"
if (-not $kafkaRunning) {
    Write-Host "⚠ Kafka is not running. Please start it first with:" -ForegroundColor Yellow
    Write-Host "  docker-compose up -d zookeeper kafka kafka-ui" -ForegroundColor White
    Write-Host ""
    $response = Read-Host "Do you want to start Kafka now? (y/n)"
    if ($response -eq 'y') {
        Write-Host "Starting Kafka..." -ForegroundColor Yellow
        docker-compose up -d zookeeper kafka kafka-ui
        Write-Host "Waiting for Kafka to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 10
    } else {
        Write-Host "Please start Kafka manually before running the services." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✓ Kafka is running" -ForegroundColor Green
}

Write-Host ""
Write-Host "Starting services..." -ForegroundColor Yellow
Write-Host ""

# Start AIS Ingestion Service
Write-Host "Starting AIS Ingestion Service..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'AIS Ingestion Service' -ForegroundColor Green; dotnet run --project src/Services/VesselETA.AisIngestion/VesselETA.AisIngestion.csproj"

Start-Sleep -Seconds 2

# Start ETA Engine
Write-Host "Starting ETA Engine..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'ETA Engine' -ForegroundColor Green; dotnet run --project src/Services/VesselETA.EtaEngine/VesselETA.EtaEngine.csproj"

Start-Sleep -Seconds 2

# Start API Gateway
Write-Host "Starting API Gateway..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; Write-Host 'API Gateway' -ForegroundColor Green; dotnet run --project src/Services/VesselETA.ApiGateway/VesselETA.ApiGateway.csproj --urls http://localhost:5000"

Write-Host ""
Write-Host "=== Services Starting ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Services are starting in separate windows. Please wait a few seconds for them to initialize." -ForegroundColor Yellow
Write-Host ""
Write-Host "Once started, you can access:" -ForegroundColor Green
Write-Host "- API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "- Swagger UI: http://localhost:5000/swagger" -ForegroundColor White
Write-Host "- Kafka UI: http://localhost:8080" -ForegroundColor White
Write-Host ""
Write-Host "To stop services, close the PowerShell windows or press Ctrl+C in each window." -ForegroundColor Yellow
Write-Host ""