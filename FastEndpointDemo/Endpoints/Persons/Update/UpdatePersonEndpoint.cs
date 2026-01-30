using FastEndpointDemo.Processors;
using FastEndpointDemo.Services.Exceptions;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// Endpoint for å oppdatere en eksisterende person.
/// Håndterer HTTP PUT-forespørsler til /persons.
/// Bruker command pattern og pre/post-processors for logging.
/// </summary>
public class UpdatePersonEndpoint() :
    Endpoint<UpdatePersonRequest, UpdatePersonResponse, UpdatePersonMapper>
{
    /// <summary>
    /// Konfigurerer endpoint med HTTP-metode, rute, processors, sikkerhet og API-dokumentasjon.
    /// </summary>
    public override void Configure()
    {
        // Definer HTTP PUT-metode
        Put("/persons");
        
        // Legg til pre-processors for logging
        PreProcessor<PreRequestLoggerProcessor<UpdatePersonRequest>>();
        PreProcessor<PreUpdatePersonRequestLogger>();
        
        // Legg til post-processor for logging av respons
        PostProcessor<PostResponseLoggerProcessor<UpdatePersonRequest, UpdatePersonResponse>>();
        
        // Tillat anonyme forespørsler
        AllowAnonymous();
        
        // Legg til Swagger/OpenAPI-dokumentasjon
        Summary(s =>
        {
            s.Summary = "Update a person";
            s.Description = "Updates an existing person in the system";
            s.Response<UpdatePersonResponse>(200, "The person was updated and returned successfully.");
            s.Response(404, "The person to update was not found.");
            s.Response(409, "A person with the same first name and last name already exists.");
        });        
    }

    /// <summary>
    /// Håndterer forespørselen om å oppdatere en person.
    /// Bruker UpdatePersonCommand for forretningslogikk og validering.
    /// </summary>
    /// <param name="request">HTTP-request med person-ID og nye verdier</param>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    public override async Task HandleAsync(UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        // Konverter HTTP-request til domenemodell
        var entity = Map.ToEntity(request);
        
        try
        {
            // Utfør oppdatering via command handler
            var command = new UpdatePersonCommand(entity);
            var result = await command.ExecuteAsync(cancellationToken);
            
            // Konverter resultat til HTTP-response og returner 200 OK
            var response = Map.FromEntity(result);
            await Send.OkAsync(response, cancellationToken);
        }
        catch (ServiceConflictException e)
        {
            // Returner 409 Conflict hvis en person med samme navn allerede eksisterer
            AddError("A person with the same first name and last name already exists.");
            await Send.ErrorsAsync(409, cancellation: cancellationToken);
            return;
        }
        catch (ServiceNotFoundException e)
        {
            // Returner 404 Not Found hvis personen ikke finnes
            AddError("The person to update was not found.");
            await Send.ErrorsAsync(404, cancellation: cancellationToken);
            return;
        }
    }
}

#region Response

#endregion

#region Request

#endregion

#region Mapper

#endregion

#region Command

#endregion

#region Event

#endregion

#region PreProcessor

#endregion
