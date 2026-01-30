namespace FastEndpointDemo.Services.Exceptions;

public class ServiceException : ApplicationException
{
    public ServiceException(string message): base(message)
    {
    }
}