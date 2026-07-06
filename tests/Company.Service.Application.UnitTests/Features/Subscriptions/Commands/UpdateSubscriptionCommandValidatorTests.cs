using AwesomeAssertions;
using Company.Service.Application.Features.Subscriptions.Commands;
using FluentValidation.TestHelper;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Commands;

public class UpdateSubscriptionCommandValidatorTests
{
    private readonly UpdateSubscriptionCommandValidator _validator = new();

    private static UpdateSubscriptionCommand CreateValidCommand() => new()
    {
        AccountId = Guid.NewGuid(),
        Id = Guid.NewGuid(),
        FriendlyName = "Premium Subscription"
    };

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyAccountId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { AccountId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Id = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyFriendlyName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { FriendlyName = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FriendlyName)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithMultipleEmptyFields_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new UpdateSubscriptionCommand
        {
            AccountId = Guid.Empty,
            Id = Guid.Empty,
            FriendlyName = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.FriendlyName);
        result.Errors.Should().HaveCount(3);
    }
}