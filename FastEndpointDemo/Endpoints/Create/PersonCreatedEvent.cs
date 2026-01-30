using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Create;

public record PersonCreatedEvent
{
    public Guid PersonId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    public class PersonCreatedEventHandler : IEventHandler<PersonCreatedEvent>
    {
        private readonly ILogger _logger;

        public PersonCreatedEventHandler(ILogger<PersonCreatedEventHandler> logger)
        {
            _logger = logger;
        }
    
        public Task HandleAsync(PersonCreatedEvent eventModel, CancellationToken ct)
        {
            _logger.LogInformation($"Person created event received:[{eventModel.PersonId}]");
            return Task.CompletedTask;
        }
    }
}