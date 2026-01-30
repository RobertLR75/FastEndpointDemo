using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Processors;

public class PostResponseLoggerProcessor<TRequest, TResponse> : IPostProcessor<TRequest, TResponse>
{
    public Task PostProcessAsync(IPostProcessorContext<TRequest, TResponse> ctx, CancellationToken ct)
    {
        var logger = ctx.HttpContext.Resolve<ILogger<TResponse>>();

        // if (ctx.Response is UpdatePersonResponse response)
             //logger.LogWarning($"Person updated: {response.Id}");

        return Task.CompletedTask;
    }
}