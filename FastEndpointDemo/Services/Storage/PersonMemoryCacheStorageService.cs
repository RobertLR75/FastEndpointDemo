using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Models;
using Microsoft.Extensions.Caching.Memory;

namespace FastEndpointDemo.Services.Storage;

/// <summary>
/// Interface for person-spesifikk storage service.
/// Arver alle CRUD-operasjoner fra IStorageService med PersonModel som entitetstype.
/// </summary>
public interface IPersonStorageService : IStorageService<PersonModel>
{
}

/// <summary>
/// In-memory cache-basert implementasjon av person storage.
/// Bruker BaseMemoryCacheStorageService for all CRUD-logikk med "Person" som prefix.
/// </summary>
public class PersonMemoryCacheStorageService(IMemoryCache cache, IClock clock) : BaseMemoryCacheStorageService<PersonModel>(cache, clock), IPersonStorageService
{
    /// <summary>
    /// Prefix for cache-nøkler (gir nøkler som "Person:index" og "Person:{id}").
    /// </summary>
    protected override string Name { get; } = "Person";
}