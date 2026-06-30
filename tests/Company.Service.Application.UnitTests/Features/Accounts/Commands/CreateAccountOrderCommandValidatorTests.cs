using AwesomeAssertions;
using Company.Service.Application.Features.Accounts.Commands;
using Company.Service.Domain.Entities;
using FluentValidation.TestHelper;

namespace Company.Service.Application.UnitTests.Features.Accounts.Commands;

public class CreateAccountOrderCommandValidatorTests
{
    private readonly CreateAccountOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTenantId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.Empty,
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyAccountDetailsName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = string.Empty,
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountDetails.Name)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithInvalidAccountDetailsEmail_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "not-an-email",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountDetails.Email)
            .WithErrorCode("EmailValidator");
    }

    [Fact]
    public void Validate_WithEmptyAccountDetailsInvoiceAddressId_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.Empty
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountDetails.InvoiceAddressId)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyContactInformationFirstName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = string.Empty,
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.FirstName)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyContactInformationLastName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = string.Empty,
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.LastName)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithInvalidContactInformationEmail_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "not-an-email",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.Email)
            .WithErrorCode("EmailValidator");
    }

    [Fact]
    public void Validate_WithEmptyContactInformationPhoneNumber_ShouldHaveError()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = string.Empty
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.PhoneNumber)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithMultipleEmptyFields_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new CreateAccountOrderCommand
        {
            TenantId = Guid.Empty,
            AccountDetails = new()
            {
                Name = string.Empty,
                Email = "test@example.com",
                Tier = AccountTier.Business,
                InvoiceAddressId = Guid.Empty
            },
            ContactInformation = new()
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Email = "john@example.com",
                PhoneNumber = string.Empty
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
        result.ShouldHaveValidationErrorFor(x => x.AccountDetails.Name);
        result.ShouldHaveValidationErrorFor(x => x.AccountDetails.InvoiceAddressId);
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.FirstName);
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.LastName);
        result.ShouldHaveValidationErrorFor(x => x.ContactInformation.PhoneNumber);
        result.Errors.Should().HaveCount(6);
    }
}
