# BaseRedisCacheStorageService

## Oversikt

`BaseRedisCacheStorageService<T>` er en abstrakt basisklasse som implementerer `IStorageService<T>` for Redis-basert storage av entiteter. Den gir distribuert caching på tvers av flere applikasjonsinstanser.

## Funksjoner

- **CRUD-operasjoner**: Create, Read, Update, Delete med Redis
- **JSON-serialisering**: Automatisk serialisering/deserialisering av entiteter
- **Index-håndtering**: Bruker Redis Set for effektiv håndtering av alle entitets-IDer
- **Parallel henting**: GetAllAsync henter alle entiteter parallelt for bedre ytelse
- **Thread-safe**: Bruker Redis' atomiske operasjoner (SADD, SREM)

## Implementasjon

### Person Storage med Redis

```csharp
public class PersonRedisCacheStorageService(IConnectionMultiplexer redis, IClock clock) 
    : BaseRedisCacheStorageService<PersonModel>(redis, clock), IPersonStorageService
{
    protected override string Name { get; } = "Person";
}
```

## Konfigurasjon i Program.cs

### 1. Installer NuGet-pakker

```bash
dotnet add package StackExchange.Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

### 2. Konfigurer Redis i appsettings.json

```json
{
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### 3. Registrer services i Program.cs

```csharp
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Konfigurer Redis connection
var redisConnectionString = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
var redis = ConnectionMultiplexer.Connect(redisConnectionString);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// Registrer storage service
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IPersonStorageService, PersonRedisCacheStorageService>();

// Eller bruk memory cache (original implementasjon)
// builder.Services.AddMemoryCache();
// builder.Services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();
```

## Bytte mellom Memory Cache og Redis

Du kan enkelt bytte mellom in-memory cache og Redis ved å endre DI-registreringen:

### Memory Cache (lokal, rask, ikke-persistent)
```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();
```

### Redis (distribuert, persistent, deles mellom instanser)
```csharp
var redis = ConnectionMultiplexer.Connect("localhost:6379");
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
builder.Services.AddScoped<IPersonStorageService, PersonRedisCacheStorageService>();
```

## Redis-nøkler

BaseRedisCacheStorageService bruker følgende nøkkelstruktur:

- **Index**: `{Name}:index` - Redis Set med alle entitets-IDer
- **Entitet**: `{Name}:{id}` - Redis String med JSON-serialisert entitet

Eksempel for Person:
- Index: `Person:index` (Redis Set)
- Entitet: `Person:550e8400-e29b-41d4-a716-446655440000` (Redis String)

## Forskjeller fra Memory Cache

| Feature | Memory Cache | Redis |
|---------|-------------|-------|
| **Persistens** | Nei (data går tapt ved restart) | Ja (data bevares) |
| **Distribuert** | Nei (per instans) | Ja (delt mellom instanser) |
| **Ytelse** | Veldig rask (in-process) | Rask (nettverkskall) |
| **Skalerbarhet** | Begrenset av minne | Høy (dedikert Redis-server) |
| **Use case** | Single instance, dev/test | Production, multi-instance |

## Docker Compose for Redis (utviklingsmiljø)

```yaml
version: '3.8'
services:
  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes

volumes:
  redis-data:
```

Start Redis:
```bash
docker-compose up -d
```

## Fordeler med BaseRedisCacheStorageService

1. **Distribuert**: Data deles mellom alle applikasjonsinstanser
2. **Persistent**: Data overlever applikasjonsrestarter
3. **Skalerbar**: Støtter horisontal skalering av applikasjonen
4. **Samme interface**: Bytt enkelt mellom Memory og Redis
5. **JSON-serialisering**: Enkel debugging og inspeksjon av data
6. **Parallel henting**: Raskere GetAllAsync med Task.WhenAll

## Testing

For tester anbefales det å bruke Memory Cache eller en testcontainer med Redis:

```csharp
// I tester
services.AddMemoryCache();
services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();

// Eller bruk Testcontainers.Redis for integrasjonstester
```

## Fremtidige forbedringer

- Implementer TTL (Time-To-Live) for automatisk utløp av cache
- Legg til Redis Pub/Sub for event-notifikasjoner
- Implementer distribuert locking med RedLock for bedre concurrency
- Legg til Redis Streams for event sourcing
- Implementer batch-operasjoner for bedre ytelse
