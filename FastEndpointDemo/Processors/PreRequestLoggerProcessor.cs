using FastEndpoints;

namespace FastEndpointDemo.Processors;

/// <summary>
/// Pre-processor som logger før en endpoint begynner å behandle en request.
/// Generisk klasse som kan brukes på alle endpoints for å logge request-informasjon.
/// Implementerer robust logger-resolving som fungerer både i runtime og i tester.
/// </summary>
/// <typeparam name="TRequest">Type for HTTP-request</typeparam>
public class PreRequestLoggerProcessor<TRequest> : IPreProcessor<TRequest>
{
    /// <summary>
    /// Kjøres automatisk før endpoint begynner å behandle request.
    /// Logger request-type og HTTP-sti.
    /// </summary>
    /// <param name="ctx">Pre-processor context med tilgang til request og HTTP-context</param>
    /// <param name="ct">Cancellation token</param>
    public Task PreProcessAsync(IPreProcessorContext<TRequest> ctx, CancellationToken ct)
    {
        // Prøv først å hente logger fra ASP.NET Core DI container
        var logger = ctx.HttpContext.RequestServices.GetService(typeof(ILogger<TRequest>)) as ILogger<TRequest>;

        // Fallback til FastEndpoints resolver hvis DI returnerer null
        if (logger is null)
        {
            try
            {
                logger = ctx.HttpContext.Resolve<ILogger<TRequest>>();
            }
            catch (InvalidOperationException)
            {
                // Unit tests / non-hosted execution: FastEndpoints service resolver may not be initialized.
                // Logging er best-effort og feiler stille i test-miljø.
            }
        }

        // Logger request-informasjon hvis logger er tilgjengelig
        if (logger is not null)
        {
            logger.LogInformation($"request:{ctx.Request?.GetType().FullName} path: {ctx.HttpContext.Request.Path}");
        }

        return Task.CompletedTask;
    }
}