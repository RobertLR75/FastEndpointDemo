namespace FastEndpointDemo.Services.Exceptions;

/// <summary>
/// Basis-exception for alle service-lag exceptions i applikasjonen.
/// Arver fra ApplicationException for å skille mellom forretningslogikk-feil og systemfeil.
/// Brukes som parent for spesifikke exceptions som ServiceNotFoundException og ServiceConflictException.
/// </summary>
public class ServiceException : ApplicationException
{
    /// <summary>
    /// Oppretter en ny ServiceException med en feilmelding.
    /// </summary>
    /// <param name="message">Beskrivelse av feilen som oppstod</param>
    public ServiceException(string message): base(message)
    {
    }
}