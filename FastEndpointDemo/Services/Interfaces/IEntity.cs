namespace FastEndpointDemo.Services.Interfaces;

/// <summary>
/// Basis-interface for alle entiteter som skal lagres i storage.
/// Definerer felles properties for ID og tidsstempler som alle entiteter må ha.
/// Brukes som constraint i IStorageService og BaseMemoryCacheStorageService.
/// </summary>
public interface IEntity
{
    /// <summary>Unik identifikator for entiteten (GUID)</summary>
    public Guid Id { get; set; }
    
    /// <summary>Tidspunkt når entiteten ble opprettet (UTC)</summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>Tidspunkt når entiteten sist ble oppdatert (UTC), null hvis aldri oppdatert</summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}