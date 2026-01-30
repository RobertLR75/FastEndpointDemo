using System.Net;
using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpointDemo.Endpoints.Persons.Get;
using FastEndpoints.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.IntegrationTests.Endpoints.Persons;

public class GetPersonEndpointTests : IClassFixture<TestAppFactory>, IAsyncLifetime
{
    private readonly TestAppFactory _factory;

    public GetPersonEndpointTests(TestAppFactory factory)
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
    public async Task GetPersonsById_WhenMissing_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var res = await client.GetAsync($"/persons/{Guid.NewGuid()}", ct);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPersonsById_WhenExists_Returns200AndPerson()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var created = await client.PostJsonAsync("/persons", new CreatePersonRequest { FirstName = "Jane", LastName = "Smith" }, ct);
        created.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdBody = await created.Content.ReadJsonAsync<CreatePersonResponse>(ct);
        createdBody.Should().NotBeNull();

        var res = await client.GetAsync($"/persons/{createdBody!.Id}", ct);
        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadJsonAsync<GetPersonResponse>(ct);
        body.Should().NotBeNull();
        body!.Id.Should().Be(createdBody.Id);
        body.Name.Should().Be("Jane Smith");
        body.CreatedDate.Should().Be(createdBody.CreatedDate);
    }
}
