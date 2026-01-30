# BaseMongoDbStorageService

## Oversikt

`BaseMongoDbStorageService<T>` er en abstrakt basisklasse som implementerer `IStorageService<T>` for MongoDB-basert storage av entiteter. Den gir full database-funksjonalitet med queries, indexes og persistence.

## Funksjoner

- **CRUD-operasjoner**: Create, Read, Update, Delete med MongoDB
- **MongoDB collections**: Hver entitetstype får sin egen collection
- **Index management**: Automatisk opprettelse av indexes for optimal ytelse
- **Query helpers**: Protected metoder for egendefinerte søk
- **Cursor-basert**: Effektiv håndtering av store datasett
- **Skalerbar**: Full MongoDB-funksjonalitet med replicas og sharding

## Implementasjon

### Person Storage med MongoDB

```csharp
public class PersonMongoDbStorageService(IMongoDatabase database, IClock clock) 
    : BaseMongoDbStorageService<PersonModel>(database, clock), IPersonStorageService
{
    protected override string CollectionName { get; } = "persons";

    public override async Task EnsureIndexesAsync(CancellationToken cancellationToken = default)
    {
        await base.EnsureIndexesAsync(cancellationToken);

        // Opprett compound index på FirstName og LastName
        var nameIndexKeys = Builders<PersonModel>.IndexKeys
            .Ascending(p => p.FirstName)
            .Ascending(p => p.LastName);

        var nameIndexModel = new CreateIndexModel<PersonModel>(nameIndexKeys, new CreateIndexOptions
        {
            Name = "idx_name",
            Unique = false
        });

        await Collection.Indexes.CreateOneAsync(nameIndexModel, cancellationToken: cancellationToken);
    }
}
```

## Konfigurasjon i Program.cs

### 1. Installer NuGet-pakke

```bash
dotnet add package MongoDB.Driver
```

### 2. Konfigurer MongoDB i appsettings.json

```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "FastEndpointDemo"
  }
}
```

### 3. Registrer services i Program.cs

```csharp
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Konfigurer MongoDB connection
var mongoConnectionString = builder.Configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "FastEndpointDemo";

var mongoClient = new MongoClient(mongoConnectionString);
var mongoDatabase = mongoClient.GetDatabase(mongoDatabaseName);

builder.Services.AddSingleton<IMongoClient>(mongoClient);
builder.Services.AddSingleton(mongoDatabase);
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddScoped<IPersonStorageService, PersonMongoDbStorageService>();

// Eller bruk extension method
builder.Services.AddStorageServices(builder.Configuration);
```

### 4. Opprett indexes ved oppstart

```csharp
var app = builder.Build();

// Opprett MongoDB indexes
using (var scope = app.Services.CreateScope())
{
    var personService = scope.ServiceProvider.GetRequiredService<IPersonStorageService>();
    if (personService is PersonMongoDbStorageService mongoService)
    {
        await mongoService.EnsureIndexesAsync();
    }
}

app.Run();
```

## Bytte mellom Memory Cache, Redis og MongoDB

Du kan enkelt bytte mellom storage-implementasjoner ved å endre konfigurasjon:

### Memory Cache (lokal, rask, ikke-persistent)
```json
{
  "Storage": {
    "Type": "Memory"
  }
}
```

