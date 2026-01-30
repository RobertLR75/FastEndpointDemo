using FastEndpointDemo.Services.Models;

namespace FastEndpointDemo.Services;

/// <summary>
/// Hosted service som initialiserer person-storage med testdata ved applikasjonsstart.
/// Kjøres automatisk når applikasjonen starter opp og legger til 4 demo-personer.
/// Fjernes i integrasjonstester for å ha deterministisk testdata.
/// </summary>
public class PersonStorageInitializerService(IServiceProvider provider) : IHostedService
{
    /// <summary>
    /// Demo-personer som legges til i storage ved oppstart.
    /// Hver person får en ny Version 7 GUID og nåværende UTC-tidspunkt.
    /// </summary>
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

    /// <summary>
    /// Kjøres når applikasjonen starter.
    /// Oppretter et scope for å få tilgang til scoped services og legger til alle demo-personer.
    /// </summary>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Opprett scope for å få tilgang til scoped IPersonStorageService
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IPersonStorageService>();

        // Legg til alle demo-personer i storage
        foreach (var person in Persons)
        {
            await service.CreateAsync(person, cancellationToken);
        }
    }
    
    /// <summary>
    /// Kjøres når applikasjonen stopper.
    /// Ingen opprydding nødvendig da in-memory cache fjernes automatisk.
    /// </summary>
    /// <param name="cancellationToken">Token for å avbryte operasjonen</param>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

 
