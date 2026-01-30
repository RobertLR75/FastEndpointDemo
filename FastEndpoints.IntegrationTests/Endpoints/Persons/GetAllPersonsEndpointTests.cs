using System.Net;
using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpointDemo.Services.Models;
using FastEndpoints.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.IntegrationTests.Endpoints.Persons;

public class GetAllPersonsEndpointTests : IClassFixture<TestAppFactory>, IAsyncLifetime
{
    private readonly TestAppFactory _factory;

    public GetAllPersonsEndpointTests(TestAppFactory factory)
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
    public async Task GetPersons_Returns200AndList()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        // ensure at least one created via API
        (await client.PostJsonAsync("/persons", new CreatePersonRequest { FirstName = "A", LastName = "B" }, ct)).StatusCode.Should().Be(HttpStatusCode.OK);

        var res = await client.GetAsync("/persons", ct);

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadJsonAsync<List<PersonModel>>(ct);
        body.Should().NotBeNull();
        body!.Should().NotBeEmpty();
        body.Should().Contain(p => p.FirstName == "A" && p.LastName == "B");
    }
}
