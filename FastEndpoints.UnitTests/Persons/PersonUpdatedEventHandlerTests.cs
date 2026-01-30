using FastEndpointDemo.Endpoints.Persons;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class PersonUpdatedEventHandlerTests
{
    [Fact]
    public async Task HandleAsync_LogsInformation()
    {
        var ct = TestContext.Current.CancellationToken;

        var logger = new Mock<ILogger<PersonUpdatedEvent.PersonUpdatedEventHandler>>(MockBehavior.Strict);
        var handler = new PersonUpdatedEvent.PersonUpdatedEventHandler(logger.Object);
        var ev = new PersonUpdatedEvent { PersonId = Guid.NewGuid(), UpdatedAt = DateTimeOffset.UtcNow };

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
        var ev = new PersonUpdatedEvent { PersonId = id, UpdatedAt = ts };

        ev.PersonId.Should().Be(id);
        ev.UpdatedAt.Should().Be(ts);
    }
}
