using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons.Create;

/// <summary>
/// Event som publiseres når en ny person opprettes i systemet.
/// Brukes for event-drevet arkitektur og løs kobling mellom komponenter.
/// </summary>
public record PersonCreatedEvent
{
    /// <summary>ID på personen som ble opprettet</summary>
    public Guid PersonId { get; set; }
    
    /// <summary>Tidspunkt når personen ble opprettet</summary>
    public DateTimeOffset CreatedAt { get; set; }
    
    /// <summary>
    /// Event handler som logger når en person opprettes.
    /// Kjøres automatisk av FastEndpoints når PersonCreatedEvent publiseres.
    /// </summary>
    public class PersonCreatedEventHandler : IEventHandler<PersonCreatedEvent>
    {
        private readonly ILogger _logger;

        public PersonCreatedEventHandler(ILogger<PersonCreatedEventHandler> logger)
        {
            _logger = logger;
        }
    
        /// <summary>
        /// Håndterer PersonCreatedEvent ved å logge informasjon om den nye personen.
        /// </summary>
        /// <param name="eventModel">Event med data om den opprettede personen</param>
        /// <param name="ct">Cancellation token</param>
        public Task HandleAsync(PersonCreatedEvent eventModel, CancellationToken ct)
        {
            _logger.LogInformation($"Person created event received:[{eventModel.PersonId}]");
            return Task.CompletedTask;
        }
    }
}