using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace FastEndpoints.UnitTests.Services;

public class BaseMemoryCacheStorageServiceTests
{
    private sealed record TestEntity : IEntity
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TestMemoryCacheStorageService(IMemoryCache cache, IClock clock) : BaseMemoryCacheStorageService<TestEntity>(cache, clock)
    {
        protected override string Name => "Test";

        public Task<List<string>> Index() => GetIndexAsync();
    }

    private static (MemoryCache Cache, TestClock Clock, TestMemoryCacheStorageService Service) Sut()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new TestMemoryCacheStorageService(cache, clock);
        return (cache, clock, service);
    }

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
        var (cache, _, service) = Sut();

        // create one real entity
        var id = await service.CreateAsync(new TestEntity { Name = "A" }, ct);

        // manually create a stale index entry
        cache.Set("Test:index", new List<string> { id.ToString(), Guid.NewGuid().ToString() });

        var result = await service.GetAllAsync(ct);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(id);
    }

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
