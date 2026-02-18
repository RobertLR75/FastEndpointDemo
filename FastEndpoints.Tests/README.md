# FastEndpoints.Tests

## Overview

This directory contains the test suite for the FastEndpointDemo application.

## About FastEndpointDemo

FastEndpointDemo is a demonstration project showcasing the use of [FastEndpoints](https://fast-endpoints.com/), a high-performance alternative to ASP.NET Core MVC for building REST APIs in .NET.

### What the Program Does

The application implements a RESTful API for managing Person records with the following capabilities:

- **Create** - Add new persons to the system
- **Read** - Retrieve individual persons by ID or get all persons
- **Update** - Modify existing person records
- **Delete** - (If implemented) Remove persons from the system

### Key Features

1. **In-Memory Storage**: Uses `IMemoryCache` for fast data access
2. **Event Publishing**: Emits events when persons are created or updated
3. **Request/Response Mapping**: Clean separation between API contracts and domain models
4. **Swagger Documentation**: Auto-generated API documentation
5. **Request Logging**: Pre and post-processing for monitoring

### API Endpoints

- `POST /persons` - Create a new person
- `GET /persons/{id:guid}` - Get a person by ID
- `GET /persons` - Get all persons
- `PUT /persons` - Update a person (ID in request body)

### Domain Model

The `PersonModel` includes:
- `Id` (Guid) - Unique identifier
- `FirstName` (string) - Person's first name
- `LastName` (string) - Person's last name
- `CreatedAt` (DateTimeOffset) - Creation timestamp
- `UpdatedAt` (DateTimeOffset?) - Last update timestamp

## Testing Strategy

### Planned Test Coverage

This test project should cover:

1. **Unit Tests**
   - Endpoint handlers logic
   - Mapper transformations
   - Service layer operations
   - Validators

2. **Integration Tests**
   - Full HTTP request/response cycles
   - Endpoint configuration
   - Exception handling
   - Event publishing

3. **API Contract Tests**
   - Request/Response models
   - HTTP status codes
   - Content negotiation

### Test Organization

Tests should be organized by feature/endpoint:
```
FastEndpoints.Tests/
├── Endpoints/
│   ├── Create/
│   │   ├── CreatePersonEndpointTests.cs
│   │   └── CreatePersonMapperTests.cs
│   ├── Get/
│   │   ├── GetPersonEndpointTests.cs
│   │   └── GetPersonMapperTests.cs
│   ├── GetAll/
│   │   └── GetAllPersonsEndpointTests.cs
│   └── Update/
│       ├── UpdatePersonEndpointTests.cs
│       └── UpdatePersonCommandTests.cs
└── Services/
    └── PersonStorageServiceTests.cs
```

## Running Tests

### Prerequisites

- .NET 10.0 SDK or later
- Visual Studio 2022, Visual Studio Code, or Rider

### Commands

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run specific test class
dotnet test --filter FullyQualifiedName~CreatePersonEndpointTests

# Run tests in watch mode
dotnet watch test
```

## Test Dependencies

The test project should include:
- `xUnit` or `NUnit` - Test framework
- `FluentAssertions` - Readable assertions
- `Moq` or `NSubstitute` - Mocking framework
- `Microsoft.AspNetCore.Mvc.Testing` - Integration testing
- `WebApplicationFactory<Program>` - Testing web applications

## Contributing

When adding new features to the main application:
1. Write tests first (TDD approach)
2. Ensure all tests pass before committing
3. Maintain test coverage above 80%
4. Follow the existing test structure and naming conventions

## Notes

- Tests are currently not implemented but this structure provides guidance for future test development
- The application uses in-memory storage, making it ideal for testing without external dependencies
- FastEndpoints has excellent testing support through `Factory<TProgram>` and `Factory.CreateClient()`
