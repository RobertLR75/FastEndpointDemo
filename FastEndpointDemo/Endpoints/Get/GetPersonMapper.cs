using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Get;

public class GetPersonMapper : ResponseMapper<GetPersonResponse, PersonModel>
{
    public override GetPersonResponse FromEntity(PersonModel request)
    {
        return new GetPersonResponse
        {
            Id = request.Id,
            CreatedDate = request.CreatedAt,
            UpdatedDate = request.UpdatedAt?.ToUniversalTime(),
            Name = request.FirstName + " " + request.LastName, 
        };
    }
}