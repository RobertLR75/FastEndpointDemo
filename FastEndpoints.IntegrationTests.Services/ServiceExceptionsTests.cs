using FastEndpointDemo.Services.Exceptions;

namespace FastEndpoints.IntegrationTests.Services;

/// <summary>
/// Enhetstester for service-unntaksklassene.
/// Tester at unntak er korrekt konfigurert med arv og meldinger.
/// </summary>
public class ServiceExceptionsTests
{
    /// <summary>
    /// Verifiserer at ServiceException er en ApplicationException og setter melding korrekt.
    /// </summary>
    [Fact]
    public void ServiceException_IsApplicationException_AndSetsMessage()
    {
        var ex = new ServiceException("x");

        ex.Should().BeOfType<ServiceException>();
        ex.Should().BeAssignableTo<ApplicationException>();
        ex.Message.Should().Be("x");
    }

    /// <summary>
    /// Verifiserer at ServiceNotFoundException arver fra ServiceException.
    /// </summary>
    [Fact]
    public void ServiceNotFoundException_InheritsServiceException()
    {
        var ex = new ServiceNotFoundException("not found");

        ex.Should().BeOfType<ServiceNotFoundException>();
        ex.Should().BeAssignableTo<ServiceException>();
        ex.Message.Should().Be("not found");
    }

    /// <summary>
    /// Verifiserer at ServiceConflictException arver fra ServiceException.
    /// </summary>
    [Fact]
    public void ServiceConflictException_InheritsServiceException()
    {
        var ex = new ServiceConflictException("conflict");

        ex.Should().BeOfType<ServiceConflictException>();
        ex.Should().BeAssignableTo<ServiceException>();
        ex.Message.Should().Be("conflict");
    }
}
