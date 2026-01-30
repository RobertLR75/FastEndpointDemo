using FastEndpointDemo.Endpoints.Persons;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class UpdatePersonResponseTests
{
    [Fact]
    public void NewResponse_HasDefaultId_And_AllowsSettingFields()
    {
        var res = new UpdatePersonResponse
        {
            UpdatedDate = DateTimeOffset.UtcNow,
            Name = "A B"
        };

        res.Id.Should().NotBeEmpty();
        res.Name.Should().Be("A B");
    }
}
