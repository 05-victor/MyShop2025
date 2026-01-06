#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and prepare all components for MyShop2025 installer
.DESCRIPTION
    This script:
    1. Publishes WinUI 3 frontend as self-contained
    2. Publishes ASP.NET Core backend as self-contained
    3. Packages Python ML API with embedded Python
    4. Creates startup launcher
#>

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  MyShop2025 Installer Build Script" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Paths
$rootDir = $PSScriptRoot
$publishDir = Join-Path $rootDir "publish-package"
$frontendDir = Join-Path $publishDir "frontend"
$backendDir = Join-Path $publishDir "backend"
$pythonDir = Join-Path $publishDir "python-ml"

# Clean previous build
Write-Host "[1/6] Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $frontendDir -Force | Out-Null
New-Item -ItemType Directory -Path $backendDir -Force | Out-Null
New-Item -ItemType Directory -Path $pythonDir -Force | Out-Null

# Build WinUI 3 Frontend
Write-Host "[2/6] Publishing WinUI 3 Frontend..." -ForegroundColor Yellow
$frontendProject = Join-Path $rootDir "src\MyShop.Client\MyShop.Client.csproj"
if (-not (Test-Path $frontendProject)) {
    Write-Host "ERROR: Frontend project not found at $frontendProject" -ForegroundColor Red
    exit 1
}

dotnet publish $frontendProject `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:Platform=x64 `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -o $frontendDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Frontend build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "  ? Frontend published to: $frontendDir" -ForegroundColor Green

# Build ASP.NET Core Backend
Write-Host "[3/6] Publishing ASP.NET Core Backend..." -ForegroundColor Yellow
$backendProject = Join-Path $rootDir "src\MyShop.Server\MyShop.Server.csproj"
if (-not (Test-Path $backendProject)) {
    Write-Host "ERROR: Backend project not found at $backendProject" -ForegroundColor Red
    exit 1
}

