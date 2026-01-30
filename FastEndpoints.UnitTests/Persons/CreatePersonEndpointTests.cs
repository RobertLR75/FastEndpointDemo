using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class CreatePersonEndpointTests
{
    [Fact]
    public void CanConstruct()
    {
        var svc = new Mock<IPersonStorageService>();
        var ep = new CreatePersonEndpoint(svc.Object);

        ep.Should().NotBeNull();
    }
}
