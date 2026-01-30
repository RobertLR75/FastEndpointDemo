using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace FastEndpoints.IntegrationTests.Services.Storage;

/// <summary>
/// Enhetstester for BaseMemoryCacheStorageService.
/// Tester CRUD-operasjoner og caching-oppførsel med in-memory cache.
/// </summary>
public class BaseMemoryCacheStorageServiceTests
{
    /// <summary>
    /// Test-entitet som brukes i alle tester.
    /// </summary>
    private sealed record TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Konkret implementasjon av BaseMemoryCacheStorageService for testing.
    /// </summary>
    private sealed class TestMemoryCacheStorageService(IMemoryCache cache, IClock clock) : BaseMemoryCacheStorageService<TestEntity>(cache, clock)
    {
        protected override string Name => "Test";

        /// <summary>
        /// Eksponerer index for testing.
        /// </summary>
        public Task<List<string>> Index() => GetIndexAsync();
    }

    /// <summary>
    /// System Under Test - oppretter cache, clock og service for testing.
    /// </summary>
    private static (MemoryCache Cache, TestClock Clock, TestMemoryCacheStorageService Service) Sut()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new TestMemoryCacheStorageService(cache, clock);
        return (cache, clock, service);
    }

    /// <summary>
    /// Verifiserer at CreateAsync genererer en ny GUID når ID er tom,
    /// lagrer entiteten i cache, og legger til ID i index.
    /// </summary>
    [Fact]
    public async Task CreateAsync_WhenIdEmpty_AssignsGuidAndCachesEntity_AndAddsToIndex()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, clock, service) = Sut();
        var entity = new TestEntity { Id = Guid.Empty, Name = "A" };

        var now = clock.UtcNow;
        var id = await service.CreateAsync(entity, ct);

        id.Should().NotBe(Guid.Empty);
        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(now);

        var cached = cache.Get<TestEntity>($"Test:{id}");
        cached.Should().NotBeNull();
        cached!.Name.Should().Be("A");

        var index = await service.Index();
        index.Should().Contain(id.ToString());
    }

    /// <summary>
    /// Verifiserer at CreateAsync beholder eksisterende ID når den er oppgitt.
    /// </summary>
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

    /// <summary>
    /// Verifiserer at GetAsync returnerer null når entiteten ikke finnes i cache.
    /// </summary>
    [Fact]
    public async Task GetAsync_WhenMissing_ReturnsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var result = await service.GetAsync(Guid.NewGuid(), ct);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifiserer at GetAsync returnerer entiteten når den finnes i cache.
    /// </summary>
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

    /// <summary>
    /// Verifiserer at GetAllAsync returnerer tom liste når index er tom.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenIndexEmpty_ReturnsEmpty()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var result = await service.GetAllAsync(ct);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifiserer at GetAllAsync filtrerer ut gamle ID-er fra index
    /// når tilhørende entiteter ikke lenger finnes i cache.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WhenIndexHasStaleIds_FiltersMissingEntities()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();

        // create one real entity
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        // manually create a stale index entry
        cache.Set("Test:index", new List<string> { id.ToString(), Guid.NewGuid().ToString() });

        var result = await service.GetAllAsync(ct);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(id);
    }

    /// <summary>
    /// Verifiserer at UpdateAsync setter UpdatedAt-tidspunkt og
    /// overskriver eksisterende entitet i cache.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt_AndOverwritesCachedEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, clock, service) = Sut();
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        var entity = await service.GetAsync(id, ct);
        entity.Should().NotBeNull();

        entity!.Name = "B";

        clock.Advance(TimeSpan.FromSeconds(1));
        var now = clock.UtcNow;
        await service.UpdateAsync(entity, ct);

        entity.UpdatedAt.Should().Be(now);

        var cached = cache.Get<TestEntity>($"Test:{id}");
        cached!.Name.Should().Be("B");
    }

    /// <summary>
    /// Verifiserer at DeleteAsync fjerner entiteten fra cache,
    /// men lar index-oppføringen være (dokumenterer nåværende oppførsel).
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesCachedEntity_ButIndexRemains()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        await service.DeleteAsync(id, ct);

        cache.Get<TestEntity>($"Test:{id}").Should().BeNull();
        var index = await service.Index();
        index.Should().Contain(id.ToString()); // documents current behavior
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

        var index = await service.Index();
        index.Should().ContainSingle(x => x == id.ToString());

        // last write wins
        var fetched = await service.GetAsync(id, ct);
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("B");
    }

    /// <summary>
    /// Verifiserer at DeleteAsync er idempotent (kan kalles flere ganger uten feil),
    /// og at GetAllAsync filtrerer ut manglende entiteter fra index.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_IsIdempotent_AndGetAllFiltersStaleIndexEntries()
    {
        var ct = TestContext.Current.CancellationToken;
        var (_, _, service) = Sut();

        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        // delete twice should not throw
        await service.Invoking(s => s.DeleteAsync(id, ct)).Should().NotThrowAsync();
        await service.Invoking(s => s.DeleteAsync(id, ct)).Should().NotThrowAsync();

        // index still contains id (documented), but GetAll filters out missing entity
        (await service.Index()).Should().Contain(id.ToString());
        (await service.GetAllAsync(ct)).Should().BeEmpty();
    }
}
