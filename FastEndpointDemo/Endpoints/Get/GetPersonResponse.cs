namespace FastEndpointDemo.Endpoints.Get;

public record GetPersonResponse 
{
    public Guid Id { get; set; }= Guid.NewGuid();
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public string Name { get; set; }
}