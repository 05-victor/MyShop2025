#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test Forecast API endpoints for SalesAgents
.DESCRIPTION
    Tests the new my-demand and my-revenue endpoints that use the SalesAgent's store_id
.PARAMETER Token
    JWT token for authentication (required)
.EXAMPLE
    .\test-salesagent-forecast.ps1 -Token "your-jwt-token"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$Token
)

$baseUrl = "http://localhost:5228"
$forecastUrl = "$baseUrl/api/v1/forecasts"

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [object]$Body = $null,
        [string]$AuthToken
    )
    
    Write-Host "`nTesting: $Name" -ForegroundColor Cyan
    Write-Host "  URL: $Method $Url" -ForegroundColor Gray
    
    try {
        $params = @{
            Method = $Method
            Uri = $Url
            UseBasicParsing = $true
            Headers = @{
                Authorization = "Bearer $AuthToken"
            }
        }
        
        if ($Body) {
            $params['ContentType'] = 'application/json'
            $params['Body'] = ($Body | ConvertTo-Json -Depth 10)
            Write-Host "  Body: $($params['Body'])" -ForegroundColor Gray
        }
        
        $response = Invoke-WebRequest @params
        $content = $response.Content | ConvertFrom-Json
        
        if ($content.success) {
            Write-Host "  Status: SUCCESS ?" -ForegroundColor Green
            Write-Host "  Message: $($content.message)" -ForegroundColor White
            if ($content.result) {
                Write-Host "  Result:" -ForegroundColor White
                $content.result | ConvertTo-Json -Depth 5 | Write-Host -ForegroundColor Yellow
            }
            return $true
        } else {
            Write-Host "  Status: FAILED ?" -ForegroundColor Red
            Write-Host "  Message: $($content.message)" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "  Status: ERROR ?" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "  Response: $responseBody" -ForegroundColor Red
        }
        return $false
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  SalesAgent Forecast API Test Suite" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$results = @()

Write-Host "[1] Test Demand Forecast for My Store" -ForegroundColor Yellow
$demandBody = @{
    week = "17/01/11"
    sku_id = 216418
    base_price = 111.86
    total_price = 99.04
    is_featured_sku = 0
    is_display_sku = 0
}
$results += Test-Endpoint `
    -Name "My Demand Forecast" `
    -Method POST `
    -Url "$forecastUrl/my-demand" `
    -Body $demandBody `
    -AuthToken $Token

Write-Host "`n[2] Test Revenue Forecast for My Department" -ForegroundColor Yellow
$revenueBody = @{
    date = "2012-11-02"
    strategy = "linear"
}
$results += Test-Endpoint `
    -Name "My Revenue Forecast (Linear)" `
    -Method POST `
    -Url "$forecastUrl/my-revenue" `
    -Body $revenueBody `
    -AuthToken $Token

$revenueBody2 = @{
    date = "2012-12-15"
    strategy = "dnn"
}
$results += Test-Endpoint `
    -Name "My Revenue Forecast (DNN)" `
    -Method POST `
    -Url "$forecastUrl/my-revenue" `
    -Body $revenueBody2 `
    -AuthToken $Token

Write-Host "`n[3] Test with Different Product SKU" -ForegroundColor Yellow
$demandBody2 = @{
    week = "17/02/15"
    sku_id = 123456
    base_price = 50.00
    is_featured_sku = 1
    is_display_sku = 1
}
$results += Test-Endpoint `
    -Name "My Demand Forecast (Featured Product)" `
    -Method POST `
    -Url "$forecastUrl/my-demand" `
    -Body $demandBody2 `
    -AuthToken $Token

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  Test Summary" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$passed = ($results | Where-Object { $_ -eq $true }).Count
$total = $results.Count

Write-Host "Total Tests: $total" -ForegroundColor White
Write-Host "Passed: $passed" -ForegroundColor Green
Write-Host "Failed: $($total - $passed)" -ForegroundColor $(if ($total - $passed -eq 0) { "Green" } else { "Red" })

if ($passed -eq $total) {
    Write-Host "`nAll tests passed! ?" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome tests failed. Check the output above. ??" -ForegroundColor Yellow
    exit 1
}
