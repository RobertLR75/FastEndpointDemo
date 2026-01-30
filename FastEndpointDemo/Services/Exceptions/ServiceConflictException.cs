namespace FastEndpointDemo.Services.Exceptions;

/// <summary>
/// Exception som kastes når en operasjon vil skape en konflikt med eksisterende data.
/// Brukes typisk ved CreateAsync eller UpdateAsync når en duplikat-entitet oppdages.
/// Resulterer i 409 Conflict HTTP-respons i endpoints.
/// </summary>
/// <param name="message">Beskrivelse av konflikten som oppstod</param>
public class ServiceConflictException(string message) : ServiceException(message);
