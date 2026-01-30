using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Storage;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons.Get;

/// <summary>
/// Endpoint for å hente en enkelt person basert på ID.
/// Håndterer HTTP GET-forespørsler til /persons/{id}.
/// </summary>
public class GetPersonEndpoint(IPersonStorageService service)
    : Endpoint<GetPersonRequest, GetPersonResponse, GetPersonMapper>
{
    /// <summary>
    /// Konfigurerer endpoint med HTTP-metode, rute, sikkerhet og API-dokumentasjon.
    /// </summary>
    public override void Configure()
    {
        // Definer HTTP GET-metode med GUID-parameter i ruten
        Get("/persons/{id:guid}");
        
        // Tillat anonyme forespørsler
        AllowAnonymous();
        
        // Legg til Swagger/OpenAPI-dokumentasjon
        Summary(s =>
        {
            s.Summary = "Gets a person by their unique identifier.";
            s.Description = "Provide a GUID to look up a specific person from the in-memory storage.";
            s.Response<GetPersonResponse>(200, "The person was found and returned successfully.");
            s.Response(404, "No person with the specified ID was found.");
        });
        
    }

    /// <summary>
    /// Håndterer forespørselen om å hente en person.
    /// </summary>
    /// <param name="request">HTTP-request med person-ID</param>
    /// <param name="ct">Cancellation token</param>
    public override async Task HandleAsync(GetPersonRequest request, CancellationToken ct)
    {
        // Hent ID fra request
        var id = request.Id;
        
        // Søk etter personen i storage
        var result = await service.GetAsync(id, ct);
        
        // Returner 404 hvis personen ikke finnes
        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        // Konverter domenemodell til HTTP-response og returner 200 OK
        var response = Map.FromEntity(result);
        await Send.OkAsync(response, ct);
    }
}

#region Mapper and Response

#endregion
