namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// HTTP-response DTO som returneres når en person er oppdatert.
/// Inneholder ID, oppdateringstidspunkt og fullt navn på den oppdaterte personen.
/// </summary>
public record UpdatePersonResponse 
{
    /// <summary>Unik identifikator (GUID) for den oppdaterte personen</summary>
    public Guid Id { get; set; }= Guid.NewGuid();
    
    /// <summary>Tidspunkt når personen sist ble oppdatert (UTC)</summary>
    public DateTimeOffset UpdatedDate { get; set; }
    
    /// <summary>Fullt navn (fornavn + etternavn) på personen</summary>
    public string Name { get; set; }
}