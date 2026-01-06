#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Complete automation: Build ? Compile ? Create Installer
.DESCRIPTION
    This script performs all steps to create MyShop2025-Setup.exe:
    1. Build all components
    2. Compile with Inno Setup
    3. Generate checksums
.EXAMPLE
    .\create-installer.ps1
#>

param(
    [switch]$SkipBuild,
    [switch]$OpenOutput
)

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "  MyShop2025 Complete Installer Builder" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

$rootDir = $PSScriptRoot
$outputDir = Join-Path $rootDir "Output"

# Step 1: Build components
if (-not $SkipBuild) {
    Write-Host "STEP 1: Building all components..." -ForegroundColor Cyan
    Write-Host "----------------------------------------`n" -ForegroundColor Cyan
    
    $buildScript = Join-Path $rootDir "build-for-installer.ps1"
    if (-not (Test-Path $buildScript)) {
        Write-Host "ERROR: build-for-installer.ps1 not found!" -ForegroundColor Red
        exit 1
    }
    
    & $buildScript
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`nERROR: Build failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`n? Build completed successfully`n" -ForegroundColor Green
} else {
    Write-Host "STEP 1: Skipping build (using existing publish-package)`n" -ForegroundColor Yellow
}

# Step 2: Verify Inno Setup installation
Write-Host "STEP 2: Checking Inno Setup installation..." -ForegroundColor Cyan
Write-Host "----------------------------------------`n" -ForegroundColor Cyan

$isccPaths = @(
    "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
    "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)

$iscc = $null
foreach ($path in $isccPaths) {
    if (Test-Path $path) {
        $iscc = $path
        break
    }
}

if (-not $iscc) {
    Write-Host "ERROR: Inno Setup not found!" -ForegroundColor Red
    Write-Host "`nPlease install Inno Setup from:" -ForegroundColor Yellow
    Write-Host "https://jrsoftware.org/isdl.php`n" -ForegroundColor Cyan
    
    $response = Read-Host "Open download page now? (Y/N)"
    if ($response -eq 'Y' -or $response -eq 'y') {
        Start-Process "https://jrsoftware.org/isdl.php"
    }
    exit 1
}

Write-Host "? Found Inno Setup at: $iscc`n" -ForegroundColor Green

# Step 3: Verify installer script
Write-Host "STEP 3: Verifying installer script..." -ForegroundColor Cyan
Write-Host "----------------------------------------`n" -ForegroundColor Cyan

$issFile = Join-Path $rootDir "MyShop-Installer.iss"
if (-not (Test-Path $issFile)) {
    Write-Host "ERROR: MyShop-Installer.iss not found!" -ForegroundColor Red
    exit 1
}

# Verify publish-package exists
$publishDir = Join-Path $rootDir "publish-package"
if (-not (Test-Path $publishDir)) {
    Write-Host "ERROR: publish-package directory not found!" -ForegroundColor Red
    Write-Host "Run build-for-installer.ps1 first" -ForegroundColor Yellow
    exit 1
}

# Check components
$frontendExe = Join-Path $publishDir "frontend\MyShop.Client.exe"
$backendExe = Join-Path $publishDir "backend\MyShop.Server.exe"
$pythonDir = Join-Path $publishDir "python-ml"

if (-not (Test-Path $frontendExe)) {
    Write-Host "ERROR: Frontend not found at $frontendExe" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $backendExe)) {
    Write-Host "ERROR: Backend not found at $backendExe" -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $pythonDir)) {
    Write-Host "ERROR: Python ML API not found at $pythonDir" -ForegroundColor Red
    exit 1
}

Write-Host "? Frontend: $frontendExe" -ForegroundColor Green
Write-Host "? Backend: $backendExe" -ForegroundColor Green
Write-Host "? Python ML: $pythonDir`n" -ForegroundColor Green

# Step 4: Compile installer
Write-Host "STEP 4: Compiling installer with Inno Setup..." -ForegroundColor Cyan
Write-Host "----------------------------------------`n" -ForegroundColor Cyan
Write-Host "This may take 2-5 minutes depending on your system...`n" -ForegroundColor Yellow

$startTime = Get-Date

