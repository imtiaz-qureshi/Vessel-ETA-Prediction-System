# Vessel ETA Prediction System - Setup Script
# This script helps set up the development environment

Write-Host "=== Vessel ETA Prediction System Setup ===" -ForegroundColor Cyan
Write-Host ""

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Red
    exit 1
}

# Check Docker
Write-Host "Checking Docker..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version
    Write-Host "✓ Docker found: $dockerVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker not found. Please install Docker Desktop from https://www.docker.com/products/docker-desktop" -ForegroundColor Red
    exit 1
}

# Restore NuGet packages
Write-Host ""
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore VesselETA.sln
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Packages restored successfully" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Build solution
Write-Host ""
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build VesselETA.sln --configuration Debug
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Build successful" -ForegroundColor Green
} else {
    Write-Host "✗ Build failed" -ForegroundColor Red
    exit 1
}

# Create data directory
Write-Host ""
Write-Host "Creating data directory..." -ForegroundColor Yellow
if (-not (Test-Path "data")) {
    New-Item -ItemType Directory -Path "data" | Out-Null
    Write-Host "✓ Data directory created" -ForegroundColor Green
} else {
    Write-Host "✓ Data directory already exists" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Setup Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Start infrastructure: docker-compose up -d zookeeper kafka kafka-ui" -ForegroundColor White
Write-Host "2. Run services locally or with Docker Compose" -ForegroundColor White
Write-Host "3. Access API at http://localhost:5000" -ForegroundColor White
Write-Host "4. Access Kafka UI at http://localhost:8080" -ForegroundColor White
Write-Host ""