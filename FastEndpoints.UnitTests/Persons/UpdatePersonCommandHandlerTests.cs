using FastEndpointDemo.Endpoints.Persons;
using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Exceptions;
using FastEndpointDemo.Services.Models;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class UpdatePersonCommandHandlerTests
{
    private static readonly DateTimeOffset T0 = new(2025, 01, 01, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset T1 = new(2025, 01, 01, 1, 0, 0, TimeSpan.Zero);

    private static PersonModel Person(Guid id, string firstName, string lastName, DateTimeOffset? createdAt = null, DateTimeOffset? updatedAt = null)
        => new()
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = createdAt ?? T0,
            UpdatedAt = updatedAt
        };

    private static UpdatePersonCommand Command(Guid id, string firstName, string lastName)
        => new(new PersonModel { Id = id, FirstName = firstName, LastName = lastName });

    private static UpdatePersonCommand.UpdatePersonCommandHandler Handler(Mock<IPersonStorageService> service)
        => new(service.Object);

    private static Mock<IPersonStorageService> CreateServiceMock() => new(MockBehavior.Strict);

    private static Func<Task> Execute(UpdatePersonCommand.UpdatePersonCommandHandler handler, UpdatePersonCommand cmd, CancellationToken ct)
        => () => handler.ExecuteAsync(cmd, ct);

    private static void SetupGetAll(Mock<IPersonStorageService> service, IEnumerable<PersonModel> persons)
        => service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(persons.ToList());

    private static void SetupUpdateNoop(Mock<IPersonStorageService> service)
        => service.Setup(s => s.UpdateAsync(It.IsAny<PersonModel>(), It.IsAny<CancellationToken>()))
            .Returns<PersonModel, CancellationToken>((_, _) => Task.CompletedTask);

    [Fact]
    public async Task WhenPersonNotFound_Throws_ServiceNotFoundException()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = CreateServiceMock();
        service.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PersonModel?)null);

        var handler = Handler(service);
        var cmd = Command(Guid.NewGuid(), "A", "B");

        await Execute(handler, cmd, ct).Should().ThrowAsync<ServiceNotFoundException>();
    }

    [Fact]
    public async Task WhenConflict_Throws_ServiceConflictException()
    {
        var ct = TestContext.Current.CancellationToken;

        var id = Guid.NewGuid();
        var existing = Person(id, "X", "Y");

        var service = CreateServiceMock();
        service.Setup(s => s.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        SetupGetAll(service, new[]
        {
            existing,
            Person(Guid.NewGuid(), "A", "B")
        });

        var handler = Handler(service);
        var cmd = Command(id, "A", "B");

        await Execute(handler, cmd, ct).Should().ThrowAsync<ServiceConflictException>();
    }

    [Fact]
    public async Task WhenSuccess_UpdatesAndReturnsUpdatedEntity()
    {
        var ct = TestContext.Current.CancellationToken;

        var id = Guid.NewGuid();
        var existing = Person(id, "X", "Y", createdAt: T0);
        var updated = Person(id, "A", "B", createdAt: existing.CreatedAt, updatedAt: T1);

        var service = CreateServiceMock();
        service.SetupSequence(s => s.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);
        SetupGetAll(service, new[] { existing });
        SetupUpdateNoop(service);

        var handler = Handler(service);
        var cmd = Command(id, "A", "B");

        var result = await handler.ExecuteAsync(cmd, ct);

        result.FirstName.Should().Be("A");
        result.LastName.Should().Be("B");

        service.Verify(
            s => s.UpdateAsync(
                It.Is<PersonModel>(p => p.Id == id && p.FirstName == "A" && p.LastName == "B"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenOnlyMatchingNameIsSamePerson_DoesNotThrowConflict()
    {
        var ct = TestContext.Current.CancellationToken;

        var id = Guid.NewGuid();
        var existing = Person(id, "Old", "Name");
        var updated = Person(id, "A", "B", updatedAt: T1);

        var service = CreateServiceMock();
        service.SetupSequence(s => s.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);
        SetupGetAll(service, new[] { Person(id, "A", "B") });
        SetupUpdateNoop(service);

        var handler = Handler(service);
        var cmd = Command(id, "A", "B");

        await Execute(handler, cmd, ct).Should().NotThrowAsync<ServiceConflictException>();
    }

    [Fact]
    public async Task WhenGetAfterUpdateReturnsNull_Throws_ServiceNotFoundException()
    {
        var ct = TestContext.Current.CancellationToken;

        var id = Guid.NewGuid();
        var existing = Person(id, "X", "Y");

        var service = CreateServiceMock();
        service.SetupSequence(s => s.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing)
            .ReturnsAsync((PersonModel?)null);
        SetupGetAll(service, new[] { existing });
        SetupUpdateNoop(service);

        var handler = Handler(service);
        var cmd = Command(id, "A", "B");

        await Execute(handler, cmd, ct)
            .Should().ThrowAsync<ServiceNotFoundException>()
            .WithMessage("Failed to retrieve updated person.");
    }

    [Fact]
    public async Task WhenUpdatingWithSameName_AsExisting_DoesNotThrow_AndSetsUpdatedAt()
    {
        var ct = TestContext.Current.CancellationToken;

        var id = Guid.NewGuid();
        var existing = Person(id, "A", "B", createdAt: T0, updatedAt: null);
        var updated = Person(id, "A", "B", createdAt: existing.CreatedAt, updatedAt: T1);

        PersonModel? updatedEntityPassedToUpdate = null;
        var stampedUpdatedAt = new DateTimeOffset(2030, 01, 01, 0, 0, 0, TimeSpan.Zero);

        var service = CreateServiceMock();
        service.SetupSequence(s => s.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing)
            .ReturnsAsync(updated);
        SetupGetAll(service, new[] { existing });
        service.Setup(s => s.UpdateAsync(It.IsAny<PersonModel>(), It.IsAny<CancellationToken>()))
            .Callback<PersonModel, CancellationToken>((p, _) =>
            {
                // mimic storage behavior (BaseStorageService.UpdateAsync) which stamps UpdatedAt
                p.UpdatedAt = stampedUpdatedAt;
                updatedEntityPassedToUpdate = p;
            })
            .Returns<PersonModel, CancellationToken>((_, _) => Task.CompletedTask);

        var handler = Handler(service);
        var cmd = Command(id, "A", "B");

        await Execute(handler, cmd, ct).Should().NotThrowAsync();

        updatedEntityPassedToUpdate.Should().NotBeNull();
        updatedEntityPassedToUpdate!.Id.Should().Be(id);
        updatedEntityPassedToUpdate.FirstName.Should().Be("A");
        updatedEntityPassedToUpdate.LastName.Should().Be("B");
        updatedEntityPassedToUpdate.UpdatedAt.Should().Be(stampedUpdatedAt);
    }

    [Fact]
    public async Task WhenConflict_Detected_CaseInsensitive_And_Trimmed()
    {
        var ct = TestContext.Current.CancellationToken;

        var id = Guid.NewGuid();
        var existing = Person(id, "X", "Y");
        var conflicting = Person(Guid.NewGuid(), "John", "Doe");

        var service = CreateServiceMock();
        service.Setup(s => s.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        SetupGetAll(service, new[] { existing, conflicting });

        var handler = Handler(service);
        var cmd = Command(id, "  john ", " DOE ");

        await Execute(handler, cmd, ct).Should().ThrowAsync<ServiceConflictException>();
    }
}
