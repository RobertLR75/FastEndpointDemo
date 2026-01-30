using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Exceptions;
using FastEndpointDemo.Services.Models;
using FastEndpoints;

namespace FastEndpointDemo.Endpoints;

public record UpdatePersonCommand(PersonModel Person) : ICommand<PersonModel>
{
    public class UpdatePersonCommandHandler(IPersonStorageService service) : ICommandHandler<UpdatePersonCommand, PersonModel>
    {
    

        public async Task<PersonModel> ExecuteAsync(UpdatePersonCommand command, CancellationToken ct)
        {
            var persons = await service.GetAllAsync(ct);
            
            var existing = await service.GetAsync(command.Person.Id, ct);
            if (existing is null)
            {
                throw new ServiceNotFoundException("The person to update was not found.");
            }
            
            var conflict = persons.Any(x => x.FirstName == command.Person.FirstName && x.LastName == command.Person.LastName && x.Id != existing.Id);
            
            if (conflict)
            {
                throw new ServiceConflictException("A person with the same first name and last name already exists.");
            }
        
            existing.FirstName = command.Person.FirstName;
            existing.LastName = command.Person.LastName;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await service.UpdateAsync(existing, ct);
            var result =await service.GetAsync(command.Person.Id, ct);
            
            var ev = new PersonUpdatedEvent
            {
                PersonId = result.Id,
                UpdatedAt = result.UpdatedAt?.ToUniversalTime() ?? DateTimeOffset.UtcNow,
            };

            await ev.PublishAsync(cancellation: ct);
            return result!;
        }
    }  
}