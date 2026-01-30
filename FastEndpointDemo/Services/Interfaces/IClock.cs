namespace FastEndpointDemo.Services.Interfaces;

/// <summary>
/// Interface for å abstrahere tidshåndtering.
/// Gjør det mulig å mocke tid i tester (TestClock) mens produksjon bruker faktisk tid (SystemClock).
/// Viktig for deterministiske tester av tidsstempler på entiteter.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Returnerer nåværende UTC-tidspunkt.
    /// I produksjon: faktisk systemtid. I tester: kontrollert tid.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
