using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace FastEndpoints.IntegrationTests.Services;

/// <summary>
/// Integrasjonstester for PersonStorageInitializerService.
/// Tester at servicen seeder fire personer ved oppstart med ekte DI-container.
/// </summary>
public class PersonStorageInitializerServiceTests
{
    /// <summary>
    /// Verifiserer at StartAsync seeder fire personer når den kalles med en ekte DI-container.
    /// </summary>
    [Fact]
    public async Task StartAsync_SeedsFourPersons_UsingRealDIAndCache()
    {
        var ct = TestContext.Current.CancellationToken;

        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<FastEndpointDemo.Services.Interfaces.IClock>(new TestClock(DateTimeOffset.UtcNow));
        services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();

        var provider = services.BuildServiceProvider();

        var initializer = new PersonStorageInitializerService(provider);
        await initializer.StartAsync(ct);

        // Resolve a scoped service and verify
        await using var scope = provider.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IPersonStorageService>();
        var persons = await storage.GetAllAsync(ct);

        persons.Should().HaveCount(4);
        persons.All(p => p.Id != Guid.Empty).Should().BeTrue();
        persons.Select(p => p.FirstName).Should().BeEquivalentTo(new[] { "John", "Jane", "Alice", "Bob" });
    }

    /// <summary>
    /// Verifiserer at StopAsync fullfører uten å kaste unntak.
    /// </summary>
    [Fact]
    public async Task StopAsync_Completes()
    {
        var ct = TestContext.Current.CancellationToken;

        var provider = new ServiceCollection().BuildServiceProvider();
        var initializer = new PersonStorageInitializerService(provider);

        await initializer.Invoking(i => i.StopAsync(ct))
            .Should().NotThrowAsync();
    }

    /// <summary>
    /// Verifiserer at StartAsync kan kalles to ganger og seeder 8 personer totalt.
    /// Dokumenterer nåværende oppførsel (ikke idempotent).
    /// </summary>
    [Fact]
    public async Task StartAsync_WhenCalledTwice_SeedsTwice_DocumentsCurrentBehavior()
    {
        var ct = TestContext.Current.CancellationToken;

        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddSingleton<FastEndpointDemo.Services.Interfaces.IClock>(new TestClock(DateTimeOffset.UtcNow));
        services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();

        var provider = services.BuildServiceProvider();

        var initializer = new PersonStorageInitializerService(provider);
        await initializer.StartAsync(ct);
        await initializer.StartAsync(ct);

        await using var scope = provider.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IPersonStorageService>();
        var persons = await storage.GetAllAsync(ct);

        persons.Should().HaveCount(8);
    }
}
