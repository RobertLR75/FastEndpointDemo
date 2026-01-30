namespace FastEndpointDemo.Endpoints.Persons.Get;

/// <summary>
/// HTTP-response DTO som returneres når en person hentes.
/// Inneholder ID, opprettelsestidspunkt, oppdateringstidspunkt og fullt navn.
/// </summary>
public record GetPersonResponse 
{
    /// <summary>Unik identifikator (GUID) for personen</summary>
    public Guid Id { get; set; }= Guid.NewGuid();
    
    /// <summary>Tidspunkt når personen ble opprettet (UTC)</summary>
    public DateTimeOffset CreatedDate { get; set; }
    
    /// <summary>Tidspunkt når personen sist ble oppdatert (UTC), null hvis aldri oppdatert</summary>
    public DateTimeOffset? UpdatedDate { get; set; }
    
    /// <summary>Fullt navn (fornavn + etternavn) på personen</summary>
    public string Name { get; set; }
}