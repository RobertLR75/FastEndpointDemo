using FastEndpointDemo.Endpoints.Persons;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class UpdatePersonRequestTests
{
    [Fact]
    public void CanSetProperties()
    {
        var id = Guid.NewGuid();
        var req = new UpdatePersonRequest { Id = id, FirstName = "A", LastName = "B" };

        req.Id.Should().Be(id);
        req.FirstName.Should().Be("A");
        req.LastName.Should().Be("B");
    }
}
