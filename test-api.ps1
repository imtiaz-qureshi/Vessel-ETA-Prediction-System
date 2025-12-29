# Simple API test script for Vessel ETA Prediction System
# Run this after starting the services to verify everything works

$baseUrl = "http://localhost:5000"

Write-Host "=== Testing Vessel ETA Prediction API ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Get all ports
Write-Host "Test 1: Getting all ports..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/ports" -Method Get
    Write-Host "✓ Found $($response.Count) ports" -ForegroundColor Green
    $response | ForEach-Object { Write-Host "  - $($_.name) ($($_.portCode))" -ForegroundColor White }
} catch {
    Write-Host "✗ Failed to get ports: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 2: Get specific port
Write-Host "Test 2: Getting Felixstowe port details..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/ports/FXT" -Method Get
    Write-Host "✓ Port details retrieved" -ForegroundColor Green
    Write-Host "  - Name: $($response.name)" -ForegroundColor White
    Write-Host "  - Location: $($response.latitude), $($response.longitude)" -ForegroundColor White
    Write-Host "  - Tidal: $($response.isTidal)" -ForegroundColor White
} catch {
    Write-Host "✗ Failed to get port details: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Get all vessels
Write-Host "Test 3: Getting all vessels..." -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/vessels" -Method Get
    Write-Host "✓ Found $($response.Count) vessels" -ForegroundColor Green
    $response | ForEach-Object { 
        Write-Host "  - $($_.mmsi) → $($_.portCode) (ETA: $($_.estimatedArrivalUtc), Risk: $($_.delayRisk))" -ForegroundColor White 
    }
} catch {
    Write-Host "✗ Failed to get vessels: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 4: Get specific vessel ETA (if vessels exist)
Write-Host "Test 4: Getting specific vessel ETA..." -ForegroundColor Yellow
try {
    $vessels = Invoke-RestMethod -Uri "$baseUrl/api/vessels" -Method Get
    if ($vessels.Count -gt 0) {
        $mmsi = $vessels[0].mmsi
        $response = Invoke-RestMethod -Uri "$baseUrl/api/vessels/$mmsi/eta" -Method Get
        Write-Host "✓ ETA retrieved for vessel $mmsi" -ForegroundColor Green
        Write-Host "  - Port: $($response.portCode)" -ForegroundColor White
        Write-Host "  - ETA: $($response.estimatedArrivalUtc)" -ForegroundColor White
        Write-Host "  - Distance: $($response.distanceNauticalMiles) nm" -ForegroundColor White
        Write-Host "  - Risk: $($response.delayRisk)" -ForegroundColor White
    } else {
        Write-Host "⚠ No vessels found (services may still be starting up)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✗ Failed to get vessel ETA: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 5: Check Swagger UI
Write-Host "Test 5: Checking Swagger UI..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger" -Method Get -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Swagger UI is accessible at $baseUrl/swagger" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Swagger UI not accessible: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Additional endpoints to try:" -ForegroundColor Yellow
Write-Host "- GET $baseUrl/api/ports/{portCode}/vessels" -ForegroundColor White
Write-Host "- GET $baseUrl/api/vessels/{mmsi}/history" -ForegroundColor White
Write-Host "- SignalR Hub: $baseUrl/hubs/eta" -ForegroundColor White
Write-Host ""