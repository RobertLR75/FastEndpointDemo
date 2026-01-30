using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints;

public class UpdatePersonMapper : Mapper<UpdatePersonRequest, UpdatePersonResponse, PersonModel>
{
    public override PersonModel ToEntity(UpdatePersonRequest request)
    {
        return new PersonModel
        {
            Id = request.Id,
            FirstName =  request.FirstName,
            LastName = request.LastName
        };
    }
    
    public UpdatePersonResponse FromEntity(PersonModel entity)
    {
        return new UpdatePersonResponse
        {
            Id = entity.Id,
            UpdatedDate = entity.UpdatedAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            Name = entity.FirstName + " " + entity.LastName, 
        };
    }
}