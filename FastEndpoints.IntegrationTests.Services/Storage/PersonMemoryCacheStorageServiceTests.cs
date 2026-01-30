using FastEndpointDemo.Services.Models;
using FastEndpointDemo.Services.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace FastEndpoints.IntegrationTests.Services.Storage;

/// <summary>
/// Integrasjonstester for PersonMemoryCacheStorageService.
/// Tester at konkret implementasjon fungerer med PersonModel.
/// </summary>
public class PersonMemoryCacheStorageServiceTests
{
    /// <summary>
    /// Verifiserer at en person kan opprettes og hentes tilbake med alle felter intakte.
    /// </summary>
    [Fact]
    public async Task CreateAndGet_Roundtrip_Works()
    {
        var ct = TestContext.Current.CancellationToken;
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new PersonMemoryCacheStorageService(cache, clock);

        var now = clock.UtcNow;
        var id = await service.CreateAsync(new PersonModel { Id = Guid.Empty, FirstName = "John", LastName = "Doe" }, ct);

        var result = await service.GetAsync(id, ct);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.CreatedAt.Should().Be(now);
    }

    /// <summary>
    /// Verifiserer at GetAll returnerer alle opprettede personer.
    /// </summary>
    [Fact]
    public async Task GetAll_ReturnsCreatedEntities()
    {
        var ct = TestContext.Current.CancellationToken;
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new PersonMemoryCacheStorageService(cache, clock);

        await service.CreateAsync(new PersonModel { FirstName = "John", LastName = "Doe" }, ct);
        await service.CreateAsync(new PersonModel { FirstName = "Jane", LastName = "Smith" }, ct);

        var persons = await service.GetAllAsync(ct);

        persons.Should().HaveCount(2);
        persons.Select(p => p.FirstName).Should().BeEquivalentTo(new[] { "John", "Jane" });
    }

    /// <summary>
    /// Verifiserer at servicen bruker riktig navneprefiks ("Person:") for index-nøkkelen.
    /// </summary>
    [Fact]
    public async Task NamePrefix_UsesPersonIndexKey()
    {
        var ct = TestContext.Current.CancellationToken;
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var service = new PersonMemoryCacheStorageService(cache, clock);

        await service.CreateAsync(new PersonModel { FirstName = "John", LastName = "Doe" }, ct);

        var index = cache.Get<List<string>>("Person:index");
        index.Should().NotBeNull();
        index!.Should().HaveCount(1);
    }
}
