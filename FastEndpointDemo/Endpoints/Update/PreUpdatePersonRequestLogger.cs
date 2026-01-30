using FastEndpoints;

namespace FastEndpointDemo.Endpoints;

public class PreUpdatePersonRequestLogger : IPreProcessor<UpdatePersonRequest>
{
    public Task PreProcessAsync(IPreProcessorContext<UpdatePersonRequest> context, CancellationToken ct)
    {
        var logger = context.HttpContext.Resolve<ILogger<UpdatePersonRequest>>();

        if (context.Request != null)
            logger.LogInformation($"person:{context.Request.Id}");

        return Task.CompletedTask;
    }
}