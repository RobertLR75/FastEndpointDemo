namespace FastEndpointDemo.Endpoints;

public record UpdatePersonResponse 
{
    public Guid Id { get; set; }= Guid.NewGuid();
    public DateTimeOffset UpdatedDate { get; set; }
    public string Name { get; set; }
}