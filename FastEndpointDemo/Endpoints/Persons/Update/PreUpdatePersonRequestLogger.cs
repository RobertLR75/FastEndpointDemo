using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// Pre-processor som logger person-ID før oppdatering utføres.
/// Kjøres automatisk før UpdatePersonEndpoint's HandleAsync-metode.
/// Implementerer robust logger-resolving som fungerer både i runtime og i tester.
/// </summary>
public class PreUpdatePersonRequestLogger : IPreProcessor<UpdatePersonRequest>
{
    /// <summary>
    /// Logger person-ID som skal oppdateres.
    /// Prøver først ASP.NET Core DI, deretter FastEndpoints resolver som fallback.
    /// </summary>
    /// <param name="context">Pre-processor context med tilgang til request og HTTP-context</param>
    /// <param name="ct">Cancellation token</param>
    public Task PreProcessAsync(IPreProcessorContext<UpdatePersonRequest> context, CancellationToken ct)
    {
        // Prøv først å hente logger fra ASP.NET Core DI container (fungerer i både runtime og tester)
        var logger = context.HttpContext.RequestServices.GetService(typeof(ILogger<UpdatePersonRequest>)) as ILogger<UpdatePersonRequest>;

        // Fallback til FastEndpoints resolver når app kjører i full FastEndpoints-pipeline
        if (logger is null)
        {
            try
            {
                logger = context.HttpContext.Resolve<ILogger<UpdatePersonRequest>>();
            }
            catch (InvalidOperationException)
            {
                // Unit tests / non-hosted execution: FastEndpoints service resolver may not be initialized.
                // Logging er best-effort og feiler stille i test-miljø.
            }
        }

        // Logger person-ID hvis logger er tilgjengelig og request er gyldig
        if (logger is not null && context.Request is not null)
        {
            logger.LogInformation($"person:{context.Request.Id}");
        }

        return Task.CompletedTask;
    }
}