using System.Net;
using FastEndpointDemo.Endpoints.Persons;
using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpoints.IntegrationTests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.IntegrationTests.Endpoints.Persons;

public class UpdatePersonEndpointTests : IClassFixture<TestAppFactory>, IAsyncLifetime
{
    private readonly TestAppFactory _factory;

    public UpdatePersonEndpointTests(TestAppFactory factory)
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
    public async Task PutPersons_WhenNotFound_Returns404()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var req = new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = "X", LastName = "Y" };

        var res = await client.PutJsonAsync("/persons", req, ct);

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutPersons_WhenValid_Returns200AndUpdatedPerson()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        var created = await client.PostJsonAsync("/persons",
            new CreatePersonRequest { FirstName = "Tom", LastName = "Thumb" }, ct);
        created.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdBody = await created.Content.ReadJsonAsync<CreatePersonResponse>(ct);
        createdBody.Should().NotBeNull();

        var req = new UpdatePersonRequest { Id = createdBody!.Id, FirstName = "Tommy", LastName = "Thumb" };

        var res = await client.PutJsonAsync("/persons", req, ct);

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadJsonAsync<UpdatePersonResponse>(ct);
        body.Should().NotBeNull();
        body!.Id.Should().Be(createdBody.Id);
        body.Name.Should().Be("Tommy Thumb");

        // Avoid time-based flakiness. Just assert it's set to a sensible value.
        body.UpdatedDate.Should().NotBe(default);
        body.UpdatedDate.Should().BeOnOrBefore(DateTimeOffset.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task PutPersons_WhenConflict_Returns409()
    {
        var ct = TestContext.Current.CancellationToken;
        using var client = _factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });

        // Create two distinct persons
        var p1 = await client.PostJsonAsync("/persons", new CreatePersonRequest { FirstName = "Same", LastName = "Name" }, ct);
        var p2 = await client.PostJsonAsync("/persons", new CreatePersonRequest { FirstName = "Other", LastName = "Person" }, ct);
        p1.StatusCode.Should().Be(HttpStatusCode.OK);
        p2.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await p2.Content.ReadJsonAsync<CreatePersonResponse>(ct);
        second.Should().NotBeNull();

        // update second to match first -> conflict
        var req = new UpdatePersonRequest { Id = second!.Id, FirstName = "Same", LastName = "Name" };
        var res = await client.PutJsonAsync("/persons", req, ct);

        res.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
