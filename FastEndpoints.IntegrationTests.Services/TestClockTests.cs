namespace FastEndpoints.IntegrationTests.Services;

/// <summary>
/// Enhetstester for TestClock.
/// Tester at test-klokken kan settes og avanseres for tidskontroll i tester.
/// </summary>
public class TestClockTests
{
    /// <summary>
    /// Verifiserer at UtcNow returnerer verdien gitt i konstruktøren.
    /// </summary>
    [Fact]
    public void UtcNow_ReturnsConstructorValue()
    {
        var now = DateTimeOffset.UtcNow;
        var clock = new TestClock(now);

        clock.UtcNow.Should().Be(now);
    }

    /// <summary>
    /// Verifiserer at Set endrer UtcNow til en ny verdi.
    /// </summary>
    [Fact]
    public void Set_ChangesUtcNow_ToNewValue()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var next = DateTimeOffset.UtcNow.AddHours(1);

        clock.Set(next);

        clock.UtcNow.Should().Be(next);
    }

    /// <summary>
    /// Verifiserer at Advance med positiv varighet flytter tiden fremover.
    /// </summary>
    [Fact]
    public void Advance_WithPositiveDuration_MovesTimeForward()
    {
        var start = DateTimeOffset.UtcNow;
        var clock = new TestClock(start);

        clock.Advance(TimeSpan.FromMinutes(5));

        clock.UtcNow.Should().Be(start.AddMinutes(5));
    }

    /// <summary>
    /// Verifiserer at Advance med null-varighet ikke endrer tiden.
    /// </summary>
    [Fact]
    public void Advance_WithZeroDuration_DoesNotChangeTime()
    {
        var start = DateTimeOffset.UtcNow;
        var clock = new TestClock(start);

        clock.Advance(TimeSpan.Zero);

        clock.UtcNow.Should().Be(start);
    }

    /// <summary>
    /// Verifiserer at Advance kaster unntak ved negativ varighet.
    /// </summary>
    [Fact]
    public void Advance_WithNegativeDuration_Throws()
    {
        var clock = new TestClock(DateTimeOffset.UtcNow);

        clock.Invoking(c => c.Advance(TimeSpan.FromSeconds(-1)))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*negative duration*");
    }
}
