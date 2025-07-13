# Health Services Application

A comprehensive healthcare management system built with ASP.NET Core 9.0 following Layered Architecture principles.

## Architecture Overview

This application is designed using Layered Architecture (also known as N-tier Architecture) to ensure maintainability, testability, and separation of concerns. The solution is organized into four distinct layers with clear dependencies flowing from top to bottom:

### 1. Domain Layer (`HealthServices.Domain`)
**Purpose**: Contains the core business logic and entities that represent the heart of the healthcare domain.

**Structure**:
- **Entities/**: Domain entities representing core healthcare concepts
  - `User.cs` - Base user entity
  - `PatientProfile.cs` - Patient-specific information
  - `DoctorProfile.cs` - Doctor-specific information and credentials
  - `Appointment.cs` - Medical appointments and scheduling
  - `MedicalHistory.cs` - Patient medical records and history
  - `Specialty.cs` - Medical specialties and departments

- **ValueObjects/**: Immutable objects that represent domain concepts
- **Events/**: Domain events for business logic communication
- **Exceptions/**: Custom domain-specific exceptions
- **Services/**: Domain services for complex business logic

**Dependencies**: None (Foundation layer)

### 2. Application Layer (`HealthServices.Application`)
**Purpose**: Contains application-specific business logic, use cases, and DTOs. This layer orchestrates domain operations and defines application workflows.

**Structure**:
- **DTOs/**: Data Transfer Objects for external communication
- **Services/**: Application services implementing use cases

**Dependencies**: 
- `HealthServices.Domain` (depends on the layer below)

### 3. Infrastructure Layer (`HealthServices.Infrastructure`)
**Purpose**: Contains implementations for external concerns like database access, external services, and technical capabilities. This layer provides concrete implementations for data persistence and external integrations.

**Structure**:
- **DbContexts/**: Entity Framework Core database contexts
- **Configurations/**: Entity Framework configurations
- **Repositories/**: Data access implementations
- **Migrations/**: Database schema migrations
- **ExternalServices/**: Third-party service integrations

**Dependencies**: 
- `HealthServices.Domain` (for entity definitions)
- `HealthServices.Application` (for service contracts and DTOs)

### 4. API Layer (`HealthServices.Api`)
**Purpose**: The presentation layer that exposes HTTP endpoints and handles web-specific concerns. This is the top layer that orchestrates requests and responses.

**Structure**:
- **Controllers/**: REST API controllers
- **Middlewares/**: Custom middleware components
- **Configurations/**: API-specific configuration
- **Program.cs**: Application startup and configuration

**Dependencies**: 
- `HealthServices.Application` (for business logic)
- `HealthServices.Infrastructure` (for data access and external services)

## Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Language**: C# with nullable reference types enabled
- **Documentation**: Swagger/OpenAPI with Swashbuckle
- **Architecture**: Layered Architecture pattern
- **Development Environment**: Visual Studio 2022

## Why Layered Architecture?

Layered Architecture is an excellent choice for this healthcare management system because it provides:

1. **Clear Separation of Concerns**: Each layer has a specific responsibility and depends only on the layers below it
2. **Maintainability**: Changes in one layer have minimal impact on others
3. **Testability**: Each layer can be tested independently
4. **Familiarity**: Well-established pattern that most .NET developers understand
5. **Scalability**: Easy to modify or extend individual layers as requirements change

### Architecture Flow

```
┌─────────────────────────────────────┐
│           API Layer                 │  ← HTTP Requests/Responses
│      (Controllers, Middleware)      │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│        Application Layer            │  ← Business Logic & Use Cases
│      (Services, DTOs)              │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│       Infrastructure Layer          │  ← Data Access & External Services
│   (Repositories, DbContext)        │
└─────────────────────────────────────┘
                  ↓
┌─────────────────────────────────────┐
│         Domain Layer                │  ← Core Business Entities
│      (Entities, ValueObjects)      │
└─────────────────────────────────────┘
```

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- SQL Server (for future database implementation)

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd health-services
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the application:
   ```bash
   dotnet run --project HealthServices.Api
   ```

5. Access the API documentation:
   - Development: `https://localhost:7292/swagger`
   - Alternative: `http://localhost:5292/swagger`

## Development Guidelines

### Code Style and Structure

- Follow .NET and ASP.NET Core conventions
- Use PascalCase for class names, method names, and public members
- Use camelCase for local variables and private fields
- Use UPPERCASE for constants
- Prefix interface names with "I" (e.g., `IUserService`)

### Naming Conventions

- Use descriptive variable and method names (e.g., `IsUserSignedIn`, `CalculateTotal`)
- Use C# 10+ features when appropriate (record types, pattern matching)
- Leverage built-in ASP.NET Core features and middleware
- Use Entity Framework Core for database operations

### Error Handling

- Use exceptions for exceptional cases, not for control flow
- Implement proper error logging using built-in .NET logging
- Use Data Annotations or Fluent Validation for model validation
- Implement global exception handling middleware
- Return appropriate HTTP status codes and consistent error responses

### API Design

- Follow RESTful API design principles
- Use attribute routing in controllers
- Implement versioning for your API
- Use action filters for cross-cutting concerns
- Provide XML comments for controllers and models to enhance Swagger documentation

### Performance Optimization

- Use asynchronous programming with async/await for I/O-bound operations
- Implement caching strategies using IMemoryCache or distributed caching
- Use efficient LINQ queries and avoid N+1 query problems
- Implement pagination for large data sets

### Testing

- Write unit tests using NUnit
- Use Moq or NSubstitute for mocking dependencies
- Implement integration tests for API endpoints

### Security

- Use Authentication and Authorization middleware
- Implement JWT authentication for stateless API authentication
- Use HTTPS and enforce SSL
- Implement proper CORS policies

## Project Structure

```
health-services/
├── HealthServices.Domain/           # Core business logic
│   ├── Entities/                   # Domain entities
│   ├── ValueObjects/               # Value objects
│   ├── Events/                     # Domain events
│   ├── Exceptions/                 # Domain exceptions
│   └── Services/                   # Domain services
├── HealthServices.Application/      # Application layer
│   ├── DTOs/                       # Data transfer objects
│   └── Services/                   # Application services
├── HealthServices.Infrastructure/   # Infrastructure layer
│   ├── DbContexts/                 # Database contexts
│   ├── Configurations/             # EF configurations
│   ├── Repositories/               # Data access
│   ├── Migrations/                 # DB migrations
│   └── ExternalServices/           # Third-party services
└── HealthServices.Api/             # API layer
    ├── Controllers/                # REST controllers
    ├── Middlewares/                # Custom middleware
    ├── Configurations/             # API configuration
    └── Program.cs                  # Application startup
```

## Current Status

The application is in early development stage with the following implemented:
- Basic project structure following N-Tier Architecture
- ASP.NET Core 9.0 setup with Swagger documentation
- Project references and dependencies properly configured
- Sample Weather API endpoint for testing

## Future Development Tasks

### Immediate Priorities

1. **Database Implementation**
   - Set up Entity Framework Core DbContext
   - Create database migrations for all entities
   - Implement repository pattern for data access

2. **Domain Model Implementation**
   - Complete all entity classes with proper relationships
   - Implement value objects for complex data types
   - Add domain events for business logic communication

3. **API Development**
   - Create controllers for all main entities (Users, Patients, Doctors, Appointments)
   - Implement CRUD operations with proper validation
   - Add authentication and authorization

4. **Application Services**
   - Implement use cases for appointment booking
   - Create services for patient management
   - Add medical history management functionality

### Long-term Goals

1. **Advanced Features**
   - Real-time notifications for appointments
   - Integration with external healthcare systems
   - Reporting and analytics capabilities

2. **DevOps and Deployment**
   - Docker containerization
   - CI/CD pipeline setup
   - Production deployment configuration

3. **Testing and Quality**
   - Comprehensive unit test coverage
   - Integration testing setup
   - Performance testing and optimization

## Contributing

1. Follow the established coding standards and layered architecture patterns
2. Respect layer dependencies - upper layers can depend on lower layers, but not vice versa
3. Ensure all new features include appropriate tests
4. Update documentation when adding new functionality
5. Use meaningful commit messages and proper branching strategy
6. Review security implications of any new code

## Support

For questions or issues related to this application, please contact the development team or create an issue in the project repository.

---

*This documentation should be updated as the application evolves and new features are implemented.* 