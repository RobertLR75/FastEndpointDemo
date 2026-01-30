using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Models;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace FastEndpoints.UnitTests.Services;

/// <summary>
/// "Contract" tests that assert common expectations across IStorageService implementations.
/// These are intentionally small but help lock in service semantics.
/// </summary>
public class StorageServiceContractTests
{
    private static PersonModel Person(string firstName = "A", string lastName = "B", Guid? id = null)
        => new() { Id = id ?? Guid.Empty, FirstName = firstName, LastName = lastName };

    private static (MemoryCache Cache, TestClock Clock, IPersonStorageService Service) Sut()
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new TestClock(DateTimeOffset.UtcNow);
        IPersonStorageService service = new PersonMemoryCacheStorageService(cache, clock);
        return (cache, clock, service);
    }

    [Fact]
    public void PersonModel_Implements_IEntity()
    {
        typeof(PersonModel).Should().BeAssignableTo<IEntity>();
    }

    [Fact]
    public async Task CreateAsync_AssignsIdAndSetsCreatedAt_AndUpdatedAtIsNull()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, clock, service) = Sut();
        using var _ = cache;

        var now = clock.UtcNow;
        var id = await service.CreateAsync(Person(), ct);

        id.Should().NotBe(Guid.Empty);

        var entity = await service.GetAsync(id, ct);
        entity.Should().NotBeNull();
        entity!.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(now);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenIdProvided_PreservesId()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();
        using var _1 = cache;

        var id = Guid.NewGuid();
        var createdId = await service.CreateAsync(Person(id: id), ct);

        createdId.Should().Be(id);
        (await service.GetAsync(id, ct)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllAsync_ContainsCreatedEntity_AndDeleteFiltersStaleIndexEntry()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();
        using var _1 = cache;

        var id = await service.CreateAsync(Person(), ct);

        (await service.GetAllAsync(ct)).Should().ContainSingle(p => p.Id == id);

        // delete removes the cached item; index remains, but GetAll should filter missing entries
        await service.DeleteAsync(id, ct);

        (await service.GetAsync(id, ct)).Should().BeNull();
        (await service.GetAllAsync(ct)).Should().NotContain(p => p.Id == id);
    }

    [Fact]
    public async Task UpdateAsync_OverwritesEntityAndSetsUpdatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, clock, service) = Sut();
        using var _ = cache;

        var id = await service.CreateAsync(Person(), ct);
        var entity = await service.GetAsync(id, ct);
        entity.Should().NotBeNull();

        entity!.FirstName = "AA";

        clock.Advance(TimeSpan.FromMinutes(1));
        var now = clock.UtcNow;
        await service.UpdateAsync(entity, ct);

        var updated = await service.GetAsync(id, ct);
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("AA");
        updated.UpdatedAt.Should().Be(now);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeCreatedAt()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, clock, service) = Sut();
        using var _ = cache;

        var id = await service.CreateAsync(Person(), ct);
        var before = await service.GetAsync(id, ct);
        before.Should().NotBeNull();

        var createdAt = before!.CreatedAt;

        before.LastName = "BB";
        clock.Advance(TimeSpan.FromMinutes(1));
        await service.UpdateAsync(before, ct);

        var after = await service.GetAsync(id, ct);
        after.Should().NotBeNull();

        after!.CreatedAt.Should().Be(createdAt);
        after.UpdatedAt.Should().Be(clock.UtcNow);
        after.LastName.Should().Be("BB");
    }

    [Fact]
    public async Task StorageService_SetsExpectedNamePrefixKeys()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();
        using var _1 = cache;

        var id = await service.CreateAsync(Person(), ct);

        cache.Get<List<string>>("Person:index").Should().ContainSingle(id.ToString());
        cache.Get<PersonModel>($"Person:{id}").Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenCalledTwiceWithSameProvidedId_DoesNotDuplicateIndex_AndOverwritesCachedEntity()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();
        using var _1 = cache;

        var id = Guid.NewGuid();

        await service.CreateAsync(Person(firstName: "A", lastName: "B", id: id), ct);
        await service.CreateAsync(Person(firstName: "AA", lastName: "BB", id: id), ct);

        // index should contain the id only once
        var index = cache.Get<List<string>>("Person:index");
        index.Should().NotBeNull();
        index!.Should().ContainSingle(x => x == id.ToString());

        // cached entity should reflect last write
        var entity = await service.GetAsync(id, ct);
        entity.Should().NotBeNull();
        entity!.FirstName.Should().Be("AA");
        entity.LastName.Should().Be("BB");
    }

    [Fact]
    public async Task CreateAsync_WhenIndexAlreadyContainsId_DoesNotDuplicateIndexEntry()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, _, service) = Sut();
        using var _1 = cache;

        var id = Guid.NewGuid();

        // prepopulate index as if it already contained the id
        cache.Set("Person:index", new List<string> { id.ToString() });

        await service.CreateAsync(Person(firstName: "A", lastName: "B", id: id), ct);

        var index = cache.Get<List<string>>("Person:index");
        index.Should().NotBeNull();
        index!.Should().ContainSingle(x => x == id.ToString());
    }

    [Fact]
    public async Task CreateAsync_WhenReCreatingWithSameId_OverwritesCreatedAt_DocumentsCurrentBehavior()
    {
        var ct = TestContext.Current.CancellationToken;
        var (cache, clock, service) = Sut();
        using var _ = cache;

        var id = Guid.NewGuid();

        var now1 = clock.UtcNow;
        await service.CreateAsync(Person(id: id), ct);
        (await service.GetAsync(id, ct))!.CreatedAt.Should().Be(now1);

        clock.Advance(TimeSpan.FromMinutes(1));
        var now2 = clock.UtcNow;
        await service.CreateAsync(Person(id: id), ct);

        var second = await service.GetAsync(id, ct);
        second.Should().NotBeNull();
        second!.CreatedAt.Should().Be(now2);
    }
}
