namespace FastEndpointDemo.Endpoints.Get;

public record GetPersonRequest
{
    public Guid Id { get; set; }
}