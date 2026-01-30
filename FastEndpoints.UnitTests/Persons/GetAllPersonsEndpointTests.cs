using FastEndpointDemo.Endpoints.Persons.GetAll;
using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class GetAllPersonsEndpointTests
{
    [Fact]
    public void CanConstruct()
    {
        var svc = new Mock<IPersonStorageService>();
        var ep = new GetAllPersonsEndpoint(svc.Object);

        ep.Should().NotBeNull();
    }
}
