using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.UnitTests.ValueObjects;

public class AccountDetailsTests
{
    private readonly Guid _invoiceAddressId = Guid.NewGuid();

    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedAccountDetails()
    {
        // Arrange
        var name = "Test Account";
        var email = "test@example.com";
        var tier = AccountTier.Business;

        // Act
        var result = AccountDetails.CreateNew(name, email, tier, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be(name);
        result.Value.Email.Should().Be(email);
        result.Value.Tier.Should().Be(tier);
        result.Value.InvoiceAddressId.Should().Be(_invoiceAddressId);
    }

    [Fact]
    public void CreateNew_WithEmptyName_ReturnsValidationError()
    {
        // Act
        var result = AccountDetails.CreateNew("", "test@example.com", AccountTier.Individual, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "name");
    }

    [Fact]
    public void CreateNew_WithEmptyEmail_ReturnsValidationError()
    {
        // Act
        var result = AccountDetails.CreateNew("Test Account", "", AccountTier.Individual, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "email");
    }

    [Fact]
    public void CreateNew_WithEmptyInvoiceAddressId_ReturnsValidationError()
    {
        // Act
        var result = AccountDetails.CreateNew("Test Account", "test@example.com", AccountTier.Individual, Guid.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "invoiceAdressId");
    }

    [Theory]
    [InlineData(AccountTier.Individual)]
    [InlineData(AccountTier.Business)]
    [InlineData(AccountTier.Enterprise)]
    public void CreateNew_WithAllTiers_CreatesSuccessfully(AccountTier tier)
    {
        // Arrange
        var name = "Test";
        var email = "test@test.com";

        // Act
        var result = AccountDetails.CreateNew(name, email, tier, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Tier.Should().Be(tier);
    }
}
