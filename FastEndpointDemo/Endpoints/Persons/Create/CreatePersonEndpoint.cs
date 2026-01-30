using FastEndpointDemo.Services;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons.Create;

/// <summary>
/// Endpoint for å opprette en ny person i systemet.
/// Håndterer HTTP POST-forespørsler til /persons.
/// </summary>
public class CreatePersonEndpoint(IPersonStorageService service) :
    Endpoint<CreatePersonRequest, CreatePersonResponse, CreatePersonMapper>
{
    /// <summary>
    /// Konfigurerer endpoint med HTTP-metode, rute, sikkerhet og API-dokumentasjon.
    /// </summary>
    public override void Configure()
    {
        // Definer HTTP POST-metode og rute
        Post("/persons");
        
        // Tillat anonyme forespørsler (ingen autentisering kreves)
        AllowAnonymous();
        
        // Legg til Swagger/OpenAPI-dokumentasjon
        Summary(s =>
        {
            s.Summary = "Create a person";
            s.Description = "";
            s.Response<CreatePersonResponse>(200, "The person was created and returned successfully.");
            s.Response(400, "The person could not be created.");
        });        
    }

    /// <summary>
    /// Håndterer forespørselen om å opprette en ny person.
    /// </summary>
    /// <param name="request">HTTP-request med persondata (fornavn og etternavn)</param>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    public override async Task HandleAsync(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        // 1. Konverter HTTP-request til domenemodell (PersonModel)
        var entity = Map.ToEntity(request);
       
        // 2. Lagre personen i databasen/cache og få tildelt ID
        var id = await service.CreateAsync(entity, cancellationToken);
        
        // 3. Hent den nyopprettede personen for å få alle genererte verdier (timestamps, etc.)
        var result = await service.GetAsync(id, cancellationToken);

        // 4. Valider at personen ble opprettet og hentet korrekt
        if (result == null)
        {
            // Send feilrespons hvis noe gikk galt
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        // 5. Publiser event for å informere andre deler av systemet om den nye personen
        await PublishAsync(new PersonCreatedEvent
        {
            PersonId = result.Id,
            CreatedAt = result.CreatedAt,
        }, cancellation: cancellationToken);
        
        // 6. Konverter domenemodell til HTTP-response og send 200 OK
        var response = Map.FromEntity(result);
        await Send.OkAsync(response, cancellationToken);
    }
}

