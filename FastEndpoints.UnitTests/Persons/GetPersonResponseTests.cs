using FastEndpointDemo.Endpoints.Persons.Get;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class GetPersonResponseTests
{
    [Fact]
    public void NewResponse_HasDefaultId_And_AllowsSettingFields()
    {
        var res = new GetPersonResponse
        {
            CreatedDate = DateTimeOffset.UtcNow,
            UpdatedDate = DateTimeOffset.UtcNow,
            Name = "A B"
        };

        res.Id.Should().NotBeEmpty();
        res.Name.Should().Be("A B");
    }
}
