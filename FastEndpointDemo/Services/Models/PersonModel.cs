
using FastEndpointDemo.Services.Interfaces;
using FastEndpoints;

namespace FastEndpointDemo.Services.Models;

/// <summary>
/// Domenemodell for en person i systemet.
/// Implementerer IEntity for å støtte generisk storage via BaseMemoryCacheStorageService.
/// Record type for immutability og verdi-basert sammenlikning.
/// </summary>
public record PersonModel : IEntity
{
    /// <summary>Unik identifikator for personen (Version 7 GUID genereres ved opprettelse)</summary>
    public Guid Id { get; set; }= Guid.NewGuid();
    
    /// <summary>Tidspunkt når personen ble opprettet (UTC)</summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>Tidspunkt når personen sist ble oppdatert (UTC), null hvis aldri oppdatert</summary>
    public DateTimeOffset? UpdatedAt { get; set; }
    
    /// <summary>Fornavn på personen (maks 10 tegn etter validering)</summary>
    public string FirstName { get; set; }
    
    /// <summary>Etternavn på personen (maks 200 tegn etter validering)</summary>
    public string LastName { get; set; }
}
