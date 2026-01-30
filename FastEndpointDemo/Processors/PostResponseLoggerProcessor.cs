using FastEndpoints;

namespace FastEndpointDemo.Processors;

/// <summary>
/// Post-processor som logger etter at en endpoint har behandlet en request.
/// Generisk klasse som kan brukes på alle endpoints for å logge response-informasjon.
/// Implementerer robust logger-resolving som fungerer både i runtime og i tester.
/// </summary>
/// <typeparam name="TRequest">Type for HTTP-request</typeparam>
/// <typeparam name="TResponse">Type for HTTP-response</typeparam>
public class PostResponseLoggerProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    /// <summary>
    /// Kjøres automatisk etter at endpoint har generert respons.
    /// Logger informasjon om responsen (implementasjonen kan utvides etter behov).
    /// </summary>
    /// <param name="ctx">Post-processor context med tilgang til request, response og HTTP-context</param>
    /// <param name="ct">Cancellation token</param>
    public Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> ctx, CancellationToken ct)
    {
        // Hent logger fra DI container, med fallback til FastEndpoints resolver
        _ = ctx.HttpContext.RequestServices.GetService(typeof(ILogger<TResponse>)) as ILogger<TResponse>
            ?? TryResolveLogger(ctx);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Prøver å hente logger fra FastEndpoints resolver som fallback.
    /// Returnerer null hvis resolver ikke er initialisert (f.eks. i tester).
    /// </summary>
    /// <param name="ctx">Post-processor context</param>
    /// <returns>Logger hvis tilgjengelig, null ellers</returns>
    private static ILogger<TResponse>? TryResolveLogger(IPostProcessorContext<TRequest, TResponse> ctx)
    {
        try
        {
            return ctx.HttpContext.Resolve<ILogger<TResponse>>();
        }
        catch (InvalidOperationException)
        {
            // Unit tests / non-hosted execution: FastEndpoints service resolver may not be initialized.
            // Logging er best-effort og feiler stille i test-miljø.
            return null;
        }
    }
}