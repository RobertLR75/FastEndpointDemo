using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// Event som publiseres når en person oppdateres i systemet.
/// Brukes for event-drevet arkitektur og løs kobling mellom komponenter.
/// </summary>
public record PersonUpdatedEvent : IEvent
{
    /// <summary>ID på personen som ble oppdatert</summary>
    public Guid PersonId { get; set; }
    
    /// <summary>Tidspunkt når personen ble oppdatert (UTC)</summary>
    public DateTimeOffset UpdatedAt { get; set; }
    
    /// <summary>
    /// Event handler som logger når en person oppdateres.
    /// Kjøres automatisk av FastEndpoints når PersonUpdatedEvent publiseres.
    /// </summary>
    public class PersonUpdatedEventHandler : IEventHandler<PersonUpdatedEvent>
    {
        private readonly ILogger _logger;

        public PersonUpdatedEventHandler(ILogger<PersonUpdatedEventHandler> logger)
        {
            _logger = logger;
        }
    
        /// <summary>
        /// Håndterer PersonUpdatedEvent ved å logge informasjon om den oppdaterte personen.
        /// </summary>
        /// <param name="eventModel">Event med data om den oppdaterte personen</param>
        /// <param name="ct">Cancellation token</param>
        public Task HandleAsync(PersonUpdatedEvent eventModel, CancellationToken ct)
        {
            _logger.LogInformation($"Person updated event received:[{eventModel.PersonId}]");
            return Task.CompletedTask;
        }
    }
}