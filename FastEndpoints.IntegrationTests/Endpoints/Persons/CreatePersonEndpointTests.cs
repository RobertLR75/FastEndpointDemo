using System.Net;
using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpoints.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace FastEndpoints.IntegrationTests.Endpoints.Persons;

public class CreatePersonEndpointTests : IClassFixture<TestAppFactory>, IAsyncLifetime
{
    private readonly TestAppFactory _factory;

    public CreatePersonEndpointTests(TestAppFactory factory)
    {
        _factory = factory;
    }

    public ValueTask InitializeAsync()
    {
        _factory.ResetState();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task PostPersons_WhenValid_Returns200AndCreatedPerson()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var req = new CreatePersonRequest { FirstName = "John", LastName = "Doe" };

        var res = await client.PostJsonAsync("/persons", req, ct);

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadJsonAsync<CreatePersonResponse>(ct);
        body.Should().NotBeNull();
        body!.Id.Should().NotBe(Guid.Empty);
        body.Name.Should().Be("John Doe");
        body.CreatedDate.Should().NotBe(default);
        body.CreatedDate.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task PostPersons_WhenDuplicate_Returns400()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var req = new CreatePersonRequest { FirstName = "Dup", LastName = "Person" };

        (await client.PostJsonAsync("/persons", req, ct)).StatusCode.Should().Be(HttpStatusCode.OK);

        var res2 = await client.PostJsonAsync("/persons", req, ct);
        res2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
