# Build and run Health Services application with Docker Compose

Write-Host "🏥 Building Health Services Application..." -ForegroundColor Green

# Stop any running containers
Write-Host "Stopping existing containers..." -ForegroundColor Yellow
docker-compose down

# Build the application
Write-Host "Building Docker images..." -ForegroundColor Yellow
docker-compose build --no-cache

# Start the services
Write-Host "Starting services..." -ForegroundColor Yellow
docker-compose up -d

# Wait for services to be ready
Write-Host "Waiting for services to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if services are running
Write-Host "Checking service status..." -ForegroundColor Yellow
docker-compose ps

# Show logs
Write-Host "Service logs:" -ForegroundColor Yellow
docker-compose logs --tail=20

Write-Host ""
Write-Host "🎉 Health Services is now running!" -ForegroundColor Green
Write-Host "📖 API Documentation: http://localhost:8080/swagger" -ForegroundColor Cyan
Write-Host "🏥 Health Check: http://localhost:8080/health" -ForegroundColor Cyan
Write-Host "🐘 PostgreSQL: localhost:5432" -ForegroundColor Cyan
Write-Host ""
Write-Host "To view logs: docker-compose logs -f" -ForegroundColor Gray
Write-Host "To stop services: docker-compose down" -ForegroundColor Gray 