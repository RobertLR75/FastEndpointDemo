using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons.GetAll;

/// <summary>
/// Endpoint for å hente alle personer i systemet.
/// Håndterer HTTP GET-forespørsler til /persons.
/// Returnerer en liste med alle personer uten paginering eller filtrering.
/// </summary>
public class GetAllPersonsEndpoint(IPersonStorageService service)
    : EndpointWithoutRequest<IEnumerable<PersonModel>>
{
     /// <summary>
     /// Konfigurerer endpoint med HTTP-metode, rute og sikkerhet.
     /// </summary>
     public override void Configure()
     {
         // Definer HTTP GET-metode til /persons
         Get("/persons");
         
         // Tillat anonyme forespørsler
         AllowAnonymous();
     }

    /// <summary>
    /// Håndterer forespørselen om å hente alle personer.
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    public override async Task HandleAsync(CancellationToken ct)
    {
        // Hent alle personer fra storage
        var results = await service.GetAllAsync(ct);
        
        // Returner listen med 200 OK
        await Send.OkAsync(results, ct);
    }
}