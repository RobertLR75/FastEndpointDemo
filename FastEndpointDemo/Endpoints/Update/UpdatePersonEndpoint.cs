using FastEndpointDemo.Endpoints.Processors;
using FastEndpointDemo.Services.Exceptions;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints;

public class UpdatePersonEndpoint() :
    Endpoint<UpdatePersonRequest, UpdatePersonResponse, UpdatePersonMapper>
{
    public override void Configure()
    {
        Put("/persons");
        PreProcessor<PreRequestLoggerProcessor<UpdatePersonRequest>>();
        PreProcessor<PreUpdatePersonRequestLogger>();
        PostProcessor<PostResponseLoggerProcessor<UpdatePersonRequest, UpdatePersonResponse>>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a person";
            s.Description = "Updates an existing person in the system";
            s.Response<UpdatePersonResponse>(200, "The person was updated and returned successfully.");
            s.Response(404, "The person to update was not found.");
            s.Response(409, "A person with the same first name and last name already exists.");
        });        
    }

    public override async Task HandleAsync(UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        var entity = Map.ToEntity(request);
        
        try
        {
            var command = new UpdatePersonCommand(entity);
            var result = await command.ExecuteAsync(cancellationToken);
            var response = Map.FromEntity(result);
            await Send.OkAsync(response, cancellationToken);
        }
        catch (ServiceConflictException e)
        {
            AddError("A person with the same first name and last name already exists.");
            await Send.ErrorsAsync(409, cancellation: cancellationToken);
            return;
        }
        catch (ServiceNotFoundException e)
        {
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
