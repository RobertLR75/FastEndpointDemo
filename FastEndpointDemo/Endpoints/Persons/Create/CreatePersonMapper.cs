using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons.Create;

/// <summary>
/// Mapper som konverterer mellom HTTP-lag og domene-lag for person-opprettelse.
/// Håndterer transformasjon av data mellom CreatePersonRequest, PersonModel og CreatePersonResponse.
/// </summary>
public class CreatePersonMapper : Mapper<CreatePersonRequest, CreatePersonResponse, PersonModel>
{
    /// <summary>
    /// Konverterer HTTP-request til domenemodell for lagring.
    /// </summary>
    /// <param name="request">HTTP-request med fornavn og etternavn</param>
    /// <returns>PersonModel klar for lagring i databasen/cache</returns>
    public override PersonModel ToEntity(CreatePersonRequest request)
    {
        return new PersonModel
        {
            // Trimmer whitespace fra fornavn og etternavn for konsistent datalagring
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
            // Merk: Id og CreatedAt genereres automatisk av storage service
        };
    }
    
    /// <summary>
    /// Konverterer domenemodell til HTTP-response for klienten.
    /// </summary>
    /// <param name="entity">PersonModel fra databasen/cache med alle genererte verdier</param>
    /// <returns>CreatePersonResponse med formaterte data for HTTP-respons</returns>
    public override CreatePersonResponse FromEntity(PersonModel entity)
    {
        return new CreatePersonResponse
        {
            // Returner den genererte ID-en fra storage
            Id = entity.Id,
            
            // Returner tidspunktet personen ble opprettet
            CreatedDate = entity.CreatedAt,
            
            // Kombiner fornavn og etternavn til ett fullt navn
            Name = entity.FirstName + " " + entity.LastName, 
        };
    }
}

