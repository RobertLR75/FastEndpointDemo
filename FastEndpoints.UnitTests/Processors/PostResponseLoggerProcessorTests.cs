using FastEndpointDemo.Processors;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Processors;

public sealed class PostResponseLoggerProcessorTestsReq;
public sealed class PostResponseLoggerProcessorTestsRes;

public class PostResponseLoggerProcessorTests
{
    private static DefaultHttpContext HttpContextWithLogger(Mock<ILogger<PostResponseLoggerProcessorTestsRes>> logger)
    {
        var services = new ServiceCollection();
        services.AddSingleton(logger.Object);

        return new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
    }

    private static Mock<IPostProcessorContext<PostResponseLoggerProcessorTestsReq, PostResponseLoggerProcessorTestsRes>> Ctx(DefaultHttpContext http)
    {
        var ctx = new Mock<IPostProcessorContext<PostResponseLoggerProcessorTestsReq, PostResponseLoggerProcessorTestsRes>>(MockBehavior.Strict);
        ctx.SetupGet(x => x.HttpContext).Returns(http);
        ctx.SetupGet(x => x.Request).Returns(new PostResponseLoggerProcessorTestsReq());
        ctx.SetupGet(x => x.Response).Returns(new PostResponseLoggerProcessorTestsRes());
        return ctx;
    }

    [Fact]
    public async Task PostProcessAsync_WhenLoggerInDI_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;

        var logger = new Mock<ILogger<PostResponseLoggerProcessorTestsRes>>(MockBehavior.Loose);
        var http = HttpContextWithLogger(logger);
        var ctx = Ctx(http);

        var proc = new PostResponseLoggerProcessor<PostResponseLoggerProcessorTestsReq, PostResponseLoggerProcessorTestsRes>();

        await proc.Invoking(p => p.PostProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task PostProcessAsync_WhenNoLoggerInDI_DoesNotThrow()
    {
        var ct = TestContext.Current.CancellationToken;

        var http = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().BuildServiceProvider()
        };
        var ctx = Ctx(http);

        var proc = new PostResponseLoggerProcessor<PostResponseLoggerProcessorTestsReq, PostResponseLoggerProcessorTestsRes>();

        await proc.Invoking(p => p.PostProcessAsync(ctx.Object, ct))
            .Should().NotThrowAsync();
    }
}
