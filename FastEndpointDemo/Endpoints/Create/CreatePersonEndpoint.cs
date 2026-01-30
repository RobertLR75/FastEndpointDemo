using FastEndpointDemo.Services;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Create;

public class CreatePersonEndpoint(IPersonStorageService service) :
    Endpoint<CreatePersonRequest, CreatePersonResponse, CreatePersonMapper>
{
    public override void Configure()
    {
        Post("/persons");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a person";
            s.Description = "";
            s.Response<CreatePersonResponse>(200, "The person was created and returned successfully.");
            s.Response(400, "The person could not be created.");
        });        
    }

    public override async Task HandleAsync(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        var entity = Map.ToEntity(request);
       
        var id = await service.CreateAsync(entity, cancellationToken);
        var result =await service.GetAsync(id, cancellationToken);

        if (result == null)
        {
            await Send.ErrorsAsync(cancellation: cancellationToken);
            return;
        }

        await PublishAsync(new PersonCreatedEvent
        {
            PersonId = result.Id,
            CreatedAt = result.CreatedAt,
        }, cancellation: cancellationToken);
        
        var response = Map.FromEntity(result);
        await Send.OkAsync(response, cancellationToken);
    }
}

#region Request and Validator

#endregion

#region Response

#endregion

#region Mapper

#endregion

#region Event

#endregion
