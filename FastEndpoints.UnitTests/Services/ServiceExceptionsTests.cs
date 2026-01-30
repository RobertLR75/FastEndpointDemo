using FastEndpointDemo.Services.Exceptions;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Services;

public class ServiceExceptionsTests
{
    [Fact]
    public void ServiceException_IsApplicationException_AndSetsMessage()
    {
        var ex = new ServiceException("x");

        ex.Should().BeOfType<ServiceException>();
        ex.Should().BeAssignableTo<ApplicationException>();
        ex.Message.Should().Be("x");
    }

    [Fact]
    public void ServiceNotFoundException_InheritsServiceException()
    {
        var ex = new ServiceNotFoundException("not found");

        ex.Should().BeOfType<ServiceNotFoundException>();
        ex.Should().BeAssignableTo<ServiceException>();
        ex.Message.Should().Be("not found");
    }

    [Fact]
    public void ServiceConflictException_InheritsServiceException()
    {
        var ex = new ServiceConflictException("conflict");

        ex.Should().BeOfType<ServiceConflictException>();
        ex.Should().BeAssignableTo<ServiceException>();
        ex.Message.Should().Be("conflict");
    }
}
