using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons.Get;

/// <summary>
/// Mapper som konverterer PersonModel til GetPersonResponse.
/// Brukes kun for response-mapping siden dette er en GET-endpoint.
/// </summary>
public class GetPersonMapper : ResponseMapper<GetPersonResponse, PersonModel>
{
    /// <summary>
    /// Konverterer domenemodell til HTTP-response for klienten.
    /// </summary>
    /// <param name="request">PersonModel fra databasen/cache</param>
    /// <returns>GetPersonResponse med formaterte data</returns>
    public override GetPersonResponse FromEntity(PersonModel request)
    {
        return new GetPersonResponse
        {
            // Returner person-ID
            Id = request.Id,
            
            // Returner opprettelsestidspunkt
            CreatedDate = request.CreatedAt,
            
            // Returner oppdateringstidspunkt (UTC) hvis det finnes
            UpdatedDate = request.UpdatedAt?.ToUniversalTime(),
            
            // Kombiner fornavn og etternavn til fullt navn
            Name = request.FirstName + " " + request.LastName, 
        };
    }
}