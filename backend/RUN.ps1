#!/usr/bin/env pwsh
# Digital Pass Backend Startup Script

Write-Host ""
Write-Host "╔════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Digital Pass Backend - Setup & Run Script       ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Set directory
Set-Location "c:\DATA\projects(freelance)\DigitalPass\DigitalpassFrontt\backend"

Write-Host "[1/5] Installing dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Restore failed" -ForegroundColor Red
    exit 1
}

Write-Host "[2/5] Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}

Write-Host "[3/5] Applying database migrations..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Migration warning - Database may already exist" -ForegroundColor Yellow
}

Write-Host "[4/5] Starting backend server..." -ForegroundColor Yellow
Write-Host ""
Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║  ✅ Backend is ready!                                         ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║  📍 API URL:      http://localhost:5000/api                   ║" -ForegroundColor Green
Write-Host "║  📚 Swagger UI:   http://localhost:5000/swagger               ║" -ForegroundColor Green
Write-Host "║  📖 OpenAPI:      http://localhost:5000/openapi               ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "║  Press Ctrl+C to stop the server                              ║" -ForegroundColor Green
Write-Host "║                                                                ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

dotnet run
