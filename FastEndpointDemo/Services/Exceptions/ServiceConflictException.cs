namespace FastEndpointDemo.Services.Exceptions;

public class ServiceConflictException(string message) : ServiceException(message);
