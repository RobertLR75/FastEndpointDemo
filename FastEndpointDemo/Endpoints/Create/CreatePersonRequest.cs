using FastEndpointDemo.Services;
using FastEndpoints;
using FluentValidation;

namespace FastEndpointDemo.Endpoints.Create;

public class CreatePersonRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public class CreatePersonValidator : Validator<CreatePersonRequest>
    {
        private readonly IPersonStorageService _service;
        public CreatePersonValidator(IPersonStorageService service)
        {
            _service = service;
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(10);
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(200);
            RuleFor(x => x).MustAsync(BeUniquePerson)
                .WithMessage("A person with the same first name and last name already exists.");
        }
    
        private async Task<bool> BeUniquePerson(CreatePersonRequest request, CancellationToken cancellationToken)
        {
            var persons = await _service.GetAllAsync(cancellationToken);
            return !persons.Any(x => x.FirstName == request.FirstName && x.LastName == request.LastName);
        }
    }
}