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

# CRITICAL: WinUI 3 requires specific publish settings for unpackaged deployment
dotnet publish $frontendProject `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:Platform=x64 `
    -p:PublishSingleFile=false `
    -p:IncludeNativeLibrariesForSelfExtract=false `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=true `
    -p:PublishReadyToRun=false `
    -p:EnableMsixTooling=false `
    -o $frontendDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Frontend build failed!" -ForegroundColor Red
    exit 1
}

# CRITICAL: Copy WinRT activation manifests for self-contained WinUI 3 deployment
# Without this manifest, the app cannot activate WinRT components and will crash with REGDB_E_CLASSNOTREG
$manifestSourceDir = Join-Path $rootDir "src\MyShop.Client\obj\x64\Release\net10.0-windows10.0.19041.0\win-x64\Manifests"
$manifestDestFile = Join-Path $frontendDir "MyShop.Client.exe.manifest"

if (Test-Path "$manifestSourceDir\app.manifest") {
    Copy-Item "$manifestSourceDir\app.manifest" $manifestDestFile -Force
    Write-Host "  [OK] Copied WinRT activation manifest" -ForegroundColor Green
} else {
    Write-Host "  WARNING: WinRT manifest not found - app may not start!" -ForegroundColor Yellow
}

# Copy appsettings.json to frontend output (since it's embedded, we need external copy for production)
$appSettingsSource = Join-Path $rootDir "src\MyShop.Client\appsettings.json"
$appSettingsDest = Join-Path $frontendDir "appsettings.json"
if (Test-Path $appSettingsSource) {
    Copy-Item $appSettingsSource $appSettingsDest -Force
    Write-Host "  [OK] Copied appsettings.json to frontend" -ForegroundColor Green
}

Write-Host "  [OK] Frontend published to: $frontendDir" -ForegroundColor Green

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

# Copy Python source files - create proper package structure
$pythonApiDir = Join-Path $pythonDir "api"
New-Item -ItemType Directory -Path $pythonApiDir -Force | Out-Null
Copy-Item -Path "$wpExtensionDir\api\*" -Destination $pythonApiDir -Recurse -Force

# Create __init__.py files if missing (required for Python packages)
$initFiles = @(
    (Join-Path $pythonDir "__init__.py"),
    (Join-Path $pythonApiDir "__init__.py"),
    (Join-Path (Join-Path $pythonApiDir "handlers") "__init__.py"),
    (Join-Path (Join-Path $pythonApiDir "models") "__init__.py"),
    (Join-Path (Join-Path $pythonApiDir "strategies") "__init__.py")
)

foreach ($initFile in $initFiles) {
    $initDir = Split-Path $initFile -Parent
    if (Test-Path $initDir) {
        if (-not (Test-Path $initFile)) {
            "" | Out-File $initFile -Encoding UTF8
            Write-Host "  Created: $initFile" -ForegroundColor Gray
        }
    }
}

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
            # Create new _pth content that enables imports properly
            $newPthContent = @"
python311.zip
.
Lib\site-packages
..\python-ml
import site
"@
            Set-Content -Path $pthFile.FullName -Value $newPthContent -Encoding ASCII
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

# Create improved launcher script
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
    
    REM Create Lib\site-packages directory
    if not exist "python-embed\Lib\site-packages" mkdir "python-embed\Lib\site-packages"
    
    cd python-embed
    
    REM Install pip first
    python.exe get-pip.py --no-warn-script-location 2>nul
    if errorlevel 1 (
        echo ERROR: Failed to install pip
        pause
        exit /b 1
    )
    
    REM Upgrade pip
    python.exe -m pip install --upgrade pip --no-warn-script-location 2>nul
    
    REM Install requirements (without TensorFlow to speed up - it's optional)
    echo Installing core dependencies...
    python.exe -m pip install fastapi uvicorn[standard] pydantic pandas numpy scikit-learn lightgbm joblib requests --no-warn-script-location
    
    if errorlevel 1 (
        echo ERROR: Failed to install Python dependencies
        pause
        exit /b 1
    )
    
    cd ..
    echo   ^> Python dependencies installed
) else (
    echo [1/4] Python dependencies OK
)

echo [2/4] Starting Python ML API (port 8000)...
cd python-ml

REM Start Python API with proper module path
start /B "" "%APP_DIR%python-embed\python.exe" -m uvicorn api.main:app --host 0.0.0.0 --port 8000 2>nul

cd ..

REM Wait for Python API to start and verify
echo   Waiting for Python API to start...
timeout /t 3 /nobreak >nul

REM Check if Python API is responding
curl -s http://localhost:8000/health >nul 2>&1
if errorlevel 1 (
    echo   WARNING: Python ML API may not be running correctly
    echo   Continuing anyway...
) else (
    echo   ^> Python ML API started successfully
)

echo [3/4] Starting Backend API (port 5228)...
cd backend
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=http://localhost:5228
start /B "" MyShop.Server.exe
cd ..

REM Wait for Backend to start
timeout /t 3 /nobreak >nul

REM Check if Backend API is responding
curl -s http://localhost:5228/health >nul 2>&1
if errorlevel 1 (
    echo   WARNING: Backend API may not be running correctly
) else (
    echo   ^> Backend API started successfully
)

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

REM Check for required WinUI dependencies
if not exist "Microsoft.Windows.SDK.NET.dll" (
    echo WARNING: Windows SDK .NET assemblies may be missing
)

REM Try to start the app with visible console for debugging
echo   ^> Launching MyShop.Client.exe...
start "" "MyShop.Client.exe"

REM Wait a bit to check if app started
timeout /t 5 /nobreak >nul

REM Check if app is running
tasklist /FI "IMAGENAME eq MyShop.Client.exe" 2>nul | find /I "MyShop.Client.exe" >nul
if errorlevel 1 (
    echo.
    echo ========================================
    echo   WARNING: Application may not have started
    echo ========================================
    echo.
    echo Troubleshooting steps:
    echo   1. Try running MyShop.Client.exe directly from:
    echo      %APP_DIR%frontend\MyShop.Client.exe
    echo.
    echo   2. Check if Windows App SDK Runtime is installed:
    echo      Download from: https://aka.ms/windowsappsdk/1.5/latest/windowsappsdk-runtime-1.5-x64.exe
    echo.
    echo   3. Check Windows Event Viewer for crash logs:
    echo      Windows Logs ^> Application
    echo.
    echo   4. Run diagnostics:
    echo      %APP_DIR%MyShop-Diagnostics.bat
    echo.
) else (
    echo   ^> MyShop application is running
)

cd ..

echo.
echo ========================================
echo   Services Status:
echo ========================================
echo.
echo Python ML API:  http://localhost:8000/docs
echo Backend API:    http://localhost:5228/swagger
echo.
echo Press any key to stop all services and exit...
pause >nul

REM Cleanup background processes
echo Stopping services...
taskkill /F /IM python.exe 2>nul
taskkill /F /IM MyShop.Server.exe 2>nul
taskkill /F /IM MyShop.Client.exe 2>nul

echo Done.
exit
'@

$launcherPath = Join-Path $publishDir "MyShop-Launcher.bat"
Set-Content -Path $launcherPath -Value $launcherScript -Encoding ASCII

Write-Host "  ? Launcher created: MyShop-Launcher.bat" -ForegroundColor Green

# Create diagnostics script
$diagScript = @'
@echo off
echo ========================================
echo   MyShop 2025 - Diagnostics
echo ========================================
echo.

set "APP_DIR=%~dp0"
cd /d "%APP_DIR%"

echo [Checking Python]
echo -----------------
if exist "python-embed\python.exe" (
    echo Python executable: FOUND
    python-embed\python.exe --version 2>nul
    if errorlevel 1 (
        echo ERROR: Python cannot execute
    )
) else (
    echo Python executable: NOT FOUND
)
echo.

echo [Checking Python Packages]
echo -------------------------
if exist "python-embed\Lib\site-packages\fastapi" (
    echo FastAPI: INSTALLED
) else (
    echo FastAPI: NOT INSTALLED
)
if exist "python-embed\Lib\site-packages\uvicorn" (
    echo Uvicorn: INSTALLED
) else (
    echo Uvicorn: NOT INSTALLED
)
echo.

echo [Checking Backend]
echo ------------------
if exist "backend\MyShop.Server.exe" (
    echo Backend executable: FOUND
) else (
    echo Backend executable: NOT FOUND
)
echo.

echo [Checking Frontend]
echo -------------------
if exist "frontend\MyShop.Client.exe" (
    echo Frontend executable: FOUND
) else (
    echo Frontend executable: NOT FOUND
)

if exist "frontend\Microsoft.WindowsAppRuntime.Bootstrap.dll" (
    echo WinAppSDK Bootstrap: FOUND
) else (
    echo WinAppSDK Bootstrap: MISSING - This is required!
)

if exist "frontend\Microsoft.Windows.SDK.NET.dll" (
    echo Windows SDK .NET: FOUND
) else (
    echo Windows SDK .NET: MISSING
)
echo.

echo [Checking Ports]
echo ----------------
netstat -an | findstr ":8000" >nul 2>&1
if errorlevel 1 (
    echo Port 8000 ^(Python API^): AVAILABLE
) else (
    echo Port 8000 ^(Python API^): IN USE
)

netstat -an | findstr ":5228" >nul 2>&1
if errorlevel 1 (
    echo Port 5228 ^(Backend^): AVAILABLE
) else (
    echo Port 5228 ^(Backend^): IN USE
)
echo.

echo [Testing APIs]
echo --------------
curl -s http://localhost:8000/health >nul 2>&1
if errorlevel 1 (
    echo Python ML API: NOT RESPONDING
) else (
    echo Python ML API: RESPONDING
)

curl -s http://localhost:5228/health >nul 2>&1
if errorlevel 1 (
    echo Backend API: NOT RESPONDING
) else (
    echo Backend API: RESPONDING
)
echo.

echo ========================================
echo   Diagnostics Complete
echo ========================================
pause
'@

$diagPath = Join-Path $publishDir "MyShop-Diagnostics.bat"
Set-Content -Path $diagPath -Value $diagScript -Encoding ASCII

Write-Host "  ? Diagnostics script created" -ForegroundColor Green

# Summary
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Build Completed Successfully!" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

Write-Host "Published to: " -NoNewline
Write-Host "$publishDir" -ForegroundColor Cyan

# List key files
Write-Host "`nKey files created:" -ForegroundColor Yellow
$keyFiles = @(
    "frontend\MyShop.Client.exe",
    "backend\MyShop.Server.exe",
    "python-embed\python.exe",
    "python-ml\api\main.py",
    "MyShop-Launcher.bat"
)

foreach ($file in $keyFiles) {
    $fullPath = Join-Path $publishDir $file
    if (Test-Path $fullPath) {
        Write-Host "  ? $file" -ForegroundColor Green
    } else {
        Write-Host "  ? $file (MISSING)" -ForegroundColor Red
    }
}

Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "  1. Test locally: Run 'publish-package\MyShop-Launcher.bat'" -ForegroundColor White
Write-Host "  2. If frontend fails, install Windows App SDK Runtime:" -ForegroundColor White
Write-Host "     https://aka.ms/windowsappsdk/1.5/latest/windowsappsdk-runtime-1.5-x64.exe" -ForegroundColor Cyan
Write-Host "  3. Install Inno Setup: https://jrsoftware.org/isdl.php" -ForegroundColor White
Write-Host "  4. Run: .\create-installer.ps1" -ForegroundColor White
Write-Host "  5. Find installer at: Output\MyShop2025-Setup.exe`n" -ForegroundColor White
