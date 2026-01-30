using FastEndpoints;
using FluentValidation;

namespace FastEndpointDemo.Endpoints;

public class UpdatePersonRequest
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public class UpdatePersonValidator : Validator<UpdatePersonRequest>
    {
        public UpdatePersonValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id must be a valid, non-empty GUID");
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(10);
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}