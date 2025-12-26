#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Start both Python ML API and C# Backend API for Forecast services
.DESCRIPTION
    This script starts:
    1. Python FastAPI (uvicorn) on port 8000
    2. C# ASP.NET Core API on port 5228
.EXAMPLE
    .\start-forecast-services.ps1
#>

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Forecast Services Startup Script" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$pythonPath = "D:\hoang\MyShop2025\WPExtension"
$csharpPath = "D:\hoang\MyShop2025\src\MyShop.Server"

Write-Host "[1/4] Checking Python dependencies..." -ForegroundColor Yellow
Set-Location $pythonPath
if (-not (Test-Path "api\requirements.txt")) {
    Write-Host "ERROR: requirements.txt not found!" -ForegroundColor Red
    exit 1
}

Write-Host "[2/4] Starting Python ML API (port 8000)..." -ForegroundColor Yellow
$pythonJob = Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd '$pythonPath'; Write-Host 'Starting Python ML API...' -ForegroundColor Green; uvicorn api.main:app --host 0.0.0.0 --port 8000"
) -PassThru

Start-Sleep -Seconds 3

Write-Host "[3/4] Building C# Backend..." -ForegroundColor Yellow
Set-Location $csharpPath
$buildResult = dotnet build --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: C# build failed!" -ForegroundColor Red
    Stop-Process -Id $pythonJob.Id -Force
    exit 1
}

Write-Host "[4/4] Starting C# Backend API (port 5228)..." -ForegroundColor Yellow
$csharpJob = Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "cd '$csharpPath'; Write-Host 'Starting C# Backend API...' -ForegroundColor Green; dotnet run"
) -PassThru

Start-Sleep -Seconds 5

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Services Started Successfully!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "Python ML API:    " -NoNewline -ForegroundColor Cyan
Write-Host "http://localhost:8000" -ForegroundColor White
Write-Host "  - Swagger UI:   " -NoNewline -ForegroundColor Cyan
Write-Host "http://localhost:8000/docs" -ForegroundColor White
Write-Host "  - Health:       " -NoNewline -ForegroundColor Cyan
Write-Host "http://localhost:8000/health" -ForegroundColor White

Write-Host "`nC# Backend API:   " -NoNewline -ForegroundColor Cyan
Write-Host "http://localhost:5228" -ForegroundColor White
Write-Host "  - Forecasts:    " -NoNewline -ForegroundColor Cyan
Write-Host "http://localhost:5228/api/v1/forecasts" -ForegroundColor White

Write-Host "`nTest the APIs:" -ForegroundColor Yellow
Write-Host "  .\test-forecast-api.ps1" -ForegroundColor White
Write-Host "  Or open: forecast-api-tests.http in VS Code" -ForegroundColor White

Write-Host "`nTo stop services, close the PowerShell windows or press Ctrl+C`n" -ForegroundColor Gray
