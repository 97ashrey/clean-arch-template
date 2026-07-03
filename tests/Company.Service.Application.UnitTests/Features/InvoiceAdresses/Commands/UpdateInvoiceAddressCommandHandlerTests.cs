//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.InvoiceAddresses.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.InvoiceAdresses.Commands;

public class UpdateInvoiceAddressCommandHandlerTests : DbContextTestBase
{
    private readonly UpdateInvoiceAddressCommandHandler _sut;

    public UpdateInvoiceAddressCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithValidCommand_UpdatesAndReturnsInvoiceAddress()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Original Name",
            address: Address.CreateNew(
                country: "USA",
                city: "OriginalCity",
                zipCode: "12345",
                street: "Original St",
                number: "10"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync(CancellationToken.None);
        DbContext.ChangeTracker.Clear();

        var command = new UpdateInvoiceAddressCommand
        {
            Id = invoiceAddress.Id,
            Name = "Updated Name",
            Address = new()
            {
                Street = "New St",
                City = "NewCity",
                ZipCode = "54321",
                Country = "Canada",
                Number = "42"
            }
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(invoiceAddress.Id);
        result.Value!.TenantId.Should().Be(invoiceAddress.TenantId);
        result.Value!.Name.Should().Be(command.Name);
        result.Value!.Address.Street.Should().Be(command.Address.Street);
        result.Value!.Address.City.Should().Be(command.Address.City);
        result.Value!.Address.ZipCode.Should().Be(command.Address.ZipCode);
        result.Value!.Address.Country.Should().Be(command.Address.Country);
        result.Value!.Address.Number.Should().Be(command.Address.Number);

        // Verify it was persisted to the database
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.InvoiceAdresses.FirstOrDefault(a => a.Id == invoiceAddress.Id);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(invoiceAddress.TenantId);
        persisted.Name.Should().Be(command.Name);
        persisted.Address.Street.Should().Be(command.Address.Street);
        persisted.Address.City.Should().Be(command.Address.City);
        persisted.Address.ZipCode.Should().Be(command.Address.ZipCode);
        persisted.Address.Country.Should().Be(command.Address.Country);
        persisted.Address.Number.Should().Be(command.Address.Number);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new UpdateInvoiceAddressCommand
        {
            Id = nonExistentId,
            Name = "Updated Name",
            Address = new()
            {
                Street = "New St",
                City = "NewCity",
                ZipCode = "54321",
                Country = "Canada",
                Number = "42"
            }
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Invoice address with Id {nonExistentId} not found.");
    }

    [Fact]
    public async Task Handle_WithInvalidData_ReturnsValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Original Name",
            address: Address.CreateNew(
                country: "USA",
                city: "OriginalCity",
                zipCode: "12345",
                street: "Original St",
                number: "10"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync(CancellationToken.None);
        DbContext.ChangeTracker.Clear();

        var command = new UpdateInvoiceAddressCommand
        {
            Id = invoiceAddress.Id,
            Name = "Updated Name",
            Address = new()
            {
                Street = "New St",
                City = "NewCity",
                ZipCode = "54321",
                Country = "Canada",
                Number = string.Empty  // Invalid: empty number
            }
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<ValidationError>();
    }
}
//__EXAMPLE_END__