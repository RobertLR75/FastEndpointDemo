using FastEndpointDemo.Services.Interfaces;
using FastEndpointDemo.Services.Models;
using StackExchange.Redis;

namespace FastEndpointDemo.Services.Storage;

/// <summary>
/// Redis-basert implementasjon av person storage.
/// Bruker BaseRedisCacheStorageService for all CRUD-logikk med "Person" som prefix.
/// Gir distribuert caching på tvers av flere applikasjonsinstanser.
/// </summary>
public class PersonRedisCacheStorageService(IConnectionMultiplexer redis, IClock clock) 
    : BaseRedisCacheStorageService<PersonModel>(redis, clock), IPersonStorageService
{
    /// <summary>
    /// Prefix for Redis-nøkler (gir nøkler som "Person:index" og "Person:{id}").
    /// </summary>
    protected override string Name { get; } = "Person";
}
