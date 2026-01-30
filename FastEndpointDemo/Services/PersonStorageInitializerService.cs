using FastEndpointDemo.Services.Models;

namespace FastEndpointDemo.Services;

public class PersonStorageInitializerService(IServiceProvider provider) : IHostedService
{

    private IEnumerable<PersonModel> Persons =>
        new List<PersonModel>()
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                CreatedAt = DateTime.UtcNow,
                FirstName = "John",
                LastName = "Doe"
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                CreatedAt = DateTime.UtcNow,
                FirstName = "Jane",
                LastName = "Smith"
            },
            new()
            {
                Id = Guid.CreateVersion7(),
                CreatedAt = DateTime.UtcNow,
                FirstName = "Alice",
                LastName = "Johnson"
            },
            new()
            {             
                Id = Guid.CreateVersion7(),
                CreatedAt = DateTime.UtcNow,
                FirstName = "Bob",
                LastName = "Brown"
            }
        };


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPersonStorageService>();

        foreach (var person in Persons)
        {
            await service.CreateAsync(person, cancellationToken);
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

 
