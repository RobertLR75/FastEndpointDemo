using FastEndpointDemo.Endpoints.Persons.Create;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class PersonCreatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_LogsInformation()
    {
        var ct = TestContext.Current.CancellationToken;

        var logger = new Mock<ILogger<PersonCreatedEvent.PersonCreatedEventHandler>>(MockBehavior.Strict);
        var handler = new PersonCreatedEvent.PersonCreatedEventHandler(logger.Object);
        var ev = new PersonCreatedEvent { PersonId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow };

        logger
            .Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();

        await handler.HandleAsync(ev, ct);

        logger.Verify();
        logger.VerifyNoOtherCalls();
    }

    [Fact]
    public void Event_CanSetProperties()
    {
        var id = Guid.NewGuid();
        var ts = DateTimeOffset.UtcNow;
        var ev = new PersonCreatedEvent { PersonId = id, CreatedAt = ts };

        ev.PersonId.Should().Be(id);
        ev.CreatedAt.Should().Be(ts);
    }
}
