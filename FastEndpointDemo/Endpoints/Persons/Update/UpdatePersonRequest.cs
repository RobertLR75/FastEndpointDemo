using FastEndpoints;
using FluentValidation;

namespace FastEndpointDemo.Endpoints.Persons;

/// <summary>
/// HTTP-request DTO for å oppdatere en eksisterende person.
/// Inneholder ID og nye verdier for fornavn og etternavn.
/// </summary>
public class UpdatePersonRequest
{
    /// <summary>Unik identifikator (GUID) for personen som skal oppdateres</summary>
    public Guid Id { get; set; }
    
    /// <summary>Nytt fornavn for personen</summary>
    public string FirstName { get; set; }
    
    /// <summary>Nytt etternavn for personen</summary>
    public string LastName { get; set; }
    
    /// <summary>
    /// Validator som sjekker at oppdateringsdata er gyldig.
    /// Validerer at ID er gyldig, og at navn ikke er tomme og har riktig lengde.
    /// Merk: Duplikatkontroll gjøres i UpdatePersonCommand, ikke her.
    /// </summary>
    public class UpdatePersonValidator : Validator<UpdatePersonRequest>
    {
        public UpdatePersonValidator()
        {
            // ID må være en gyldig, ikke-tom GUID
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id must be a valid, non-empty GUID");
                
            // Fornavn må være utfylt og ikke være lengre enn 10 tegn
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(10);
                
            // Etternavn må være utfylt og ikke være lengre enn 200 tegn
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}