using FastEndpoints;

namespace FastEndpointDemo.Endpoints;

public record PersonUpdatedEvent : IEvent
{
    public Guid PersonId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public class PersonUpdatedEventHandler : IEventHandler<PersonUpdatedEvent>
    {
        private readonly ILogger _logger;

        public PersonUpdatedEventHandler(ILogger<PersonUpdatedEventHandler> logger)
        {
            _logger = logger;
        }
    
        public Task HandleAsync(PersonUpdatedEvent eventModel, CancellationToken ct)
        {
            _logger.LogInformation($"Person updated event received:[{eventModel.PersonId}]");
            return Task.CompletedTask;
        }
    }
}