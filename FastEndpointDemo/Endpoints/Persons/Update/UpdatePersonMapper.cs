using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// Mapper som konverterer mellom HTTP-lag og domene-lag for person-oppdatering.
/// Håndterer transformasjon av data mellom UpdatePersonRequest, PersonModel og UpdatePersonResponse.
/// </summary>
public class UpdatePersonMapper : Mapper<UpdatePersonRequest, UpdatePersonResponse, PersonModel>
{
    /// <summary>
    /// Konverterer HTTP-request til domenemodell for oppdatering.
    /// </summary>
    /// <param name="request">HTTP-request med person-ID og nye verdier</param>
    /// <returns>PersonModel klar for oppdatering i databasen/cache</returns>
    public override PersonModel ToEntity(UpdatePersonRequest request)
    {
        return new PersonModel
        {
            // Behold eksisterende ID
            Id = request.Id,
            
            // Oppdater fornavn (trimming gjøres i command handler)
            FirstName =  request.FirstName,
            
            // Oppdater etternavn (trimming gjøres i command handler)
            LastName = request.LastName
            // Merk: UpdatedAt genereres automatisk av storage service
        };
    }
    
    /// <summary>
    /// Konverterer domenemodell til HTTP-response for klienten.
    /// Merk: Mangler 'override' keyword, bør legges til.
    /// </summary>
    /// <param name="entity">PersonModel fra databasen/cache med oppdaterte verdier</param>
    /// <returns>UpdatePersonResponse med formaterte data for HTTP-respons</returns>
    public UpdatePersonResponse FromEntity(PersonModel entity)
    {
        return new UpdatePersonResponse
        {
            // Returner person-ID
            Id = entity.Id,
            
            // Returner oppdateringstidspunkt (UTC), bruk CreatedAt som fallback
            UpdatedDate = entity.UpdatedAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            
            // Kombiner fornavn og etternavn til fullt navn
            Name = entity.FirstName + " " + entity.LastName, 
        };
    }
}