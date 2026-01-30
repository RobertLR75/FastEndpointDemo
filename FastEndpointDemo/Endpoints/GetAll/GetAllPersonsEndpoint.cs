using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.GetAll;

public class GetAllPersonsEndpoint(IPersonStorageService service)
    : EndpointWithoutRequest<IEnumerable<PersonModel>>
{
     public override void Configure()
     {
         Get("/persons");
         AllowAnonymous();
     }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var results = await service.GetAllAsync(ct);
        await Send.OkAsync(results, ct);
    }
}