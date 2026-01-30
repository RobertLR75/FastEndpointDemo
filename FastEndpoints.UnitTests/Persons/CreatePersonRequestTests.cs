using FastEndpointDemo.Endpoints.Persons.Create;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class CreatePersonRequestTests
{
    [Fact]
    public void NewRequest_CanSetProperties()
    {
        var req = new CreatePersonRequest
        {
            FirstName = "John",
            LastName = "Doe"
        };

        req.FirstName.Should().Be("John");
        req.LastName.Should().Be("Doe");
    }
}
