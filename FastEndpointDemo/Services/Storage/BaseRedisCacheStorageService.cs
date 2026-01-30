using System.Collections.Concurrent;
using System.Text.Json;
using FastEndpointDemo.Services.Interfaces;
using StackExchange.Redis;

namespace FastEndpointDemo.Services.Storage;

/// <summary>
/// Abstrakt basisklasse for Redis-basert storage av entiteter.
/// Implementerer CRUD-operasjoner med thread-safe index-håndtering.
/// Bruker Redis for distribuert caching og persistens på tvers av applikasjonsinstanser.
/// </summary>
/// <typeparam name="T">Type entitet som skal lagres (må implementere IEntity)</typeparam>
public abstract class BaseRedisCacheStorageService<T>(IConnectionMultiplexer redis, IClock clock) : IStorageService<T>
    where T : class, IEntity
{
    private readonly IDatabase _db = redis.GetDatabase();
    
    /// <summary>
    /// Prefix for Redis-nøkler (f.eks. "Person" gir nøkler som "Person:index" og "Person:{id}").
    /// Må implementeres av avledede klasser.
    /// </summary>
    protected abstract string Name { get; }

    // Delt lock per entitetstype for å holde index-oppdateringer thread-safe på tvers av requests.
    // Merk: For distribuerte systemer bør man vurdere distribuert locking (f.eks. RedLock).
    private static readonly ConcurrentDictionary<string, object> IndexLocks = new();
    private object IndexLock => IndexLocks.GetOrAdd(Name, _ => new object());

    #region Index Management

    /// <summary>
    /// Oppdaterer Redis Set med ny entitets-ID.
    /// Bruker Redis SADD for atomisk operasjon.
    /// </summary>
    /// <param name="id">ID på entiteten som skal legges til i index</param>
    private async Task UpdateIndexAsync(string id)
    {
        // Bruk Redis Set for å lagre index - automatisk deduplisering
        await _db.SetAddAsync(Name + ":index", id);
    }
    
    /// <summary>
    /// Henter alle entitets-IDer fra Redis Set.
    /// Returnerer en liste med alle IDer i storage.
    /// </summary>
    /// <returns>Liste med alle entitets-IDer i storage</returns>
    protected async Task<List<string>> GetIndexAsync()
    {
        var members = await _db.SetMembersAsync(Name + ":index");
        return members.Select(m => m.ToString()).ToList();
    }

    /// <summary>
    /// Fjerner en ID fra Redis Set index.
    /// </summary>
    /// <param name="id">ID som skal fjernes fra index</param>
    private async Task RemoveFromIndexAsync(string id)
    {
        await _db.SetRemoveAsync(Name + ":index", id);
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serialiserer entitet til JSON for lagring i Redis.
    /// </summary>
    /// <param name="entity">Entitet som skal serialiseres</param>
    /// <returns>JSON-streng</returns>
    private static string Serialize(T entity)
    {
        return JsonSerializer.Serialize(entity, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
    }

    /// <summary>
    /// Deserialiserer JSON fra Redis til entitet.
    /// </summary>
    /// <param name="json">JSON-streng fra Redis</param>
    /// <returns>Deserialisert entitet, eller null hvis JSON er null/tom</returns>
    private static T? Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    #endregion

    /// <summary>
    /// Oppretter en ny entitet i Redis.
    /// Genererer ny ID hvis den er tom, setter CreatedAt-tidspunkt og legger til i index.
    /// </summary>
    /// <param name="entity">Entitet som skal opprettes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ID på den opprettede entiteten</returns>
    public async Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken)
    {
        // Generer ny Version 7 GUID hvis ID er tom, ellers behold eksisterende ID
        entity.Id = entity.Id == Guid.Empty ? Guid.CreateVersion7() : entity.Id;
        entity.CreatedAt = clock.UtcNow;
        
        // Serialiser og lagre entitet i Redis
        var json = Serialize(entity);
        await _db.StringSetAsync(Name + $":{entity.Id}", json);

        // Legg til ID i Redis Set index
        await UpdateIndexAsync(entity.Id.ToString());
        
        return entity.Id;
    }

    /// <summary>
    /// Oppdaterer en eksisterende entitet i Redis.
    /// Setter UpdatedAt-tidspunkt og overskriver entiteten.
    /// </summary>
    /// <param name="entity">Entitet med oppdaterte verdier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken)
    {
        entity.UpdatedAt = clock.UtcNow;
        
        // Serialiser og lagre oppdatert entitet i Redis
        var json = Serialize(entity);
        await _db.StringSetAsync(Name + $":{entity.Id}", json);
    }

    /// <summary>
    /// Sletter en entitet fra Redis basert på ID.
    /// Fjerner både entiteten og dens ID fra index.
    /// </summary>
    /// <param name="id">ID på entiteten som skal slettes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // Slett entitet fra Redis
        await _db.KeyDeleteAsync(Name + $":{id}");
        
        // Fjern ID fra index
        await RemoveFromIndexAsync(id.ToString());
    }

    /// <summary>
    /// Henter en enkelt entitet fra Redis basert på ID.
    /// </summary>
    /// <param name="id">ID på entiteten som skal hentes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Entitet hvis funnet, null ellers</returns>
    public async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var json = await _db.StringGetAsync(Name + $":{id}");
        
        if (json.IsNullOrEmpty)
            return null;
        
        return Deserialize(json);
    }

    /// <summary>
    /// Henter alle entiteter fra Redis.
    /// Itererer gjennom index og henter hver entitet parallelt for bedre ytelse.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Liste med alle entiteter som eksisterer i storage</returns>
    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken)
    {
        var ids = await GetIndexAsync();

        // Hent alle entiteter parallelt for bedre ytelse
        var tasks = ids.Select(async id =>
        {
            var json = await _db.StringGetAsync(Name + $":{id}");
            return json.IsNullOrEmpty ? null : Deserialize(json);
        });

        var entities = await Task.WhenAll(tasks);
        
        // Filtrer ut null-verdier (slettede entiteter som fortsatt er i index)
        return entities.OfType<T>().ToList();
    }
}
