using FastEndpointDemo.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace FastEndpointDemo.Services;

/// <summary>
/// Abstrakt basisklasse for in-memory cache-basert storage av entiteter.
/// Implementerer CRUD-operasjoner med thread-safe index-håndtering.
/// Bruker IMemoryCache for å lagre entiteter og en index for rask oppslag.
/// </summary>
/// <typeparam name="T">Type entitet som skal lagres (må implementere IEntity)</typeparam>
public abstract class BaseMemoryCacheStorageService<T>(IMemoryCache cache, IClock clock) : IStorageService<T>
    where T : class, IEntity
{
    /// <summary>
    /// Prefix for cache-nøkler (f.eks. "Person" gir nøkler som "Person:index" og "Person:{id}").
    /// Må implementeres av avledede klasser.
    /// </summary>
    protected abstract string Name { get; }

    // Delt lock per entitetstype for å holde index-oppdateringer thread-safe på tvers av requests.
    private static readonly ConcurrentDictionary<string, object> IndexLocks = new();
    private object IndexLock => IndexLocks.GetOrAdd(Name, _ => new object());

    #region Index Management

    /// <summary>
    /// Oppdaterer index med ny entitets-ID.
    /// Thread-safe via lock for å unngå concurrent list modification.
    /// </summary>
    /// <param name="id">ID på entiteten som skal legges til i index</param>
    private Task UpdateIndexAsync(string id)
    {
        lock (IndexLock)
        {
            var ids = cache.Get<List<string>>(Name + ":index");
            if (ids == null)
            {
                // Opprett ny index hvis den ikke eksisterer
                ids = new List<string> { id };
                cache.Set(Name + ":index", ids);
            }
            else if (!ids.Contains(id))
            {
                // Opprett ny liste for å unngå problemer hvis den cachede listen itereres andre steder
                var newIds = new List<string>(ids) { id };
                cache.Set(Name + ":index", newIds);
            }
        }

        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Henter alle entitets-IDer fra index.
    /// Thread-safe via lock og returnerer en kopi av listen.
    /// </summary>
    /// <returns>Liste med alle entitets-IDer i storage</returns>
    protected Task<List<string>> GetIndexAsync()
    {
        lock (IndexLock)
        {
            var ids = cache.Get<List<string>>(Name + ":index") ?? [];
            return Task.FromResult(ids.ToList());
        }
    }

    #endregion

    /// <summary>
    /// Oppretter en ny entitet i storage.
    /// Genererer ny ID hvis den er tom, setter CreatedAt-tidspunkt og legger til i index.
    /// </summary>
    /// <param name="entity">Entitet som skal opprettes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ID på den opprettede entiteten</returns>
    public async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        // Generer ny Version 7 GUID hvis ID er tom, ellers behold eksisterende ID
        entity.Id = entity.Id == Guid.Empty ? entity.Id = Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = clock.UtcNow;
        
        // Lagre entitet i cache
        cache.Set(Name + $":{entity.Id}", entity);

        // Legg til ID i index
        await UpdateIndexAsync(entity.Id.ToString());
        return entity.Id;
    }

    /// <summary>
    /// Oppdaterer en eksisterende entitet i storage.
    /// Setter UpdatedAt-tidspunkt og overskriver entiteten i cache.
    /// </summary>
    /// <param name="entity">Entitet med oppdaterte verdier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = clock.UtcNow;
        var id = entity.Id;
        cache.Set(Name + $":{id}", entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sletter en entitet fra storage basert på ID.
    /// Merk: Fjerner ikke ID fra index, men entiteten vil ikke kunne hentes.
    /// </summary>
    /// <param name="id">ID på entiteten som skal slettes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        cache.Remove(Name + $":{id}");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Henter en enkelt entitet fra storage basert på ID.
    /// </summary>
    /// <param name="id">ID på entiteten som skal hentes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entitet hvis funnet, null ellers</returns>
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var cachedItems = cache.Get<T>(Name + $":{id}");
        return Task.FromResult(cachedItems);
    }

    /// <summary>
    /// Henter alle entiteter fra storage.
    /// Itererer gjennom index og henter hver entitet fra cache.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liste med alle entiteter som eksisterer i storage</returns>
    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        var ids = await GetIndexAsync();

        return ids.Select(id => cache.Get<T>(Name + $":{id}")).OfType<T>().ToList();
    }
}