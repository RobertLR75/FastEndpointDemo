using FastEndpointDemo.Endpoints.Persons.Create;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class CreatePersonResponseTests
{
    [Fact]
    public void NewResponse_HasDefaultId_And_AllowsSettingFields()
    {
        var res = new CreatePersonResponse
        {
            CreatedDate = DateTimeOffset.UtcNow,
            Name = "John Doe"
        };

        res.Id.Should().NotBeEmpty();
        res.Name.Should().Be("John Doe");
    }
}
