using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Create;

public class CreatePersonMapper : Mapper<CreatePersonRequest, CreatePersonResponse, PersonModel>
{
    public override PersonModel ToEntity(CreatePersonRequest request)
    {
        return new PersonModel
        {
            FirstName =  request.FirstName,
            LastName = request.LastName
        };
    }
    
    public override CreatePersonResponse FromEntity(PersonModel entity)
    {
        return new CreatePersonResponse
        {
            Id = entity.Id,
            CreatedDate = entity.CreatedAt,
            Name = entity.FirstName + " " + entity.LastName, 
        };
    }
}