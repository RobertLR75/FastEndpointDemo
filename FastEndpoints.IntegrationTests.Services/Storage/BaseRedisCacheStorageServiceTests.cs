using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Storage;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace FastEndpoints.IntegrationTests.Services.Storage;

/// <summary>
/// Unit tester for BaseRedisCacheStorageService.
/// Bruker Testcontainers for å kjøre en ekte Redis-instans under testing.
/// </summary>
public class BaseRedisCacheStorageServiceTests : IAsyncLifetime
{
    private RedisContainer? _redisContainer;
    private IConnectionMultiplexer? _redis;

    private sealed record TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestRedisCacheStorageService(IConnectionMultiplexer redis, IClock clock) 
        : BaseRedisCacheStorageService<TestEntity>(redis, clock)
    {
        protected override string Name => "Test";

        public Task<List<string>> Index() => GetIndexAsync();
    }

    /// <summary>
    /// Initialiserer Redis container før hver test.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        _redisContainer = new RedisBuilder("redis:7-alpine")
            .Build();

        await _redisContainer.StartAsync();
        
        var connectionString = _redisContainer.GetConnectionString();
        _redis = await ConnectionMultiplexer.ConnectAsync(connectionString, null);
    }

    /// <summary>
    /// Rydder opp og stopper Redis container etter hver test.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_redis != null)
        {
            await _redis.CloseAsync();
            _redis.Dispose();
        }

        if (_redisContainer != null)
        {
            await _redisContainer.StopAsync();
            await _redisContainer.DisposeAsync();
        }
    }

    private (IConnectionMultiplexer Redis, TestClock Clock, TestRedisCacheStorageService Service) Sut()
    {
        if (_redis == null)
            throw new InvalidOperationException("Redis is not initialized");

        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new TestRedisCacheStorageService(_redis, clock);
        return (_redis, clock, service);
    }

    [Fact]
    public async Task CreateAsync_WhenIdEmpty_AssignsGuidAndStoresEntity_AndAddsToIndex()
    {
        var ct = TestContext.Current.CancellationToken;
        var (redis, clock, service) = Sut();
        var entity = new TestEntity { Id = Guid.Empty, Name = "A" };

        var now = clock.UtcNow;
        var id = await service.CreateAsync(entity, ct);

        id.Should().NotBe(Guid.Empty);
        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(now);

        // Verifiser at entiteten er lagret i Redis
        var db = redis.GetDatabase();
        var json = await db.StringGetAsync($"Test:{id}");
        json.HasValue.Should().BeTrue();
        json.ToString().Should().Contain("\"name\":\"A\"");

        // Verifiser at ID er lagt til i index
        var index = await service.Index();
        index.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task CreateAsync_WhenIdProvided_PreservesId()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "A" };

        var createdId = await service.CreateAsync(entity, ct);

        createdId.Should().Be(id);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetAsync_WhenMissing_ReturnsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var result = await service.GetAsync(Guid.NewGuid(), ct);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WhenExists_ReturnsEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();
        var entity = new TestEntity { Name = "A" };
        var id = await service.CreateAsync(entity, ct);

        var result = await service.GetAsync(id, ct);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Name.Should().Be("A");
    }

    [Fact]
    public async Task GetAllAsync_WhenIndexEmpty_ReturnsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var result = await service.GetAllAsync(ct);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WhenIndexHasStaleIds_FiltersMissingEntities()
    {
        var ct = TestContext.Current.CancellationToken;
        var (redis, _, service) = Sut();

        // Opprett en ekte entitet
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        // Legg til en stale ID manuelt i Redis Set
        var db = redis.GetDatabase();
        await db.SetAddAsync("Test:index", Guid.NewGuid().ToString());

        var result = await service.GetAllAsync(ct);

        // Skal bare returnere den ekte entiteten, ikke den stale
        result.Should().HaveCount(1);
        result[0].Id.Should().Be(id);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndOverwritesStoredEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (redis, clock, service) = Sut();
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        var entity = await service.GetAsync(id, ct);
        entity.Should().NotBeNull();

        entity!.Name = "B";

        clock.Advance(TimeSpan.FromSeconds(1));
        var now = clock.UtcNow;
        await service.UpdateAsync(entity, ct);

        entity.UpdatedAt.Should().Be(now);

        // Verifiser at oppdateringen er lagret i Redis
        var db = redis.GetDatabase();
        var json = await db.StringGetAsync($"Test:{id}");
        json.ToString().Should().Contain("\"name\":\"B\"");

        // Verifiser ved å hente fra service
        var fetched = await service.GetAsync(id, ct);
        fetched!.Name.Should().Be("B");
    }

    [Fact]
    public async Task DeleteAsync_RemovesStoredEntity_AndRemovesFromIndex()
    {
        var ct = TestContext.Current.CancellationToken;
        var (redis, _, service) = Sut();
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        await service.DeleteAsync(id, ct);

        // Verifiser at entiteten er slettet fra Redis
        var db = redis.GetDatabase();
        var json = await db.StringGetAsync($"Test:{id}");
        json.HasValue.Should().BeFalse();

        // Verifiser at ID er fjernet fra index (forskjell fra Memory Cache!)
        var index = await service.Index();
        index.Should().NotContain(id.ToString());
    }

    /// <summary>
    /// Verifiserer at CreateAsync ikke lager duplikate index-oppføringer
    /// når den kalles flere ganger med samme ID. Siste skriving vinner.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenCalledTwiceWithSameId_DoesNotDuplicateIndexEntry()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var id = Guid.NewGuid();
        var entity1 = new TestEntity { Id = id, Name = "A" };
        var entity2 = new TestEntity { Id = id, Name = "B" };

        (await service.CreateAsync(entity1, ct)).Should().Be(id);
        (await service.CreateAsync(entity2, ct)).Should().Be(id);

        // Redis Set gir automatisk deduplisering
        var index = await service.Index();
        index.Should().ContainSingle(x => x == id.ToString());

        // Last write wins
        var fetched = await service.GetAsync(id, ct);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("B");
    }

    /// <summary>
    /// Verifiserer at DeleteAsync er idempotent og fjerner ID fra index.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_IsIdempotent_AndRemovesFromIndex()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        // Slett to ganger skal ikke kaste exception
        await service.Invoking(s => s.DeleteAsync(id, ct)).Should().NotThrowAsync();
        await service.Invoking(s => s.DeleteAsync(id, ct)).Should().NotThrowAsync();

        // Verifiser at ID er fjernet fra index (forskjell fra Memory Cache!)
        (await service.Index()).Should().NotContain(id.ToString());
        (await service.GetAllAsync(ct)).Should().BeEmpty();
    }

    /// <summary>
    /// Verifiserer at GetAllAsync kan hente flere entiteter parallelt.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities_InParallel()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        // Opprett flere entiteter
        var id1 = await service.CreateAsync(new TestEntity { Name = "A" }, ct);
        var id2 = await service.CreateAsync(new TestEntity { Name = "B" }, ct);
        var id3 = await service.CreateAsync(new TestEntity { Name = "C" }, ct);

        var result = await service.GetAllAsync(ct);

        result.Should().HaveCount(3);
        result.Select(e => e.Name).Should().BeEquivalentTo(new[] { "A", "B", "C" });
        result.Select(e => e.Id).Should().BeEquivalentTo(new[] { id1, id2, id3 });
    }

    /// <summary>
    /// Verifiserer at JSON-serialisering bevarer DateTimeOffset med presisjon.
    /// </summary>
    [Fact]
    public async Task JsonSerialization_PreservesDateTimeOffsetPrecision()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var specificTime = new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero);
        var clockWithSpecificTime = new TestClock(specificTime);
        var serviceWithTime = new TestRedisCacheStorageService(_redis!, clockWithSpecificTime);

        var entity = new TestEntity { Name = "TimeTest" };
        var id = await serviceWithTime.CreateAsync(entity, ct);

        var fetched = await serviceWithTime.GetAsync(id, ct);

        fetched.Should().NotBeNull();
        fetched!.CreatedAt.Should().Be(specificTime);
    }

    /// <summary>
    /// Verifiserer at UpdateAsync kan opprette entitet selv om CreateAsync ikke er kalt først.
    /// Merk: Entiteten blir ikke lagt til i index.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenEntityDoesNotExist_StillCreatesEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, clock, service) = Sut();

        var id = Guid.NewGuid();
        var entity = new TestEntity 
        { 
            Id = id, 
            Name = "DirectUpdate",
            CreatedAt = clock.UtcNow 
        };

        // Kall UpdateAsync uten å kalle CreateAsync først
        await service.UpdateAsync(entity, ct);

        // Entiteten skal eksistere i Redis, men ikke i index
        var fetched = await service.GetAsync(id, ct);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("DirectUpdate");
        fetched.UpdatedAt.Should().NotBeNull();

        // Index skal ikke inneholde ID (kun CreateAsync legger til i index)
        var index = await service.Index();
        index.Should().NotContain(id.ToString());
    }

    /// <summary>
    /// Verifiserer at flere service-instanser deler samme Redis index.
    /// </summary>
    [Fact]
    public async Task MultipleServices_ShareSameRedisIndex()
    {
        var ct = TestContext.Current.CancellationToken;
        var (redis, clock, service1) = Sut();
        var service2 = new TestRedisCacheStorageService(redis, clock);

        // Opprett entitet via service1
        var id = await service1.CreateAsync(new TestEntity { Name = "Shared" }, ct);

        // Hent via service2
        var fetched = await service2.GetAsync(id, ct);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Shared");

        // Begge skal se samme index
        var index1 = await service1.Index();
        var index2 = await service2.Index();
        index1.Should().BeEquivalentTo(index2);
    }

    /// <summary>
    /// Verifiserer at Redis Set automatisk dedupliserer index-oppføringer.
    /// </summary>
    [Fact]
    public async Task RedisSet_AutomaticallyDeduplicatesIndexEntries()
    {
        var (redis, _, service) = Sut();

        var id = Guid.NewGuid();
        
        // Legg til samme ID flere ganger manuelt i Redis Set
        var db = redis.GetDatabase();
        await db.SetAddAsync("Test:index", id.ToString());
        await db.SetAddAsync("Test:index", id.ToString());
        await db.SetAddAsync("Test:index", id.ToString());

        var index = await service.Index();

        // Redis Set skal bare inneholde én forekomst
        index.Should().ContainSingle(x => x == id.ToString());
    }
}
