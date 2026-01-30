using FastEndpointDemo.Endpoints.Persons;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class UpdatePersonEndpointTests
{
    [Fact]
    public void CanConstruct()
    {
        var ep = new UpdatePersonEndpoint();

        ep.Should().NotBeNull();
    }
}
