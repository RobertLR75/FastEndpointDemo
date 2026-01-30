using FastEndpointDemo.Endpoints.Persons.Get;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class GetPersonRequestTests
{
    [Fact]
    public void CanSetId()
    {
        var id = Guid.NewGuid();
        var req = new GetPersonRequest { Id = id };
        req.Id.Should().Be(id);
    }
}
