using FastEndpointDemo.Services.Interfaces;

namespace FastEndpoints.UnitTests.Services;

internal sealed class TestClock : IClock
{
    public TestClock(DateTimeOffset now)
    {
        _now = now;
    }

    private DateTimeOffset _now;

    public DateTimeOffset UtcNow => _now;

    public void Set(DateTimeOffset now) => _now = now;

    public void Advance(TimeSpan by)
    {
        if (by < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(by), by, "Time cannot be advanced by a negative duration.");

        _now = _now.Add(by);
    }
}
