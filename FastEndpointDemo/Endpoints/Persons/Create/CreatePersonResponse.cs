namespace FastEndpointDemo.Endpoints.Persons.Create;

/// <summary>
/// HTTP-response DTO som returneres når en person er opprettet.
/// Inneholder ID, opprettelsestidspunkt og fullt navn på den nye personen.
/// </summary>
public record CreatePersonResponse 
{
    /// <summary>Unik identifikator (GUID) for den opprettede personen</summary>
    public Guid Id { get; set; }= Guid.NewGuid();
    
    /// <summary>Tidspunkt når personen ble opprettet (UTC)</summary>
    public DateTimeOffset CreatedDate { get; set; }
    
    /// <summary>Fullt navn (fornavn + etternavn) på personen</summary>
    public string Name { get; set; }
}