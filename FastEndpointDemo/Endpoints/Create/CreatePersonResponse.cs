namespace FastEndpointDemo.Endpoints.Create;

public record CreatePersonResponse 
{
    public Guid Id { get; set; }= Guid.NewGuid();
    public DateTimeOffset CreatedDate { get; set; }
    public string Name { get; set; }
}