# BaseRedisCacheStorageService Tester

## Oversikt

`BaseRedisCacheStorageServiceTests` inneholder unit tester for Redis-basert storage service. Testene bruker **Testcontainers** for å kjøre en ekte Redis-instans i Docker under testing.

## Testoppsett

### Testcontainers

Testene bruker `Testcontainers.Redis` som automatisk:
1. Starter en Redis Docker-container før hver test
2. Gir en unik connection string til testen
3. Stopper og rydder opp containeren etter testen

Dette gir ekte integrasjonstester mot Redis uten å måtte sette opp Redis manuelt.

### IAsyncLifetime

Testklassen implementerer `IAsyncLifetime` for setup og teardown:

```csharp
public async ValueTask InitializeAsync()
{
    _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();
    await _redisContainer.StartAsync();
    _redis = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
}

public async ValueTask DisposeAsync()
{
    await _redis?.CloseAsync();
    await _redisContainer?.StopAsync();
}
```

## Testdekning

Testklassen dekker samme scenarios som `BaseMemoryCacheStorageServiceTests`, men med Redis:

### CRUD-operasjoner
- ✅ `CreateAsync_WhenIdEmpty_AssignsGuidAndStoresEntity_AndAddsToIndex`
- ✅ `CreateAsync_WhenIdProvided_PreservesId`
- ✅ `GetAsync_WhenMissing_ReturnsNull`
- ✅ `GetAsync_WhenExists_ReturnsEntity`
- ✅ `GetAllAsync_WhenIndexEmpty_ReturnsEmpty`
- ✅ `UpdateAsync_SetsUpdatedAt_AndOverwritesStoredEntity`
- ✅ `DeleteAsync_RemovesStoredEntity_AndRemovesFromIndex` ⚠️

### Edge cases
- ✅ `GetAllAsync_WhenIndexHasStaleIds_FiltersMissingEntities`
- ✅ `CreateAsync_WhenCalledTwiceWithSameId_DoesNotDuplicateIndexEntry`
- ✅ `DeleteAsync_IsIdempotent_AndRemovesFromIndex` ⚠️
- ✅ `UpdateAsync_WhenEntityDoesNotExist_StillCreatesEntity`

### Redis-spesifikke tester
- ✅ `GetAllAsync_ReturnsAllEntities_InParallel` - Verifiserer parallel henting
- ✅ `JsonSerialization_PreservesDateTimeOffsetPrecision` - Verifiserer JSON-serialisering
- ✅ `MultipleServices_ShareSameRedisIndex` - Verifiserer delt state
- ✅ `RedisSet_AutomaticallyDeduplicatesIndexEntries` - Verifiserer Set-egenskaper

## Viktige forskjeller fra Memory Cache

| Aspekt | Memory Cache | Redis |
|--------|--------------|-------|
| **DeleteAsync index-oppførsel** | ID blir i index | ID fjernes fra index ⚠️ |
| **Delt state** | Nei (per instance) | Ja (mellom services) |
| **Serialisering** | Direkte objekt | JSON (camelCase) |
| **Ytelse** | Raskere | Noe tregere (nettverk) |
| **Persistens** | Nei | Ja |

⚠️ **Viktig forskjell**: Redis-implementasjonen fjerner ID fra index ved `DeleteAsync`, mens Memory Cache beholder den. Dette er mer korrekt oppførsel.

## Kjøre testene

### Forutsetninger

Testene krever at **Docker** kjører på maskinen, da Testcontainers starter Redis i en container.

### Kjør alle Redis-tester

```bash
# Fra root-katalogen
dotnet test FastEndpoints.UnitTests/FastEndpoints.UnitTests.csproj --filter "FullyQualifiedName~BaseRedisCacheStorageServiceTests"
```

### Kjør en spesifikk test

```bash
dotnet test --filter "FullyQualifiedName~BaseRedisCacheStorageServiceTests.CreateAsync_WhenIdEmpty_AssignsGuidAndStoresEntity_AndAddsToIndex"
```

### Kjør alle storage-tester (Memory + Redis)

```bash
dotnet test FastEndpoints.UnitTests/FastEndpoints.UnitTests.csproj --filter "FullyQualifiedName~Storage"
```

## Testytelse

Redis-testene tar lenger tid enn Memory Cache-testene fordi de:
1. Starter en Docker-container for hver test (via IAsyncLifetime)
2. Gjør faktiske nettverkskall til Redis
3. Stopper containeren etter hver test

**Forventet tid per test**: ~2-5 sekunder (avhengig av Docker-ytelse)

## Debugging

### Se Redis-innhold under testing

Legg til en breakpoint og inspiser Redis:

```csharp
var db = _redis.GetDatabase();
var keys = await db.ExecuteAsync("KEYS", "*");
Console.WriteLine($"Redis keys: {keys}");
```

### Testcontainers logger

Testcontainers logger til konsollen. Kjør med `--verbosity detailed` for å se Docker-output:

```bash
dotnet test --verbosity detailed --filter "FullyQualifiedName~BaseRedisCacheStorageServiceTests"
```

### Docker-containere

Testcontainers rydder automatisk opp, men hvis tester krasjer kan containere bli igjen:

```bash
# Se containere
docker ps -a | grep redis

# Rydd opp
docker rm -f $(docker ps -aq --filter "ancestor=redis:7-alpine")
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
      - name: Run tests
        run: dotnet test
```

Ubuntu runners har Docker installert som standard.

## Pakker

Testene bruker følgende NuGet-pakker:

```xml
<PackageReference Include="StackExchange.Redis" Version="2.8.16" />
<PackageReference Include="Testcontainers.Redis" Version="4.3.0" />
<PackageReference Include="FluentAssertions" Version="8.8.0" />
<PackageReference Include="xunit.v3" Version="3.2.2" />
```

## Best Practices

1. **Docker må kjøre** - Testene feiler hvis Docker ikke er tilgjengelig
2. **Isolering** - Hver test får sin egen Redis-instans via Testcontainers
3. **Rask feedback** - Hvis Docker er tregt, vurder å kjøre Memory Cache-tester lokalt
4. **CI/CD** - Kjør alle tester i CI/CD for å sikre Redis-kompatibilitet

## Fremtidige forbedringer

- [ ] Legg til tester for TTL (Time-To-Live)
- [ ] Test Redis Pub/Sub for events
- [ ] Test distribuert locking (RedLock)
- [ ] Performance-tester (sammenlign Memory vs Redis)
- [ ] Test failover-scenarier
- [ ] Test connection resilience
