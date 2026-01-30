namespace FastEndpointDemo.Endpoints.Persons.Get;

/// <summary>
/// HTTP-request DTO for å hente en person basert på ID.
/// ID-en kommer fra URL-parameter: /persons/{id}
/// </summary>
public record GetPersonRequest
{
    /// <summary>Unik identifikator (GUID) for personen som skal hentes</summary>
    public Guid Id { get; set; }
}