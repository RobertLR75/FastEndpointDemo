using FastEndpointDemo.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FastEndpoints.UnitTests.Services;

public class PersonStorageInitializerServiceTests
{
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

    [Fact]
    public async Task StopAsync_Completes()
    {
        var ct = TestContext.Current.CancellationToken;

        var provider = new ServiceCollection().BuildServiceProvider();
        var initializer = new PersonStorageInitializerService(provider);

        await initializer.Invoking(i => i.StopAsync(ct))
            .Should().NotThrowAsync();
    }

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