### Redis (distribuert cache, persistent)
```json
{
  "Storage": {
    "Type": "Redis"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### MongoDB (full database, queries, skalerbar)
```json
{
  "Storage": {
    "Type": "MongoDB"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "FastEndpointDemo"
  }
}
```

## MongoDB Collections og Indexes

BaseMongoDbStorageService oppretter følgende struktur:

### Collection navn
- Defineres av `CollectionName` property i avledet klasse
- Eksempel: `"persons"` for PersonMongoDbStorageService

### Standard indexes (fra BaseMongoDbStorageService)
- **idx_id_created**: Compound index på `Id` og `CreatedAt`

### Tilpassede indexes (fra PersonMongoDbStorageService)
- **idx_name**: Compound index på `FirstName` og `LastName`
- **idx_updated_at**: Descending index på `UpdatedAt`

## Query Helpers

BaseMongoDbStorageService gir protected helper-metoder for egendefinerte queries:

### FindAsync
```csharp
public async Task<List<PersonModel>> FindByNameAsync(string firstName, CancellationToken ct)
{
    var filter = Builders<PersonModel>.Filter.Regex(
        p => p.FirstName, 
        new BsonRegularExpression(firstName, "i")
    );
    
    return await FindAsync(filter, ct);
}
```

### CountAsync
```csharp
public async Task<long> CountActivePersonsAsync(CancellationToken ct)
{
    var filter = Builders<PersonModel>.Filter.Eq(p => p.IsActive, true);
    return await CountAsync(filter, ct);
}
```

### Direct Collection Access
```csharp
public async Task<List<PersonModel>> GetRecentPersonsAsync(int limit, CancellationToken ct)
{
    return await Collection
        .Find(Builders<PersonModel>.Filter.Empty)
        .SortByDescending(p => p.CreatedAt)
        .Limit(limit)
        .ToListAsync(ct);
}
```

## Forskjeller fra Memory Cache og Redis

| Feature | Memory Cache | Redis | MongoDB |
|---------|-------------|-------|---------|
| **Persistens** | Nei | Ja | Ja |
| **Distribuert** | Nei | Ja | Ja |
| **Queries** | Nei (LINQ in-memory) | Nei | Ja (MongoDB queries) |
| **Indexes** | Nei | Nei | Ja |
| **Aggregations** | Nei | Begrenset | Ja (full pipeline) |
| **Transactions** | Nei | Begrenset | Ja (multi-document) |
| **Skalerbarhet** | Lav | Middels | Høy (sharding) |
| **Use case** | Dev/test | Cache | Production database |

## Docker Compose for MongoDB (utviklingsmiljø)

```yaml
version: '3.8'
services:
  mongodb:
    image: mongo:7
    ports:
      - "27017:27017"
    environment:
      - MONGO_INITDB_DATABASE=FastEndpointDemo
    volumes:
      - mongodb-data:/data/db
      - mongodb-config:/data/configdb
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 5s
      timeout: 3s
      retries: 5

  mongo-express:
    image: mongo-express:latest
    ports:
      - "8082:8081"
    environment:
      - ME_CONFIG_MONGODB_URL=mongodb://mongodb:27017/
      - ME_CONFIG_BASICAUTH=false
    depends_on:
      - mongodb

volumes:
  mongodb-data:
  mongodb-config:
```

Start MongoDB:
```bash
docker-compose up -d mongodb mongo-express
```

Åpne Mongo Express (Web UI):
```
http://localhost:8082
```

## MongoDB Shell Commands

```bash
# Koble til MongoDB
docker exec -it fastendpoint-mongodb mongosh

# Vis databaser
show dbs

# Bruk FastEndpointDemo database
use FastEndpointDemo

# Vis collections
show collections

# Vis alle personer
db.persons.find().pretty()

# Vis indexes
db.persons.getIndexes()

# Tell dokumenter
db.persons.countDocuments()

# Søk med filter
db.persons.find({ "FirstName": "John" })

# Slett alle dokumenter (for testing)
db.persons.deleteMany({})

# Dropp collection
db.persons.drop()
```

## Fordeler med BaseMongoDbStorageService

1. **Full database-funksjonalitet**: Queries, aggregations, transactions
2. **Skalerbar**: Støtter replica sets og sharding
3. **Fleksibel**: Egendefinerte indexes og queries
4. **Persistent**: Data bevares permanent
5. **Production-ready**: Brukt av millioner av applikasjoner
6. **Samme interface**: Bytt enkelt mellom Memory/Redis/MongoDB
7. **Developer-friendly**: Mongo Express UI for debugging

## Testing

For tester anbefales det å:
- Bruke Memory Cache for unit tests
- Bruke Testcontainers.MongoDb for integrasjonstester
- Bruke MongoDB Atlas Free Tier for staging

```csharp
// I unit tester
services.AddMemoryCache();
services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();

// I integrasjonstester (med Testcontainers)
var mongoContainer = new MongoDbBuilder().Build();
await mongoContainer.StartAsync();
var client = new MongoClient(mongoContainer.GetConnectionString());
```

## Fremtidige forbedringer

- [ ] Implementer Change Streams for real-time updates
- [ ] Legg til Aggregation pipeline helpers
- [ ] Implementer soft delete med filters
- [ ] Legg til pagination support (skip/take)
- [ ] Implementer multi-document transactions
- [ ] Legg til Time Series collections
- [ ] Implementer full-text search indexes
- [ ] Legg til GridFS for fillagring
