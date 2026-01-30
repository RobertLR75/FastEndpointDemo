# Program.cs - Eksempel på konfigurasjon

## Med StorageServiceExtensions (anbefalt)

```csharp
using FastEndpointDemo.Services;
using FastEndpoints;
using FastEndpoints.Swagger;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(); 

// Enkel registrering av storage services (leser fra appsettings.json)
builder.Services.AddStorageServices(builder.Configuration);

// Eller eksplisitt Memory Cache
// builder.Services.AddMemoryCacheStorage();

// Eller eksplisitt Redis
// builder.Services.AddRedisCacheStorage("localhost:6379");

builder.Services.AddHostedService<PersonStorageInitializerService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.UseDefaultExceptionHandler()
    .UseFastEndpoints()
    .UseSwaggerGen();
    
app.Run();

namespace FastEndpointDemo
{
    public partial class Program { }
}
```

## Manuell konfigurasjon (hvis du vil ha mer kontroll)

```csharp
using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Interfaces;
using FastEndpoints;
using FastEndpoints.Swagger;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddFastEndpoints()
    .SwaggerDocument(); 

// Les storage type fra konfigurasjon
var storageType = builder.Configuration["Storage:Type"]?.ToLowerInvariant() ?? "memory";

builder.Services.AddSingleton<IClock, SystemClock>();

if (storageType == "redis")
{
    // Redis storage
    var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);
    builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
    builder.Services.AddScoped<IPersonStorageService, PersonRedisCacheStorageService>();
}
else
{
    // Memory cache storage (default)
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();
}

builder.Services.AddHostedService<PersonStorageInitializerService>();

var app = builder.Build();
app.UseHttpsRedirection();

app.UseDefaultExceptionHandler()
    .UseFastEndpoints()
    .UseSwaggerGen();
    
app.Run();

namespace FastEndpointDemo
{
    public partial class Program { }
}
```

## appsettings.json konfigurasjon

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Storage": {
    "Type": "Memory"
  }
}
```

For å bytte til Redis, endre `"Type": "Memory"` til `"Type": "Redis"`.

## Kjøre Redis lokalt med Docker

```bash
# Start Redis
docker-compose up -d

# Sjekk at Redis kjører
docker ps

# Test Redis connection
docker exec -it fastendpoint-redis redis-cli ping
# Skal returnere: PONG

# Åpne Redis Commander (Web UI) i nettleser
# http://localhost:8081
```

## Environment-spesifikk konfigurasjon

### appsettings.Development.json
```json
{
  "Storage": {
    "Type": "Memory"
  }
}
```

### appsettings.Production.json
```json
{
  "Storage": {
    "Type": "Redis"
  },
  "Redis": {
    "ConnectionString": "your-redis-server:6379"
  }
}
```