try {
    & $iscc $issFile
    
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed with exit code $LASTEXITCODE"
    }
} catch {
    Write-Host "`nERROR: Compilation failed!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n? Compilation completed in $($duration.TotalSeconds) seconds`n" -ForegroundColor Green

# Step 5: Verify output
Write-Host "STEP 5: Verifying installer..." -ForegroundColor Cyan
Write-Host "----------------------------------------`n" -ForegroundColor Cyan

$installerPath = Join-Path $outputDir "MyShop2025-Setup.exe"
if (-not (Test-Path $installerPath)) {
    Write-Host "ERROR: Installer not found at $installerPath" -ForegroundColor Red
    exit 1
}

$fileInfo = Get-Item $installerPath
$fileSizeMB = [math]::Round($fileInfo.Length / 1MB, 2)

Write-Host "? Installer created successfully!" -ForegroundColor Green
Write-Host "  Path: $installerPath" -ForegroundColor White
Write-Host "  Size: $fileSizeMB MB`n" -ForegroundColor White

# Step 6: Generate checksum
Write-Host "STEP 6: Generating checksums..." -ForegroundColor Cyan
Write-Host "----------------------------------------`n" -ForegroundColor Cyan

$sha256 = (Get-FileHash $installerPath -Algorithm SHA256).Hash
$checksumFile = "$installerPath.sha256"

@"
MyShop2025-Setup.exe
SHA256: $sha256
Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Size: $fileSizeMB MB
"@ | Out-File $checksumFile -Encoding UTF8

Write-Host "? SHA256: $sha256" -ForegroundColor Green
Write-Host "  Saved to: $checksumFile`n" -ForegroundColor White

# Step 7: Create release info
$releaseInfoFile = Join-Path $outputDir "RELEASE-INFO.txt"
@"
========================================
MyShop 2025 - Release Information
========================================

Build Date: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
Build Duration: $($duration.TotalSeconds) seconds

Installer Details:
- File: MyShop2025-Setup.exe
- Size: $fileSizeMB MB
- SHA256: $sha256

Components:
- WinUI 3 Frontend (MyShop.Client)
- ASP.NET Core Backend (MyShop.Server)
- Python ML API (FastAPI + Uvicorn)
- Python 3.11.9 Embeddable Runtime

System Requirements:
- Windows 10/11 (64-bit)
- 2 GB RAM (recommended: 4 GB)
- 1 GB disk space
- Internet connection (first run only)

Installation:
1. Run MyShop2025-Setup.exe as Administrator
2. Follow installation wizard
3. First launch will install Python dependencies (2-5 minutes)
4. Subsequent launches are instant

Distribution:
- Upload MyShop2025-Setup.exe
- Include MyShop2025-Setup.exe.sha256 for verification
- Provide INSTALLER-GUIDE.md for users

Support:
https://github.com/05-victor/MyShop2025

========================================
"@ | Out-File $releaseInfoFile -Encoding UTF8

Write-Host "? Release info saved to: $releaseInfoFile`n" -ForegroundColor Green

# Final summary
Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "  SUCCESS! Installer Ready for Distribution" -ForegroundColor Magenta
Write-Host "========================================`n" -ForegroundColor Magenta

Write-Host "?? Installer: " -NoNewline
Write-Host "$installerPath" -ForegroundColor Cyan

Write-Host "?? Size: " -NoNewline
Write-Host "$fileSizeMB MB" -ForegroundColor Cyan

Write-Host "?? SHA256: " -NoNewline
Write-Host "$sha256" -ForegroundColor Cyan

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Test installer on a clean Windows machine" -ForegroundColor White
Write-Host "  2. Verify all features work correctly" -ForegroundColor White
Write-Host "  3. Upload to release platform (GitHub, etc.)" -ForegroundColor White
Write-Host "  4. Share INSTALLER-GUIDE.md with users`n" -ForegroundColor White

if ($OpenOutput) {
    Write-Host "Opening output directory..." -ForegroundColor Cyan
    Start-Process explorer.exe -ArgumentList $outputDir
}

# Ask to open output folder
$response = Read-Host "Open output folder now? (Y/N)"
if ($response -eq 'Y' -or $response -eq 'y') {
    Start-Process explorer.exe -ArgumentList $outputDir
}

Write-Host "`n? Done!`n" -ForegroundColor Green
