@echo off
REM Docker Build and Test Script for MyShop.Server (Windows)
REM Run this script to build and test your Docker image locally

echo ?? Building MyShop.Server Docker Image...
echo ===========================================

REM Build the Docker image
docker build -t myshop-server:latest .

if %errorlevel% equ 0 (
    echo ? Docker build successful!
    echo.
    echo ?? Testing the Docker image...
    echo ===============================
    
    REM Run the container on port 8080
    echo Starting container on port 8080...
    docker run -d --name myshop-test -p 8080:8080 ^
        -e ASPNETCORE_ENVIRONMENT=Development ^
        -e PORT=8080 ^
        myshop-server:latest
    
    if %errorlevel% equ 0 (
        echo ? Container started successfully!
        
        REM Wait a few seconds for the app to start
        echo Waiting for application to start...
        timeout /t 10 /nobreak > nul
        
        REM Test the health endpoint
        echo Testing health endpoint...
        curl -f http://localhost:8080/api/health
        
        if %errorlevel% equ 0 (
            echo.
            echo ? Health check passed!
            echo.
            echo ?? Your Docker image is ready for deployment!
            echo.
            echo Next steps:
            echo 1. Push your code to GitHub
            echo 2. Deploy to Render using the provided render.yaml
            echo 3. Set up environment variables in Render dashboard
        ) else (
            echo.
            echo ? Health check failed. Check the logs:
            docker logs myshop-test
        )
        
        REM Clean up
        echo.
        echo Cleaning up test container...
        docker stop myshop-test
        docker rm myshop-test
        
    ) else (
        echo ? Failed to start container
        exit /b 1
    )
    
) else (
    echo ? Docker build failed
    exit /b 1
)

echo.
echo ???  Available commands:
echo   docker build -t myshop-server:latest .
echo   docker run -p 8080:8080 myshop-server:latest
echo   docker logs ^<container-name^>
echo.
echo ?? See DEPLOYMENT_GUIDE.md for complete deployment instructions

pause