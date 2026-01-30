namespace FastEndpointDemo.Services.Exceptions;

/// <summary>
/// Exception som kastes når en forespurt ressurs ikke finnes i storage.
/// Brukes typisk ved GetAsync eller UpdateAsync når en entitet med gitt ID ikke eksisterer.
/// Resulterer i 404 Not Found HTTP-respons i endpoints.
/// </summary>
/// <param name="message">Beskrivelse av hva som ikke ble funnet</param>
public class ServiceNotFoundException(string message) : ServiceException(message);