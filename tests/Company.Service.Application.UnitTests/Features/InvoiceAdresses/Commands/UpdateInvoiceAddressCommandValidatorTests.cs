using AwesomeAssertions;
using Company.Service.Application.Features.InvoiceAddresses.Commands;
using FluentValidation.TestHelper;

namespace Company.Service.Application.UnitTests.Features.InvoiceAdresses.Commands;

public class UpdateInvoiceAddressCommandValidatorTests
{
    private readonly UpdateInvoiceAddressCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
    public void Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.Empty,
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = string.Empty,
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.NewGuid(),
            Name = "Valid Address",
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        var command = new UpdateInvoiceAddressCommand
        {
            Id = Guid.Empty,
            Name = string.Empty,
            Address = new UpdateInvoiceAddressCommand.AddressCommand
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
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Address.Street);
        result.ShouldHaveValidationErrorFor(x => x.Address.City);
        result.Errors.Should().HaveCount(4);
    }
}
