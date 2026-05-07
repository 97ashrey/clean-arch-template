using Company.Service.Application.Common.Behaviours;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using FluentAssertions;
using FluentValidation;

namespace Company.Service.Application.UnitTests.Common.Behaviours;

public class ValidationPipelineBehaviourTests
{
    private record FakeRequest : ApplicationRequest<string>
    {
        public required string Name { get; init; }
        public required int Age { get; init; }
    }

    private class FakeRequestValidator : AbstractValidator<FakeRequest>
    {
        public FakeRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Age)
                .GreaterThan(0);
        }
    }

    [Fact]
    public async Task Handle_WhenValidatorIsNull_PassesThroughToNextHandler()
    {
        // Arrange
        var sut = new ValidationPipelineBehaviour<FakeRequest, ValueResult<string, ApplicationError>>(validator: null);
        var request = new FakeRequest { Name = "Test", Age = 25 };
        var expectedResult = ValueResult<string, ApplicationError>.Success("Success");

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult(expectedResult),
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WhenValidationSucceeds_PassesThroughToNextHandler()
    {
        // Arrange
        var validator = new FakeRequestValidator();
        var sut = new ValidationPipelineBehaviour<FakeRequest, ValueResult<string, ApplicationError>>(validator);
        var request = new FakeRequest { Name = "Test", Age = 25 };
        var expectedResult = ValueResult<string, ApplicationError>.Success("Success");

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => ValueTask.FromResult(expectedResult),
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
    }

    [Fact]
    public async Task Handle_WhenValidationFails_ReturnsValidationError()
    {
        // Arrange
        var validator = new FakeRequestValidator();
        var sut = new ValidationPipelineBehaviour<FakeRequest, ValueResult<string, ApplicationError>>(validator);
        var request = new FakeRequest { Name = "", Age = 0 };

        // Act
        var result = await sut.Handle(
            request,
            (_, _) => throw new InvalidOperationException("Next handler should not be called"),
            default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();

        var validationError = (ValidationError)result.Error!;
        validationError.Message.Should().Be("Validation failed.");
        validationError.Id.Should().NotBeEmpty();
        validationError.Failures.Should().HaveCount(2);
        validationError.Failures.Select(f => f.PropertyName).Should().Contain(["Name", "Age"]);
    }
}