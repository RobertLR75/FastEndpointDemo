using FastEndpointDemo.Services.Interfaces;

namespace FastEndpointDemo.Services;

/// <summary>
/// Produksjonsimplementasjon av IClock som returnerer faktisk systemtid.
/// Brukes i runtime-miljø (ikke i tester hvor TestClock brukes for deterministisk tid).
/// Sealed for å unngå arv og sikre konsistent oppførsel.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <summary>
    /// Returnerer nåværende UTC-tidspunkt fra systemklokken.
    /// </summary>
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
