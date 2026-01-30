using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpointDemo.Services.Models;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class CreatePersonMapperTests
{
    private static CreatePersonMapper Mapper() => new();

    private static CreatePersonRequest Request(string firstName = "A", string lastName = "B")
        => new() { FirstName = firstName, LastName = lastName };

    private static PersonModel Entity(Guid id, DateTimeOffset createdAt, string firstName = "A", string lastName = "B")
        => new() { Id = id, CreatedAt = createdAt, FirstName = firstName, LastName = lastName };

    [Fact]
    public void ToEntity_Maps_FirstName_And_LastName()
    {
        var mapper = Mapper();
        var req = Request("A", "B");

        var entity = mapper.ToEntity(req);

        entity.FirstName.Should().Be("A");
        entity.LastName.Should().Be("B");
    }

    [Fact]
    public void ToEntity_Trims_FirstName_And_LastName()
    {
        var mapper = Mapper();
        var req = Request("  John ", " Doe  ");

        var entity = mapper.ToEntity(req);

        entity.FirstName.Should().Be("John");
        entity.LastName.Should().Be("Doe");
    }

    [Fact]
    public void FromEntity_Maps_Id_CreatedDate_And_Name()
    {
        var mapper = Mapper();
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;
        var entity = Entity(id, createdAt, "A", "B");

        var res = mapper.FromEntity(entity);

        res.Id.Should().Be(id);
        res.CreatedDate.Should().Be(createdAt);
        res.Name.Should().Be("A B");
    }
}
