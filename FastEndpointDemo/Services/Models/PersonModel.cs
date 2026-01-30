
using FastEndpointDemo.Services.Interfaces;
using FastEndpoints;

namespace FastEndpointDemo.Services.Models;

public record PersonModel : IEntity
{
    public Guid Id { get; set; }= Guid.NewGuid();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
