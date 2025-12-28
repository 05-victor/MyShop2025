#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test Forecast API endpoints
.DESCRIPTION
    Tests both Demand Forecast and Price Forecast endpoints
.PARAMETER Quick
    Run only health check and basic tests
.EXAMPLE
    .\test-forecast-api.ps1
    .\test-forecast-api.ps1 -Quick
#>

param(
    [switch]$Quick
)

$baseUrl = "http://localhost:5228"
$forecastUrl = "$baseUrl/api/v1/forecasts"

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null
    )
    
    Write-Host "`nTesting: $Name" -ForegroundColor Cyan
    Write-Host "  URL: $Method $Url" -ForegroundColor Gray
    
    try {
        $params = @{
            Method = $Method
            Uri = $Url
            UseBasicParsing = $true
        }
        
        if ($Body) {
            $params['ContentType'] = 'application/json'
            $params['Body'] = ($Body | ConvertTo-Json -Depth 10)
            Write-Host "  Body: $($params['Body'])" -ForegroundColor Gray
        }
        
        $response = Invoke-WebRequest @params
        $content = $response.Content | ConvertFrom-Json
        
        if ($content.success) {
            Write-Host "  Status: SUCCESS" -ForegroundColor Green
            Write-Host "  Message: $($content.message)" -ForegroundColor White
            if ($content.result) {
                Write-Host "  Result:" -ForegroundColor White
                $content.result | ConvertTo-Json -Depth 5 | Write-Host -ForegroundColor Yellow
            }
            return $true
        } else {
            Write-Host "  Status: FAILED" -ForegroundColor Red
            Write-Host "  Message: $($content.message)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "  Status: ERROR" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Forecast API Test Suite" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$results = @()

Write-Host "[1] Health Check" -ForegroundColor Yellow
$results += Test-Endpoint -Name "Forecast Service Health" -Method GET -Url "$forecastUrl/health"

if (-not $Quick) {
    Write-Host "`n[2] Demand Forecast Tests" -ForegroundColor Yellow
    
    $demandBody1 = @{
        week = "17/01/11"
        store_id = 8091
        sku_id = 216418
        base_price = 111.8625
        total_price = 99.0375
        is_featured_sku = 0
        is_display_sku = 0
    }
    $results += Test-Endpoint -Name "Demand Forecast (Full)" -Method POST -Url "$forecastUrl/demand" -Body $demandBody1
    
    $demandBody2 = @{
        week = "17/01/11"
        store_id = 8091
        sku_id = 216418
        base_price = 111.8625
        is_featured_sku = 1
        is_display_sku = 1
    }
    $results += Test-Endpoint -Name "Demand Forecast (No total_price)" -Method POST -Url "$forecastUrl/demand" -Body $demandBody2
    
    Write-Host "`n[3] Price Forecast Tests" -ForegroundColor Yellow
    
    $priceBody1 = @{
        Store = 1
        Dept = 1
        Date = "2012-11-02"
        strategy = "linear"
    }
    $results += Test-Endpoint -Name "Price Forecast (Linear)" -Method POST -Url "$forecastUrl/price" -Body $priceBody1
    
    $priceBody2 = @{
        Store = 1
        Dept = 1
        Date = "2012-11-02"
        strategy = "dnn"
    }
    $results += Test-Endpoint -Name "Price Forecast (DNN)" -Method POST -Url "$forecastUrl/price" -Body $priceBody2
    
    $priceBody3 = @{
        Store = 5
        Dept = 10
        Date = "2012-12-15"
    }
    $results += Test-Endpoint -Name "Price Forecast (Default strategy)" -Method POST -Url "$forecastUrl/price" -Body $priceBody3
} else {
    Write-Host "`n[2] Quick Test - Demand Forecast" -ForegroundColor Yellow
    $demandBody = @{
        week = "17/01/11"
        store_id = 8091
        sku_id = 216418
        base_price = 111.8625
        total_price = 99.0375
        is_featured_sku = 0
        is_display_sku = 0
    }
    $results += Test-Endpoint -Name "Demand Forecast" -Method POST -Url "$forecastUrl/demand" -Body $demandBody
    
    Write-Host "`n[3] Quick Test - Price Forecast" -ForegroundColor Yellow
    $priceBody = @{
        Store = 1
        Dept = 1
        Date = "2012-11-02"
    }
    $results += Test-Endpoint -Name "Price Forecast" -Method POST -Url "$forecastUrl/price" -Body $priceBody
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$passed = ($results | Where-Object { $_ -eq $true }).Count
$total = $results.Count

Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $($total - $passed)" -ForegroundColor $(if ($total - $passed -eq 0) { "Green" } else { "Red" })

if ($passed -eq $total) {
    Write-Host "`nAll tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome tests failed. Check the output above." -ForegroundColor Yellow
    exit 1
}
