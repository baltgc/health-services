#!/bin/bash

# Build and run Health Services application with Docker Compose

echo "🏥 Building Health Services Application..."

# Stop any running containers
echo "Stopping existing containers..."
docker-compose down

# Build the application
echo "Building Docker images..."
docker-compose build --no-cache

# Start the services
echo "Starting services..."
docker-compose up -d

# Wait for services to be ready
echo "Waiting for services to start..."
sleep 10

# Check if services are running
echo "Checking service status..."
docker-compose ps

# Show logs
echo "Service logs:"
docker-compose logs --tail=20

echo ""
echo "🎉 Health Services is now running!"
echo "📖 API Documentation: http://localhost:8080/swagger"
echo "🏥 Health Check: http://localhost:8080/health"
echo "🐘 PostgreSQL: localhost:5432"
echo ""
echo "To view logs: docker-compose logs -f"
echo "To stop services: docker-compose down" 