dotnet publish $backendProject `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o $backendDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Backend build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "  ? Backend published to: $backendDir" -ForegroundColor Green

# Package Python ML API
Write-Host "[4/6] Packaging Python ML API..." -ForegroundColor Yellow
$wpExtensionDir = Join-Path $rootDir "WPExtension"
if (-not (Test-Path $wpExtensionDir)) {
    Write-Host "ERROR: WPExtension directory not found!" -ForegroundColor Red
    exit 1
}

# Copy Python source files
Copy-Item -Path "$wpExtensionDir\api" -Destination "$pythonDir\api" -Recurse -Force

# Copy datasets (required for ML models)
if (Test-Path "$wpExtensionDir\PF dataset") {
    Copy-Item -Path "$wpExtensionDir\PF dataset" -Destination "$pythonDir\PF dataset" -Recurse -Force
    Write-Host "  ? Price Forecast dataset copied" -ForegroundColor Green
}

if (Test-Path "$wpExtensionDir\DF dataset") {
    Copy-Item -Path "$wpExtensionDir\DF dataset" -Destination "$pythonDir\DF dataset" -Recurse -Force
    Write-Host "  ? Demand Forecast dataset copied" -ForegroundColor Green
}

# Copy model files (pickle files)
Copy-Item -Path "$wpExtensionDir\*.pkl" -Destination "$pythonDir\" -Force -ErrorAction SilentlyContinue
Copy-Item -Path "$wpExtensionDir\*.csv" -Destination "$pythonDir\" -Force -ErrorAction SilentlyContinue

# Copy requirements.txt
if (Test-Path "$wpExtensionDir\api\requirements.txt") {
    Copy-Item -Path "$wpExtensionDir\api\requirements.txt" -Destination "$pythonDir\requirements.txt" -Force
}

Write-Host "  ? Python ML API packaged to: $pythonDir" -ForegroundColor Green

# Download Python Embeddable Package
Write-Host "[5/6] Downloading Python Embeddable Package..." -ForegroundColor Yellow
$pythonEmbedDir = Join-Path $publishDir "python-embed"
$pythonZip = Join-Path $publishDir "python-3.11.9-embed-amd64.zip"

if (-not (Test-Path "$pythonEmbedDir\python.exe")) {
    Write-Host "  Downloading Python 3.11.9 embeddable..." -ForegroundColor Cyan
    $pythonUrl = "https://www.python.org/ftp/python/3.11.9/python-3.11.9-embed-amd64.zip"
    
    try {
        Invoke-WebRequest -Uri $pythonUrl -OutFile $pythonZip -UseBasicParsing
        Expand-Archive -Path $pythonZip -DestinationPath $pythonEmbedDir -Force
        Remove-Item $pythonZip -Force
        
        # Enable site-packages and imports
        $pthFile = Get-ChildItem $pythonEmbedDir -Filter "python311._pth" | Select-Object -First 1
        if ($pthFile) {
            $pthContent = Get-Content $pthFile.FullName
            $newContent = @()
            
            foreach ($line in $pthContent) {
                # Uncomment import site
                if ($line -match "^#\s*import site") {
                    $newContent += "import site"
                } else {
                    $newContent += $line
                }
            }
            
            # Add Lib/site-packages path
            $newContent += "Lib\site-packages"
            $newContent += "..\python-ml"
            
            Set-Content -Path $pthFile.FullName -Value $newContent -Encoding ASCII
            Write-Host "  ? Python paths configured" -ForegroundColor Green
        }
        
        Write-Host "  ? Python embeddable downloaded" -ForegroundColor Green
    } catch {
        Write-Host "  WARNING: Failed to download Python. You need to manually download from:" -ForegroundColor Yellow
        Write-Host "  $pythonUrl" -ForegroundColor Yellow
        Write-Host "  Extract to: $pythonEmbedDir" -ForegroundColor Yellow
    }
}

# Download get-pip.py
$getPipPath = Join-Path $pythonEmbedDir "get-pip.py"
if (-not (Test-Path $getPipPath)) {
    Write-Host "  Downloading get-pip.py..." -ForegroundColor Cyan
    try {
        Invoke-WebRequest -Uri "https://bootstrap.pypa.io/get-pip.py" -OutFile $getPipPath -UseBasicParsing
        Write-Host "  ? get-pip.py downloaded" -ForegroundColor Green
    } catch {
        Write-Host "  WARNING: Failed to download get-pip.py" -ForegroundColor Yellow
    }
}

# Create startup launcher
Write-Host "[6/6] Creating startup launcher..." -ForegroundColor Yellow

# Create appsettings.Production.json for backend
$backendConfig = @'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Urls": "http://localhost:5228",
  "ForecastSettings": {
    "BaseUrl": "http://localhost:8000"
  }
}
'@

$backendConfigPath = Join-Path $backendDir "appsettings.Production.json"
Set-Content -Path $backendConfigPath -Value $backendConfig -Encoding UTF8
Write-Host "  ? Backend config created" -ForegroundColor Green

$launcherScript = @'
@echo off
setlocal enabledelayedexpansion

set "APP_DIR=%~dp0"
cd /d "%APP_DIR%"

echo.
echo ========================================
echo   MyShop 2025 - Starting Services
echo ========================================
echo.

REM Check if Python ML dependencies are installed
if not exist "python-embed\Lib\site-packages\fastapi" (
    echo [1/4] Installing Python dependencies...
    echo This may take a few minutes on first run...
    
    cd python-embed
    python.exe get-pip.py --no-warn-script-location 2>nul
    python.exe -m pip install --upgrade pip --no-warn-script-location 2>nul
    python.exe -m pip install -r ..\python-ml\requirements.txt --no-warn-script-location
    cd ..
    
    echo   ^> Python dependencies installed
) else (
    echo [1/4] Python dependencies OK
)

echo [2/4] Starting Python ML API (port 8000)...
start /B "" python-embed\python.exe -m uvicorn python-ml.api.main:app --host 0.0.0.0 --port 8000 2>nul

REM Wait for Python API to start
timeout /t 5 /nobreak >nul
echo   ^> Python ML API started

echo [3/4] Starting Backend API (port 5228)...
cd backend
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://localhost:5228
start /B "" MyShop.Server.exe
cd ..

REM Wait for Backend to start
timeout /t 3 /nobreak >nul
echo   ^> Backend API started

echo [4/4] Starting MyShop Application...
timeout /t 2 /nobreak >nul

cd frontend

REM Check if MyShop.Client.exe exists
if not exist "MyShop.Client.exe" (
    echo ERROR: MyShop.Client.exe not found!
    echo Please reinstall the application.
    pause
    exit /b 1
)

REM Try to start the app
echo   ^> Launching MyShop.Client.exe...
start "" MyShop.Client.exe

REM Wait a bit to check if app started
timeout /t 3 /nobreak >nul

REM Check if app is running
tasklist /FI "IMAGENAME eq MyShop.Client.exe" 2>nul | find /I "MyShop.Client.exe" >nul
if errorlevel 1 (
    echo.
    echo WARNING: MyShop application may not have started correctly.
    echo.
    echo Troubleshooting:
    echo   1. Check if WinAppSDK Runtime is installed
    echo   2. Run MyShop.Client.exe directly to see error messages
    echo   3. Check Windows Event Viewer for crash logs
    echo.
) else (
    echo   ^> MyShop application is running
)

cd ..

echo.
echo ========================================
echo   All services started successfully!
echo ========================================
echo.
echo Python ML API:  http://localhost:8000
echo Backend API:    http://localhost:5228
echo.
echo Press any key to exit (this will close background services)...
pause >nul

REM Cleanup background processes
taskkill /F /IM python.exe 2>nul
taskkill /F /IM MyShop.Server.exe 2>nul

exit
'@

$launcherPath = Join-Path $publishDir "MyShop-Launcher.bat"
Set-Content -Path $launcherPath -Value $launcherScript -Encoding ASCII

Write-Host "  ? Launcher created: MyShop-Launcher.bat" -ForegroundColor Green

# Summary
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Build Completed Successfully!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "Published to: " -NoNewline
Write-Host "$publishDir" -ForegroundColor Cyan

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Install Inno Setup: https://jrsoftware.org/isdl.php" -ForegroundColor White
Write-Host "  2. Open MyShop-Installer.iss with Inno Setup" -ForegroundColor White
Write-Host "  3. Click 'Build' -> 'Compile' to create installer" -ForegroundColor White
Write-Host "  4. Find installer at: Output\MyShop2025-Setup.exe`n" -ForegroundColor White
