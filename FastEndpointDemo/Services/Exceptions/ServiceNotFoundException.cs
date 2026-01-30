namespace FastEndpointDemo.Services.Exceptions;

public class ServiceNotFoundException(string message) : ServiceException(message);