# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# Set the working directory
WORKDIR /app

# Copy solution and project files first (for better Docker layer caching)
COPY src/MyShop.Server/MyShop.Server.csproj src/MyShop.Server/
COPY src/MyShop.Data/MyShop.Data.csproj src/MyShop.Data/
COPY src/MyShop.Plugins/MyShop.Plugins.csproj src/MyShop.Plugins/
COPY src/MyShop.Core/MyShop.Core.csproj src/MyShop.Core/

# Restore dependencies
RUN dotnet restore src/MyShop.Server/MyShop.Server.csproj

# Copy the rest of the source code
COPY src/ src/

# Build the application
RUN dotnet build src/MyShop.Server/MyShop.Server.csproj -c Release --no-restore

# Publish the application
RUN dotnet publish src/MyShop.Server/MyShop.Server.csproj -c Release --no-build -o /app/publish

# Use the official .NET 9.0 runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Set the working directory
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/publish .

# Copy email templates
COPY src/MyShop.Server/EmailTemplates/ ./EmailTemplates/

# Create a non-root user for security
RUN addgroup --system --gid 1001 dotnetgroup && \
    adduser --system --uid 1001 --ingroup dotnetgroup dotnetuser

# Change ownership of the app directory to the non-root user
RUN chown -R dotnetuser:dotnetgroup /app

# Switch to the non-root user
USER dotnetuser

# Expose ports
EXPOSE 8080
EXPOSE 10000

# Set environment variables - Render will override PORT
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000

# Health check - use PORT environment variable
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:${PORT}/api/health || exit 1

# Start the application - bind to all interfaces and use PORT env var
CMD ASPNETCORE_URLS=http://+:${PORT} dotnet MyShop.Server.dll