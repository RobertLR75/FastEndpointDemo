using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Services;

public class TestClockTests
{
    [Fact]
    public void UtcNow_ReturnsConstructorValue()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new TestClock(now);

        clock.UtcNow.Should().Be(now);
    }

    [Fact]
    public void Set_ChangesUtcNow_ToNewValue()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var next = DateTimeOffset.UtcNow.AddHours(1);

        clock.Set(next);

        clock.UtcNow.Should().Be(next);
    }

    [Fact]
    public void Advance_WithPositiveDuration_MovesTimeForward()
    {
        var start = DateTimeOffset.UtcNow;
        var clock = new TestClock(start);

        clock.Advance(TimeSpan.FromMinutes(5));

        clock.UtcNow.Should().Be(start.AddMinutes(5));
    }

    [Fact]
    public void Advance_WithZeroDuration_DoesNotChangeTime()
    {
        var start = DateTimeOffset.UtcNow;
        var clock = new TestClock(start);

        clock.Advance(TimeSpan.Zero);

        clock.UtcNow.Should().Be(start);
    }

    [Fact]
    public void Advance_WithNegativeDuration_Throws()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);

        clock.Invoking(c => c.Advance(TimeSpan.FromSeconds(-1)))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*negative duration*");
    }
}
