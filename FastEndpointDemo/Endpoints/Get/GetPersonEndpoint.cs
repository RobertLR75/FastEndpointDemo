using FastEndpointDemo.Services;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Get;

public class GetPersonEndpoint(IPersonStorageService service)
    : Endpoint<GetPersonRequest, GetPersonResponse, GetPersonMapper>
{
    public override void Configure()
    {
        Get("/persons/{id:guid}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Gets a person by their unique identifier.";
            s.Description = "Provide a GUID to look up a specific person from the in-memory storage.";
            s.Response<GetPersonResponse>(200, "The person was found and returned successfully.");
            s.Response(404, "No person with the specified ID was found.");
        });
        
    }

    public override async Task HandleAsync(GetPersonRequest request, CancellationToken ct)
    {
        var id = request.Id;
        var result = await service.GetAsync(id, ct);
        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var response = Map.FromEntity(result);
        await Send.OkAsync(response, ct);
    }
}

#region Mapper and Response

#endregion
