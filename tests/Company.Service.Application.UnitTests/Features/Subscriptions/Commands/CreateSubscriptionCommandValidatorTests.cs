using AwesomeAssertions;
using Company.Service.Application.Features.Subscriptions.Commands;
using Company.Service.Domain.Entities;
using FluentValidation.TestHelper;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Commands;

public class CreateSubscriptionCommandValidatorTests
{
    private readonly CreateSubscriptionCommandValidator _validator = new();

    private static CreateSubscriptionCommand CreateValidCommand() => new()
    {
        AccountId = Guid.NewGuid(),
        Name = "Premium",
        FriendlyName = "Premium Subscription",
        PurchasePrice = new CreateSubscriptionCommand.PriceCommand
        {
            Value = 29.99m,
            Currency = "USD"
        },
        BillCycle = BillCycle.Monthly,
        StartDate = DateTime.UtcNow,
        EndDate = DateTime.UtcNow.AddYears(1),
        ProductId = Guid.NewGuid()
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
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
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
    public void Validate_WithZeroPurchasePriceValue_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand
            {
                Value = 0,
                Currency = "USD"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Value)
            .WithErrorCode("GreaterThanValidator");
    }

    [Fact]
    public void Validate_WithNegativePurchasePriceValue_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand
            {
                Value = -1,
                Currency = "USD"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Value)
            .WithErrorCode("GreaterThanValidator");
    }

    [Fact]
    public void Validate_WithEmptyPurchasePriceCurrency_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand
            {
                Value = 29.99m,
                Currency = string.Empty
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Currency)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithPurchasePriceCurrencyShorterThanThree_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand
            {
                Value = 29.99m,
                Currency = "US"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Currency)
            .WithErrorCode("ExactLengthValidator");
    }

    [Fact]
    public void Validate_WithPurchasePriceCurrencyLongerThanThree_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand
            {
                Value = 29.99m,
                Currency = "USDD"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Currency)
            .WithErrorCode("ExactLengthValidator");
    }

    [Fact]
    public void Validate_WithInvalidBillCycle_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { BillCycle = (BillCycle)999 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BillCycle)
            .WithErrorCode("EnumValidator");
    }

    [Fact]
    public void Validate_WithEmptyStartDate_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { StartDate = default };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDate)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyEndDate_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { EndDate = default };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDate)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyProductId_ShouldHaveError()
    {
        // Arrange
        var command = CreateValidCommand() with { ProductId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithMultipleInvalidFields_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            AccountId = Guid.Empty,
            Name = string.Empty,
            FriendlyName = string.Empty,
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand
            {
                Value = 0,
                Currency = string.Empty
            },
            BillCycle = (BillCycle)999,
            StartDate = default,
            EndDate = default,
            ProductId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.FriendlyName);
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Value);
        result.ShouldHaveValidationErrorFor(x => x.PurchasePrice.Currency);
        result.ShouldHaveValidationErrorFor(x => x.BillCycle);
        result.ShouldHaveValidationErrorFor(x => x.StartDate);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
        result.Errors.Should().HaveCount(10);
    }
}