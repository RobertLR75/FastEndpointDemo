using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Exceptions;
using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// Command for å oppdatere en person i systemet.
/// Følger CQRS-mønsteret ved å separere kommandoer fra spørringer.
/// </summary>
/// <param name="Person">PersonModel med ID og nye verdier som skal oppdateres</param>
public record UpdatePersonCommand(PersonModel Person) : ICommand<PersonModel>
{
    /// <summary>
    /// Command handler som utfører forretningslogikken for person-oppdatering.
    /// Validerer at personen eksisterer, sjekker for duplikater, oppdaterer og publiserer event.
    /// </summary>
    public class UpdatePersonCommandHandler(IPersonStorageService service) : ICommandHandler<UpdatePersonCommand, PersonModel>
    {
        /// <summary>
        /// Utfører oppdatering av person med validering og event-publisering.
        /// </summary>
        /// <param name="command">Command med persondata som skal oppdateres</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Oppdatert PersonModel</returns>
        /// <exception cref="ServiceNotFoundException">Kastes hvis personen ikke finnes</exception>
        /// <exception cref="ServiceConflictException">Kastes hvis nytt navn kolliderer med eksisterende person</exception>
        public async Task<PersonModel> ExecuteAsync(UpdatePersonCommand command, CancellationToken ct)
        {
            // 1. Sjekk at personen som skal oppdateres eksisterer
            var existing = await service.GetAsync(command.Person.Id, ct);
            if (existing is null)
            {
                throw new ServiceNotFoundException("The person to update was not found.");
            }

            // 2. Hent alle personer for duplikatkontroll
            var persons = await service.GetAllAsync(ct);

            // 3. Normaliser navn (trim og håndter null-verdier)
            var fn = (command.Person.FirstName ?? string.Empty).Trim();
            var ln = (command.Person.LastName ?? string.Empty).Trim();

            // 4. Sjekk om det finnes en annen person med samme navn (case-insensitive)
            var conflict = persons.Any(x =>
                string.Equals((x.FirstName ?? string.Empty).Trim(), fn, StringComparison.OrdinalIgnoreCase) &&
                string.Equals((x.LastName ?? string.Empty).Trim(), ln, StringComparison.OrdinalIgnoreCase) &&
                x.Id != existing.Id); // Ignorer sammenlikning med seg selv

            if (conflict)
            {
                throw new ServiceConflictException("A person with the same first name and last name already exists.");
            }

            // 5. Oppdater personens navn (trimmet)
            existing.FirstName = command.Person.FirstName?.Trim();
            existing.LastName = command.Person.LastName?.Trim();
            await service.UpdateAsync(existing, ct);

            // 6. Hent den oppdaterte personen for å få UpdatedAt-timestamp
            var result = await service.GetAsync(command.Person.Id, ct) 
                ?? throw new ServiceNotFoundException("Failed to retrieve updated person.");

            // 7. Publiser PersonUpdatedEvent for event-drevet arkitektur
            var ev = new PersonUpdatedEvent
            {
                PersonId = result.Id,
                UpdatedAt = result.UpdatedAt?.ToUniversalTime() ?? result.CreatedAt.ToUniversalTime(),
            };

            try
            {
                await ev.PublishAsync(cancellation: ct);
            }
            catch (InvalidOperationException)
            {
                // Unit tests / non-hosted execution: FastEndpoints service resolver may not be initialized.
                // Event publishing is best-effort og feiler stille i test-miljø.
            }

            return result;
        }
    }
}