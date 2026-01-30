using FastEndpointDemo.Services.Models;

namespace FastEndpoints.IntegrationTests.Services;

/// <summary>
/// Enhetstester for PersonModel.
/// Tester modellens egenskaper, konstruktør og record-semantikk.
/// </summary>
public class PersonModelTests
{
    /// <summary>
    /// Hjelpemetode for å opprette PersonModel med valgfrie verdier.
    /// </summary>
    private static PersonModel Model(
        Guid? id = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null,
        string? firstName = null,
        string? lastName = null)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            CreatedAt = createdAt ?? DateTimeOffset.UtcNow,
            UpdatedAt = updatedAt,
            FirstName = firstName ?? "John",
            LastName = lastName ?? "Doe"
        };

    /// <summary>
    /// Verifiserer at en ny PersonModel har en ikke-tom GUID som standard.
    /// </summary>
    [Fact]
    public void NewPersonModel_HasNonEmptyId_ByDefault()
    {
        var model = new PersonModel();

        model.Id.Should().NotBe(Guid.Empty);
    }

    /// <summary>
    /// Verifiserer at UpdatedAt er null som standard for en ny PersonModel.
    /// </summary>
    [Fact]
    public void NewPersonModel_UpdatedAt_IsNull_ByDefault()
    {
        var model = new PersonModel();

        model.UpdatedAt.Should().BeNull();
    }

    /// <summary>
    /// Verifiserer at alle egenskaper kan settes og leses korrekt.
    /// </summary>
    [Fact]
    public void CanSetProperties()
    {
        var id = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow.AddMinutes(-1);
        var updated = DateTimeOffset.UtcNow;

        var model = new PersonModel
        {
            Id = id,
            CreatedAt = created,
            UpdatedAt = updated,
            FirstName = "John",
            LastName = "Doe"
        };

        model.Id.Should().Be(id);
        model.CreatedAt.Should().Be(created);
        model.UpdatedAt.Should().Be(updated);
        model.FirstName.Should().Be("John");
        model.LastName.Should().Be("Doe");
    }

    /// <summary>
    /// Verifiserer at to PersonModel-objekter med samme verdier er like (record-semantikk).
    /// </summary>
    [Fact]
    public void RecordSemantic_TwoModelsWithSameValues_AreEqual()
    {
        var id = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow.AddMinutes(-1);
        var updated = DateTimeOffset.UtcNow;

        var a = Model(id: id, createdAt: created, updatedAt: updated, firstName: "A", lastName: "B");
        var b = Model(id: id, createdAt: created, updatedAt: updated, firstName: "A", lastName: "B");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    /// <summary>
    /// Verifiserer at endring av en egenskap påvirker likhet (record-semantikk).
    /// </summary>
    [Fact]
    public void RecordSemantic_ChangingProperty_ChangesEquality()
    {
        var id = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow.AddMinutes(-1);

        var a = Model(id: id, createdAt: created, updatedAt: null, firstName: "A", lastName: "B");
        var b = Model(id: id, createdAt: created, updatedAt: null, firstName: "A", lastName: "C");

        a.Should().NotBe(b);
    }
}
