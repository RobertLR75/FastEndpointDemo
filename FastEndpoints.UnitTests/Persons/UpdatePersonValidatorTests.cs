using FastEndpointDemo.Endpoints.Persons;
using FluentAssertions;
using Xunit;

namespace FastEndpoints.UnitTests.Persons;

public class UpdatePersonValidatorTests
{
    [Fact]
    public async Task EmptyGuid_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();

        var result = await validator.ValidateAsync(new UpdatePersonRequest { Id = Guid.Empty, FirstName = "A", LastName = "B" }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Id");
    }

    [Fact]
    public async Task TooLongFirstName_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();
        var result = await validator.ValidateAsync(new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = new string('a', 11), LastName = "B" }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task ValidRequest_Passes()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();
        var result = await validator.ValidateAsync(new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = "A", LastName = "B" }, ct);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Whitespace_FirstName_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();

        var result = await validator.ValidateAsync(new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = "   ", LastName = "B" }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Null_FirstName_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();

        var result = await validator.ValidateAsync(new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = null!, LastName = "B" }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Fact]
    public async Task Null_LastName_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();

        var result = await validator.ValidateAsync(new UpdatePersonRequest { Id = Guid.NewGuid(), FirstName = "A", LastName = null! }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }

    [Fact]
    public async Task BoundaryLengths_Pass_AtMaxLimits()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();

        var result = await validator.ValidateAsync(new UpdatePersonRequest
        {
            Id = Guid.NewGuid(),
            FirstName = new string('a', 10),
            LastName = new string('b', 200)
        }, ct);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task LastName_TooLong_Fails()
    {
        var ct = TestContext.Current.CancellationToken;
        var validator = new UpdatePersonRequest.UpdatePersonValidator();

        var result = await validator.ValidateAsync(new UpdatePersonRequest
        {
            Id = Guid.NewGuid(),
            FirstName = "A",
            LastName = new string('b', 201)
        }, ct);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "LastName");
    }
}
