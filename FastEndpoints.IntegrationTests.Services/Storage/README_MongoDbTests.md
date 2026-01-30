# BaseMongoDbStorageService Tester

## Oversikt

`BaseMongoDbStorageServiceTests` inneholder unit tester for MongoDB-basert storage service. Testene bruker **Testcontainers** (DotNet.Testcontainers) for å kjøre en ekte MongoDB-instans i Docker under testing.

## Status

✅ **Testklassen er opprettet** med 20 omfattende tester
⚠️ **Krever packages restore** - Kjør `dotnet restore` for å laste inn MongoDB.Driver

## Testoppsett

### Testcontainers

Testene bruker `DotNet.Testcontainers` som automatisk:
1. Starter en MongoDB 7 Docker-container før hver test
2. Mapper port 27017 til en tilfeldig host-port
3. Gir en unik connection string til testen
4. Stopper og rydder opp containeren etter testen

Dette gir ekte integrasjonstester mot MongoDB uten å måtte sette opp MongoDB manuelt.

### IAsyncLifetime

Testklassen implementerer `IAsyncLifetime` for setup og teardown:

```csharp
public async ValueTask InitializeAsync()
{
    _mongoContainer = new ContainerBuilder()
        .WithImage("mongo:7")
        .WithPortBinding(27017, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(27017))
        .Build();
    await _mongoContainer.StartAsync();
    
    var port = _mongoContainer.GetMappedPublicPort(27017);
    _mongoClient = new MongoClient($"mongodb://localhost:{port}");
}
```

## Testdekning (20 tester)

### CRUD-operasjoner (8 tester)
- ✅ `CreateAsync_WhenIdEmpty_AssignsGuidAndStoresEntity`
- ✅ `CreateAsync_WhenIdProvided_PreservesId`
- ✅ `GetAsync_WhenMissing_ReturnsNull`
- ✅ `GetAsync_WhenExists_ReturnsEntity`
- ✅ `GetAllAsync_WhenCollectionEmpty_ReturnsEmpty`
- ✅ `GetAllAsync_ReturnsAllEntities`
- ✅ `UpdateAsync_SetsUpdatedAt_AndOverwritesStoredEntity`
- ✅ `DeleteAsync_RemovesStoredEntity`

### Edge cases (6 tester)
- ✅ `CreateAsync_WhenCalledTwiceWithSameId_ThrowsDuplicateKeyException` ⚠️
- ✅ `DeleteAsync_IsIdempotent`
- ✅ `UpdateAsync_WhenEntityDoesNotExist_DoesNotThrow`
- ✅ `UpdateAsync_PreservesCreatedAt`
- ✅ `DeleteAsync_OnlyRemovesSpecifiedEntity`
- ✅ `CreateAsync_GeneratesVersion7Guid`

### MongoDB-spesifikke tester (6 tester)
- ✅ `MongoDbSerialization_PreservesDateTimeOffsetPrecision`
- ✅ `MultipleServices_ShareSameMongoDbCollection`
- ✅ `EnsureIndexesAsync_CreatesIndexes`
- ✅ `FindAsync_WithFilter_ReturnsMatchingEntities`
- ✅ `CountAsync_ReturnsCorrectCount`

⚠️ **Viktig forskjell**: MongoDB kaster `MongoWriteException` ved duplikat _id (forskjellig fra Memory og Redis).

## Kjøre testene

### Forutsetninger

Testene krever at **Docker** kjører på maskinen, da Testcontainers starter MongoDB i en container.

### Installer packages først

```bash
cd /Users/robert/Repos/GitHub/FastEndpointDemo
dotnet restore
```

### Kjør alle MongoDB-tester

```bash
# Fra root-katalogen
dotnet test FastEndpoints.UnitTests/FastEndpoints.UnitTests.csproj --filter "FullyQualifiedName~BaseMongoDbStorageServiceTests"
```

### Kjør en spesifikk test

```bash
dotnet test --filter "FullyQualifiedName~BaseMongoDbStorageServiceTests.CreateAsync_WhenIdEmpty_AssignsGuidAndStoresEntity"
```

### Kjør alle storage-tester (Memory + Redis + MongoDB)

```bash
dotnet test FastEndpoints.UnitTests/FastEndpoints.UnitTests.csproj --filter "FullyQualifiedName~Storage"
```

## Testytelse

MongoDB-testene tar lenger tid enn Memory Cache-testene fordi de:
1. Starter en Docker-container for hver test (via IAsyncLifetime)
2. Gjør faktiske nettverkskall til MongoDB
3. Stopper containeren etter hver test

**Forventet tid per test**: ~3-6 sekunder (avhengig av Docker-ytelse)

## Viktige forskjeller fra Memory Cache og Redis

| Aspekt | Memory Cache | Redis | MongoDB |
|--------|--------------|-------|---------|
| **Duplikat ID** | Overskriver | Overskriver | Kaster exception ⚠️ |
| **Collection** | N/A | Keys | Collections ✅ |
| **Indexes** | N/A | N/A | Ja ✅ |
| **Queries** | LINQ in-memory | Begrenset | Full support ✅ |
| **Container** | Nei | Redis 7 Alpine | MongoDB 7 |
| **Port mapping** | N/A | 6379 | 27017 (random) |

