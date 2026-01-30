# FastEndpointDemo

## Overview

This is a demonstration project showcasing [FastEndpoints](https://fast-endpoints.com/), a high-performance REST API framework for .NET that offers a simpler and faster alternative to traditional ASP.NET Core MVC controllers.

## What This Program Does

FastEndpointDemo implements a complete RESTful API for managing Person records with the following operations:

- **Create Person** (`POST /persons`) - Add new persons to the system
- **Get Person** (`GET /persons/{id}`) - Retrieve a person by their unique ID
- **Get All Persons** (`GET /persons`) - List all persons in the system
- **Update Person** (`PUT /persons/{id}`) - Modify existing person records

### Key Features

- ✅ **FastEndpoints Framework** - High-performance endpoint-based architecture
- ✅ **In-Memory Storage** - Fast data access using `IMemoryCache`
- ✅ **Event Publishing** - Domain events for person creation and updates
- ✅ **Request/Response Mapping** - Clean separation between API and domain models
- ✅ **Swagger/OpenAPI** - Auto-generated API documentation
- ✅ **Request Logging** - Pre and post-processors for monitoring

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later

### Running the Application

```bash
# Clone the repository
git clone https://github.com/RobertLR75/FastEndpointDemo.git

# Navigate to the project directory
cd FastEndpointDemo/FastEndpointDemo

# Run the application
dotnet run

# The API will be available at:
# - https://localhost:5001 (HTTPS)
# - http://localhost:5000 (HTTP)
```

### Exploring the API

Once running, visit:
- **Swagger UI**: `https://localhost:5001/swagger`
- **OpenAPI Spec**: `https://localhost:5001/swagger/v1/swagger.json`

### Example Usage

```bash
# Create a person
curl -X POST https://localhost:5001/persons \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe"}'

# Get all persons
curl https://localhost:5001/persons

# Get a specific person
curl https://localhost:5001/persons/{guid}

# Update a person
curl -X PUT https://localhost:5001/persons/{guid} \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe"}'
```

## Project Structure

```
FastEndpointDemo/
├── Endpoints/           # API endpoint definitions
│   ├── Create/         # Person creation endpoint
│   ├── Get/            # Get single person endpoint
│   ├── GetAll/         # Get all persons endpoint
│   ├── Update/         # Person update endpoint
│   └── Processors/     # Request/response processors
├── Services/           # Business logic and storage
│   ├── Models/         # Domain models
│   ├── Interfaces/     # Service contracts
│   └── Exceptions/     # Custom exceptions
└── Program.cs          # Application entry point
```

## Testing

See [FastEndpoints.Tests/README.md](./FastEndpoints.Tests/README.md) for information about the testing strategy and test structure.

## Technologies Used

- **.NET 10.0** - Latest .NET runtime
- **FastEndpoints 7.2.0** - High-performance REST API framework
- **FastEndpoints.Swagger 7.2.0** - OpenAPI/Swagger integration
- **Microsoft.Extensions.Caching.Memory** - In-memory caching

## Learn More

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [FastEndpoints GitHub](https://github.com/FastEndpoints/FastEndpoints)

## License

See [LICENSE](./LICENSE) file for details.
