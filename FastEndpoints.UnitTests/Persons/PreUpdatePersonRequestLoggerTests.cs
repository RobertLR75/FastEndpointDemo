using FastEndpointDemo.Endpoints.Persons;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class PreUpdatePersonRequestLoggerTests
{
    private static (DefaultHttpContext Http, Mock<ILogger<UpdatePersonRequest>> Logger) HttpContextWithLogger()
    {
        var logger = new Mock<ILogger<UpdatePersonRequest>>(MockBehavior.Strict);

        var services = new ServiceCollection();
        services.AddSingleton(logger.Object);

        var http = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };

        return (http, logger);
    }

    private static Mock<FastEndpoints.IPreProcessorContext<UpdatePersonRequest>> CreateCtx(DefaultHttpContext http, UpdatePersonRequest? req)
    {
        var ctx = new Mock<FastEndpoints.IPreProcessorContext<UpdatePersonRequest>>(MockBehavior.Strict);
        ctx.SetupGet(x => x.HttpContext).Returns(http);
        ctx.SetupGet(x => x.Request).Returns(req);
        return ctx;
    }

    private static void SetupLoggerExpectInfo(Mock<ILogger<UpdatePersonRequest>> logger, string mustContain)
    {
        logger
            .Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains(mustContain)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
            .Verifiable();
    }

    [Fact]
    public async Task WhenRequestIsNull_DoesNotLog()
    {
        var ct = TestContext.Current.CancellationToken;

        var (http, logger) = HttpContextWithLogger();
        var ctx = CreateCtx(http, null);

        var pre = new PreUpdatePersonRequestLogger();

        await pre.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();

        logger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task WhenRequestIsPresent_LogsInformation()
    {
        var ct = TestContext.Current.CancellationToken;

        var req = new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = "A", LastName = "B" };
        var (http, logger) = HttpContextWithLogger();
        var ctx = CreateCtx(http, req);

        SetupLoggerExpectInfo(logger, $"person:{req.Id}");

        var pre = new PreUpdatePersonRequestLogger();

        await pre.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();

        logger.Verify();
        logger.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task WhenLoggerIsAvailableViaDI_DoesNotRequireFastEndpointsResolver()
    {
        var ct = TestContext.Current.CancellationToken;

        var req = new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = "A", LastName = "B" };
        var (http, logger) = HttpContextWithLogger();
        var ctx = CreateCtx(http, req);

        // If the implementation attempted to use FastEndpoints resolver despite DI having a logger,
        // unit tests would typically throw "Service resolver is null".
        SetupLoggerExpectInfo(logger, $"person:{req.Id}");

        var pre = new PreUpdatePersonRequestLogger();

        await pre.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();

        logger.Verify();
        logger.VerifyNoOtherCalls();
    }
}
