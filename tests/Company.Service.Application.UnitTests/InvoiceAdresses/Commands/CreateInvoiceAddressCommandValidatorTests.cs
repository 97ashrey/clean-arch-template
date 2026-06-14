using Company.Service.Application.InvoiceAddresses.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;
using System;
using Xunit;

namespace Company.Service.Application.UnitTests.InvoiceAdresses.Commands;

public class CreateInvoiceAddressCommandValidatorTests
{
    private readonly CreateInvoiceAddressCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
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
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.Empty,
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = string.Empty,
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyStreet_ShouldHaveError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = string.Empty,
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address.Street)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyCity_ShouldHaveError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = string.Empty,
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address.City)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyZipCode_ShouldHaveError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = string.Empty,
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address.ZipCode)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyCountry_ShouldHaveError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = string.Empty,
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address.Country)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyNumber_ShouldHaveError()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = "123 Main St",
                City = "New York",
                ZipCode = "10001",
                Country = "USA",
                Number = string.Empty
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address.Number)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithMultipleEmptyFields_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new CreateInvoiceAddressCommand
        {
            TenantId = Guid.Empty,
            Name = string.Empty,
            Address = new CreateInvoiceAddressCommand.AddressCommand
            {
                Street = string.Empty,
                City = string.Empty,
                ZipCode = "10001",
                Country = "USA",
                Number = "123"
            }
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Address.Street);
        result.ShouldHaveValidationErrorFor(x => x.Address.City);
        result.Errors.Should().HaveCount(4);
    }
}