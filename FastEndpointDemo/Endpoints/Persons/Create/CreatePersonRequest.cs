using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Storage;
using FastEndpoints;
using FluentValidation;

namespace FastEndpointDemo.Endpoints.Persons.Create;

/// <summary>
/// HTTP-request DTO for å opprette en ny person.
/// Inneholder fornavn og etternavn som kreves for å opprette en person.
/// </summary>
public class CreatePersonRequest
{
    /// <summary>Fornavn på personen som skal opprettes</summary>
    public required string FirstName { get; set; }
    
    /// <summary>Etternavn på personen som skal opprettes</summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Validator som sjekker at persondata er gyldig før opprettelse.
    /// Validerer at navn ikke er tomme, har riktig lengde, og at personen ikke allerede eksisterer.
    /// </summary>
    public class CreatePersonValidator : Validator<CreatePersonRequest>
    {
        private readonly IPersonStorageService _service;
        
        public CreatePersonValidator(IPersonStorageService service)
        {
            _service = service;
            
            // Fornavn må være utfylt og ikke være lengre enn 10 tegn
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(10);
                
            // Etternavn må være utfylt og ikke være lengre enn 200 tegn
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(200);
                
            // Sjekk at kombinasjonen fornavn + etternavn er unik
            RuleFor(x => x).MustAsync(BeUniquePerson)
                .WithMessage("A person with the same first name and last name already exists.");
        }

        /// <summary>
        /// Asynkron validering som sjekker om en person med samme navn allerede eksisterer.
        /// Sammenligner fornavn og etternavn case-insensitive og trimmet.
        /// </summary>
        /// <param name="request">Request med persondata som skal valideres</param>
        /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
        /// <returns>True hvis personen er unik, false hvis duplikat</returns>
        private async Task<bool> BeUniquePerson(CreatePersonRequest request, CancellationToken cancellationToken)
        {
            // Hvis grunnleggende validering feiler (null/empty), hopp over unikhetskontroll
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
                return true;

            // Hent alle eksisterende personer fra storage
            var persons = await _service.GetAllAsync(cancellationToken);

            var fn = request.FirstName.Trim();
            var ln = request.LastName.Trim();

            // Sjekk om det finnes en person med samme navn (case-insensitive og trimmet)
            return !persons.Any(x =>
                !string.IsNullOrEmpty(x.FirstName) &&
                !string.IsNullOrEmpty(x.LastName) &&
                string.Equals(x.FirstName.Trim(), fn, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.LastName.Trim(), ln, StringComparison.OrdinalIgnoreCase));
        }
    }
}

