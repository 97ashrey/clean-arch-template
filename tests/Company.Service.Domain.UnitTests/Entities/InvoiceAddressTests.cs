//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.UnitTests.Entities;

public class InvoiceAddressTests
{
    private static readonly Address ValidAddress = Address.CreateNew("USA", "New York", "10001", "Broadway", "123").Value!;

    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedInvoiceAddress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var name = "Main Office";

        // Act
        var result = InvoiceAddress.CreateNew(tenantId, name, ValidAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value.TenantId.Should().Be(tenantId);
        result.Value.Name.Should().Be(name);
        result.Value.Address.Should().Be(ValidAddress);
    }

    [Fact]
    public void CreateNew_WithEmptyTenantId_ReturnsValidationError()
    {
        // Act
        var result = InvoiceAddress.CreateNew(Guid.Empty, "Main Office", ValidAddress);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "tenantId");
    }

    [Fact]
    public void CreateNew_WithEmptyName_ReturnsValidationError()
    {
        // Act
        var result = InvoiceAddress.CreateNew(Guid.NewGuid(), "", ValidAddress);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "name");
    }

    [Fact]
    public void CreateNew_WithNullName_ReturnsValidationError()
    {
        // Act
        var result = InvoiceAddress.CreateNew(Guid.NewGuid(), null!, ValidAddress);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "name");
    }

    [Fact]
    public void CreateNew_GeneratesUniqueId_ForEachCall()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var result1 = InvoiceAddress.CreateNew(tenantId, "Office 1", ValidAddress);
        var result2 = InvoiceAddress.CreateNew(tenantId, "Office 2", ValidAddress);

        // Assert
        result1.Value!.Id.Should().NotBe(result2.Value!.Id);
    }

    [Fact]
    public void ChangeName_WithValidName_UpdatesName()
    {
        // Arrange
        var invoiceAddress = CreateValidInvoiceAddress();
        var newName = "New Office Name";

        // Act
        var result = invoiceAddress.ChangeName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invoiceAddress.Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeName_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        var invoiceAddress = CreateValidInvoiceAddress();
        var originalName = invoiceAddress.Name;

        // Act
        var result = invoiceAddress.ChangeName("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        invoiceAddress.Name.Should().Be(originalName);
    }

    [Fact]
    public void ChangeName_WithNullName_ReturnsValidationError()
    {
        // Arrange
        var invoiceAddress = CreateValidInvoiceAddress();
        var originalName = invoiceAddress.Name;

        // Act
        var result = invoiceAddress.ChangeName(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        invoiceAddress.Name.Should().Be(originalName);
    }

    [Fact]
    public void ChangeAddress_UpdatesAddress()
    {
        // Arrange
        var invoiceAddress = CreateValidInvoiceAddress();
        var newAddress = Address.CreateNew("Canada", "Toronto", "M5A", "Maple St", "456").Value!;

        // Act
        invoiceAddress.ChangeAddress(newAddress);

        // Assert
        invoiceAddress.Address.Should().Be(newAddress);
    }

    [Fact]
    public void PrivateConstructor_CreatesInstanceWithDefaultValues()
    {
        // Arrange - use reflection to test private constructor is accessible for EF Core
        var ctor = typeof(InvoiceAddress).GetConstructors(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Assert
        ctor.Should().Contain(c => c.IsPrivate && c.GetParameters().Length == 0);
    }

    private static InvoiceAddress CreateValidInvoiceAddress()
    {
        var tenantId = Guid.NewGuid();
        return InvoiceAddress.CreateNew(tenantId, "Main Office", ValidAddress).Value!;
    }
}
//__EXAMPLE_END__
