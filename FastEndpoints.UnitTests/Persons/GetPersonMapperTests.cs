using FastEndpointDemo.Endpoints.Persons.Get;
using FastEndpointDemo.Services.Models;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class GetPersonMapperTests
{
    private static GetPersonMapper Mapper() => new();

    private static PersonModel Entity(Guid id, DateTimeOffset createdAt, DateTimeOffset? updatedAt, string firstName = "A", string lastName = "B")
        => new()
        {
            Id = id,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            FirstName = firstName,
            LastName = lastName
        };

    [Fact]
    public void FromEntity_Maps_Id_Created_Updated_And_Name()
    {
        var mapper = Mapper();
        var id = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow.AddDays(-1);
        var updated = DateTimeOffset.UtcNow;
        var entity = Entity(id, created, updated, "A", "B");

        var res = mapper.FromEntity(entity);

        res.Id.Should().Be(id);
        res.CreatedDate.Should().Be(created);
        res.UpdatedDate.Should().Be(updated.ToUniversalTime());
        res.Name.Should().Be("A B");
    }
}