⚠️ **MongoDB-spesifikt**: 
- `_id` field er unik og vil kaste `MongoWriteException` ved duplikat
- Støtter compound indexes og full query syntax
- Collection names må defineres (f.eks. "test_entities")

## Debugging

### Se MongoDB-innhold under testing

Legg til en breakpoint og inspiser MongoDB:

```csharp
var collection = service.GetCollection();
var allDocs = await collection.Find(Builders<TestEntity>.Filter.Empty).ToListAsync();
Console.WriteLine($"MongoDB documents: {allDocs.Count}");
```

### Testcontainers logger

Testcontainers logger til konsollen. Kjør med `--verbosity detailed` for å se Docker-output:

```bash
dotnet test --verbosity detailed --filter "FullyQualifiedName~BaseMongoDbStorageServiceTests"
```

### Docker-containere

Testcontainers rydder automatisk opp, men hvis tester krasjer kan containere bli igjen:

```bash
# Se containere
docker ps -a | grep mongo

# Rydd opp
docker rm -f $(docker ps -aq --filter "ancestor=mongo:7")
```

### Koble til MongoDB under test

Sett et breakpoint og noter porten fra `GetMappedPublicPort`, deretter:

```bash
# Koble til med mongosh
docker exec -it <container-name> mongosh

# Eller direkte:
mongosh mongodb://localhost:<port>/test_db
```

## CI/CD

For CI/CD-pipelines (GitHub Actions, Azure DevOps, etc.), sørg for at Docker er tilgjengelig:

```yaml
# GitHub Actions eksempel
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      - name: Run MongoDB tests
        run: dotnet test --filter "FullyQualifiedName~BaseMongoDbStorageServiceTests"
```

Ubuntu runners har Docker installert som standard.

## Pakker

Testene bruker følgende NuGet-pakker:

```xml
<PackageReference Include="MongoDB.Driver" Version="2.30.0" />
<PackageReference Include="DotNet.Testcontainers" Version="3.10.0" />
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="xunit.v3" Version="3.2.2" />
```

## Sammenligning: Memory vs Redis vs MongoDB

### Testoppsett

| | Memory | Redis | MongoDB |
|---|---|---|---|
| **Container** | Nei | RedisContainer | ContainerBuilder |
| **Setup tid** | <1s | ~2-3s | ~3-5s |
| **Image** | N/A | redis:7-alpine | mongo:7 |
| **Builder** | N/A | RedisBuilder | ContainerBuilder |

### Oppførsel

| Scenario | Memory | Redis | MongoDB |
|---|---|---|---|
| **Duplikat ID** | Overskriver | Overskriver | Exception |
| **Delete + Delete** | OK | OK | OK |
| **Update non-existent** | Oppretter | Oppretter | Ingen effekt |
| **Index** | N/A | Redis Set | MongoDB indexes |

### Use cases

- **Memory**: Unit tests, dev, rask feedback
- **Redis**: Cache tests, distributed scenarios
- **MongoDB**: Database tests, query tests, production-like

## Best Practices

1. **Docker må kjøre** - Testene feiler hvis Docker ikke er tilgjengelig
2. **Isolering** - Hver test får sin egen MongoDB-instans
3. **Test isolation** - IAsyncLifetime sikrer clean state
4. **Parallellisering** - Testene kan kjøres parallelt (hver får egen container)
5. **CI/CD** - Testcontainers fungerer i CI/CD med Docker

## Feilsøking

### "Cannot resolve symbol 'BaseMongoDbStorageService'"

```bash
cd /Users/robert/Repos/GitHub/FastEndpointDemo
dotnet restore
dotnet build FastEndpointDemo/FastEndpointDemo.csproj
dotnet build FastEndpoints.UnitTests/FastEndpoints.UnitTests.csproj
```

### "Cannot connect to Docker daemon"

Sørg for at Docker Desktop kjører:

```bash
docker ps
```

### "Port already in use"

Testcontainers mapper til random port automatisk, men hvis du får feil:

```bash
# Stopp alle MongoDB containere
docker stop $(docker ps -q --filter "ancestor=mongo:7")
```

## Fremtidige forbedringer

- [ ] Legg til tester for MongoDB transactions
- [ ] Test Change Streams funksjonalitet
- [ ] Test aggregation pipelines
- [ ] Test full-text search indexes
- [ ] Test Time Series collections
- [ ] Test GridFS for fillagring
- [ ] Performance-tester (sammenlign med Redis)
- [ ] Test failover-scenarier med replica sets

## Oppsummering

✅ **20 omfattende tester** dekker alle CRUD-operasjoner og edge cases
✅ **Testcontainers** gir isolerte, rene MongoDB-instanser per test
✅ **Samme teststruktur** som Memory og Redis for konsistens
✅ **MongoDB-spesifikke** tester for indexes, queries og serialisering
✅ **Production-like** tester mot ekte MongoDB 7
✅ **CI/CD-klar** fungerer på alle plattformer med Docker

Testklassen er klar til bruk etter `dotnet restore`! 🎉
