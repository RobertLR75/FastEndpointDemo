using FastEndpointDemo.Endpoints.Persons;
using FastEndpointDemo.Services.Models;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class UpdatePersonMapperTests
{
    private static UpdatePersonMapper Mapper() => new();

    private static UpdatePersonRequest Request(Guid id, string firstName = "A", string lastName = "B")
        => new() { Id = id, FirstName = firstName, LastName = lastName };

    private static PersonModel Entity(Guid id, string firstName = "A", string lastName = "B", DateTimeOffset? updatedAt = null)
        => new() { Id = id, FirstName = firstName, LastName = lastName, UpdatedAt = updatedAt };

    [Fact]
    public void ToEntity_Maps_Id_And_Names()
    {
        var mapper = Mapper();
        var id = Guid.NewGuid();
        var req = Request(id);

        var entity = mapper.ToEntity(req);

        entity.Id.Should().Be(id);
        entity.FirstName.Should().Be("A");
        entity.LastName.Should().Be("B");
    }

    [Fact]
    public void FromEntity_Maps_Id_UpdatedDate_And_Name()
    {
        var mapper = Mapper();
        var id = Guid.NewGuid();
        var updated = DateTimeOffset.UtcNow;
        var entity = Entity(id, updatedAt: updated);

        var res = mapper.FromEntity(entity);

        res.Id.Should().Be(id);
        res.Name.Should().Be("A B");
        res.UpdatedDate.Should().Be(updated.ToUniversalTime());
    }
}
