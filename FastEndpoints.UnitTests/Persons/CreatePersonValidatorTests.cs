using FastEndpointDemo.Endpoints.Persons.Create;
using FastEndpointDemo.Services;
using FastEndpointDemo.Services.Models;
using FastEndpointDemo.Services.Storage;
using FluentAssertions;
using Moq;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class CreatePersonValidatorTests
{
    [Fact]
    public async Task DuplicateName_Fails_WithExpectedMessage()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel> { new() { FirstName = "A", LastName = "B" } });

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);

        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = "A", LastName = "B" }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists"));
    }

    [Theory]
    [InlineData("", "B", "FirstName")]
    [InlineData("A", "", "LastName")]
    public async Task Empty_Fields_Fail(string first, string last, string expectedProp)
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = first, LastName = last }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedProp);
    }

    [Theory]
    [InlineData("12345678901", "B", "FirstName")]
    public async Task MaxLength_IsEnforced(string first, string last, string expectedProp)
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = first, LastName = last }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedProp);
    }

    [Fact]
    public async Task UniqueName_Passes()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = "A", LastName = "B" }, ct);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("   ", "B", "FirstName")]
    [InlineData("A", "   ", "LastName")]
    public async Task Whitespace_Fields_Fail(string first, string last, string expectedProp)
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = first, LastName = last }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == expectedProp);
    }

    [Fact]
    public async Task Null_FirstName_Fails()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = null!, LastName = "B" }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Null_LastName_Fails()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest { FirstName = "A", LastName = null! }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task BoundaryLengths_Pass_AtMaxLimits()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest
        {
            FirstName = new string('a', 10),
            LastName = new string('b', 200)
        }, ct);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task LastName_TooLong_Fails()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>());

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);
        var result = await validator.ValidateAsync(new CreatePersonRequest
        {
            FirstName = "A",
            LastName = new string('b', 201)
        }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task DuplicateName_IsDetected_CaseInsensitive_And_Trimmed()
    {
        var ct = TestContext.Current.CancellationToken;

        var service = new Mock<IPersonStorageService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PersonModel>
            {
                new() { FirstName = "John", LastName = "Doe" }
            });

        var validator = new CreatePersonRequest.CreatePersonValidator(service.Object);

        var result = await validator.ValidateAsync(new CreatePersonRequest
        {
            FirstName = "  john ",
            LastName = " DOE  "
        }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already exists"));
    }
}
