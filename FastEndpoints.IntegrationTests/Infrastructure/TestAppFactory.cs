using FastEndpointDemo;
using FastEndpointDemo.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FastEndpoints.IntegrationTests.Infrastructure;

public sealed class TestAppFactory : WebApplicationFactory<Program>
{
    private static readonly object ResetLock = new();
    
    public void ResetState()
    {
        lock (ResetLock)
        {
            using var scope = Services.CreateScope();
            var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
        
            // Clear the person index explicitly before compacting
            cache.Remove("Person:index");
        
            if (cache is MemoryCache memoryCache)
                memoryCache.Compact(1.0);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Remove ONLY PersonStorageInitializerService (seeding) to keep the host deterministic.
            for (var i = services.Count - 1; i >= 0; i--)
            {
                var d = services[i];
                if (d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(PersonStorageInitializerService))
                    services.RemoveAt(i);
            }
        });
    }
}
