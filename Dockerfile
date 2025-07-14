# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy solution file
COPY health-services.sln ./

# Copy project files
COPY HealthServices.Domain/HealthServices.Domain.csproj ./HealthServices.Domain/
COPY HealthServices.Application/HealthServices.Application.csproj ./HealthServices.Application/
COPY HealthServices.Infrastructure/HealthServices.Infrastructure.csproj ./HealthServices.Infrastructure/
COPY HealthServices.Api/HealthServices.Api.csproj ./HealthServices.Api/

# Restore dependencies
RUN dotnet restore

# Copy source code
COPY . ./

# Build and publish the application
RUN dotnet publish HealthServices.Api/HealthServices.Api.csproj -c Release -o out --no-restore

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks and other utilities
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy the published application
COPY --from=build-env /app/out .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose ports
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "HealthServices.Api.dll"] 