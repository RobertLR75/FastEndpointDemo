using FastEndpointDemo.Processors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Processors;

public sealed class PreRequestLoggerProcessorTestsReq;

public class PreRequestLoggerProcessorTests
{
    private static DefaultHttpContext HttpContextWithServices(ServiceProvider provider, string path)
    {
        var http = new DefaultHttpContext { RequestServices = provider };
        http.Request.Path = path;
        return http;
    }

    private static (DefaultHttpContext Http, ServiceProvider Provider, Mock<ILogger<PreRequestLoggerProcessorTestsReq>> Logger) HttpContextWithLogger(string path)
    {
        var logger = new Mock<ILogger<PreRequestLoggerProcessorTestsReq>>(MockBehavior.Loose);

        var services = new ServiceCollection();
        services.AddSingleton(logger.Object);
        var provider = services.BuildServiceProvider();

        return (HttpContextWithServices(provider, path), provider, logger);
    }

    private static (DefaultHttpContext Http, ServiceProvider Provider) HttpContextWithoutLogger(string path)
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        return (HttpContextWithServices(provider, path), provider);
    }

    private static Mock<IPreProcessorContext<PreRequestLoggerProcessorTestsReq>> Ctx(DefaultHttpContext http, PreRequestLoggerProcessorTestsReq? req)
    {
        var ctx = new Mock<IPreProcessorContext<PreRequestLoggerProcessorTestsReq>>(MockBehavior.Strict);
        ctx.SetupGet(x => x.HttpContext).Returns(http);
        ctx.SetupGet(x => x.Request).Returns(req);
        return ctx;
    }

    private static void VerifyInfoLog(Mock<ILogger<PreRequestLoggerProcessorTestsReq>> logger, params string[] mustContain)
    {
        logger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) =>
                    mustContain.All(s => (o.ToString() ?? string.Empty).Contains(s))),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PreProcessAsync_WhenLoggerInDI_LogsInformation()
    {
        var ct = TestContext.Current.CancellationToken;

        var (http, provider, logger) = HttpContextWithLogger("/abc");
        await using var _ = provider;

        var ctx = Ctx(http, new PreRequestLoggerProcessorTestsReq());

        var proc = new PreRequestLoggerProcessor<PreRequestLoggerProcessorTestsReq>();

        await proc.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();

        VerifyInfoLog(logger, "path: /abc", typeof(PreRequestLoggerProcessorTestsReq).FullName!);
    }

    [Fact]
    public async Task PreProcessAsync_WhenRequestIsNull_StillLogsInformation()
    {
        var ct = TestContext.Current.CancellationToken;

        var (http, provider, logger) = HttpContextWithLogger("/null");
        await using var _ = provider;

        var ctx = Ctx(http, null);

        var proc = new PreRequestLoggerProcessor<PreRequestLoggerProcessorTestsReq>();

        await proc.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();

        // request type will be null in the message; path must still be present
        VerifyInfoLog(logger, "path: /null");
    }

    [Fact]
    public async Task PreProcessAsync_WhenPathEmpty_StillLogsInformation()
    {
        var ct = TestContext.Current.CancellationToken;

        var (http, provider, logger) = HttpContextWithLogger(string.Empty);
        await using var _ = provider;

        var ctx = Ctx(http, new PreRequestLoggerProcessorTestsReq());

        var proc = new PreRequestLoggerProcessor<PreRequestLoggerProcessorTestsReq>();

        await proc.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();

        VerifyInfoLog(logger, "path:");
    }

    [Fact]
    public async Task PreProcessAsync_WhenNoLoggerInDI_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;

        var (http, provider) = HttpContextWithoutLogger("/no-logger");
        await using var _ = provider;

        var ctx = Ctx(http, new PreRequestLoggerProcessorTestsReq());

        var proc = new PreRequestLoggerProcessor<PreRequestLoggerProcessorTestsReq>();

        await proc.Invoking(p => p.PreProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();
    }
}
