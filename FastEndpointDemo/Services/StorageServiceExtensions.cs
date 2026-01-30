using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Storage;
using MongoDB.Driver;
using StackExchange.Redis;

namespace FastEndpointDemo.Services;

/// <summary>
/// Extension methods for å konfigurere storage services i dependency injection.
/// Gir fleksibilitet til å velge mellom in-memory cache, Redis og MongoDB.
/// </summary>
public static class StorageServiceExtensions
{
    /// <summary>
    /// Legger til storage services basert på konfigurasjon.
    /// Leser "Storage:Type" fra appsettings.json for å velge implementasjon.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Legg til clock
        services.AddSingleton<IClock, SystemClock>();

        // Les storage type fra konfigurasjon
        var storageType = configuration["Storage:Type"]?.ToLowerInvariant() ?? "memory";

        switch (storageType)
        {
            case "redis":
                AddRedisStorage(services, configuration);
                break;
            case "mongodb":
                AddMongoDbStorage(services, configuration);
                break;
            case "memory":
            default:
                AddMemoryStorage(services);
                break;
        }

        return services;
    }

    /// <summary>
    /// Legger til in-memory cache storage.
    /// Rask, men ikke persistent og ikke delt mellom instanser.
    /// </summary>
    private static void AddMemoryStorage(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped<IPersonStorageService, PersonMemoryCacheStorageService>();
    }

    /// <summary>
    /// Legger til Redis cache storage.
    /// Persistent, distribuert og delt mellom instanser.
    /// </summary>
    private static void AddRedisStorage(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        
        // Konfigurer Redis connection
        var redis = ConnectionMultiplexer.Connect(connectionString);
        services.AddSingleton<IConnectionMultiplexer>(redis);

        // Registrer Redis storage service
        services.AddScoped<IPersonStorageService, PersonRedisCacheStorageService>();
    }

    /// <summary>
    /// Legger til MongoDB storage.
    /// Full database med queries, indexes og persistence.
    /// </summary>
    private static void AddMongoDbStorage(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["MongoDB:ConnectionString"] ?? "mongodb://localhost:27017";
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "FastEndpointDemo";
        
        // Konfigurer MongoDB connection
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        
        services.AddSingleton<IMongoClient>(client);
        services.AddSingleton(database);

        // Registrer MongoDB storage service
        services.AddScoped<IPersonStorageService, PersonMongoDbStorageService>();
    }

    /// <summary>
    /// Eksplisitt registrering av Memory Cache storage.
    /// </summary>
    public static IServiceCollection AddMemoryCacheStorage(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        AddMemoryStorage(services);
        return services;
    }

    /// <summary>
    /// Eksplisitt registrering av Redis storage.
    /// </summary>
    public static IServiceCollection AddRedisCacheStorage(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddSingleton<IClock, SystemClock>();
        
        var redis = ConnectionMultiplexer.Connect(connectionString);
        services.AddSingleton<IConnectionMultiplexer>(redis);
        services.AddScoped<IPersonStorageService, PersonRedisCacheStorageService>();
        
        return services;
    }

    /// <summary>
    /// Eksplisitt registrering av MongoDB storage.
    /// </summary>
    public static IServiceCollection AddMongoDbStorage(
        this IServiceCollection services,
        string connectionString,
        string databaseName = "FastEndpointDemo")
    {
        services.AddSingleton<IClock, SystemClock>();
        
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        
        services.AddSingleton<IMongoClient>(client);
        services.AddSingleton(database);
        services.AddScoped<IPersonStorageService, PersonMongoDbStorageService>();
        
        return services;
    }
}
