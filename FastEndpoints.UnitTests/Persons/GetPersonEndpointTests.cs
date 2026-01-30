using FastEndpointDemo.Endpoints.Persons.Get;
using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class GetPersonEndpointTests
{
    [Fact]
    public void CanConstruct()
    {
        var svc = new Mock<IPersonStorageService>();
        var ep = new GetPersonEndpoint(svc.Object);

        ep.Should().NotBeNull();
    }
}
