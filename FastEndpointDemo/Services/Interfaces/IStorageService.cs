namespace FastEndpointDemo.Services.Interfaces;

/// <summary>
/// Generisk interface for storage service som håndterer CRUD-operasjoner for entiteter.
/// Definerer standard operasjoner som alle storage-implementasjoner må støtte.
/// </summary>
/// <typeparam name="T">Type entitet som skal lagres (må implementere IEntity)</typeparam>
public interface IStorageService<T> where T : class, IEntity
{
    /// <summary>Navn/prefix for storage (brukes til cache-nøkler)</summary>
    public static string Name { get; }
    
    /// <summary>Oppretter en ny entitet i storage</summary>
    /// <param name="entity">Entitet som skal opprettes</param>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    /// <returns>ID på den opprettede entiteten</returns>
    public Task<Guid> CreateAsync(T entity, CancellationToken cancellationToken = default);
    
    /// <summary>Oppdaterer en eksisterende entitet i storage</summary>
    /// <param name="entity">Entitet med oppdaterte verdier</param>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    public Task UpdateAsync(T entity, CancellationToken cancellationToken=default);
    
    /// <summary>Sletter en entitet fra storage basert på ID</summary>
    /// <param name="id">ID på entiteten som skal slettes</param>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken=default);
    
    /// <summary>Henter en enkelt entitet fra storage basert på ID</summary>
    /// <param name="id">ID på entiteten som skal hentes</param>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    /// <returns>Entitet hvis funnet, null ellers</returns>
    public Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>Henter alle entiteter fra storage</summary>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    /// <returns>Liste med alle entiteter</returns>
    public Task<List<T>> GetAllAsync(CancellationToken cancellationToken=default);
}
