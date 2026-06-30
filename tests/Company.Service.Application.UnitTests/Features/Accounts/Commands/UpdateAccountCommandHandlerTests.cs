using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Accounts.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Accounts.Commands;

public class UpdateAccountCommandHandlerTests : DbContextTestBase
{
    private readonly UpdateAccountCommandHandler _sut;

    public UpdateAccountCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new UpdateAccountCommand
        {
            Id = Guid.NewGuid(),
            Name = "New Name"
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Account with Id {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithValidIdAndName_UpdatesAndReturnsAccount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Default Invoice Address",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "12345",
                street: "Main St",
                number: "10"
            ).Value!
        ).Value!;

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Original Name",
            email: "original@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new UpdateAccountCommand
        {
            Id = account.Id,
            Name = "Updated Name"
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(account.Id);
        result.Value.Name.Should().Be("Updated Name");
        result.Value.Email.Should().Be(account.Email);
        result.Value.TenantId.Should().Be(account.TenantId);
        result.Value.Tier.Should().Be(account.Tier);
        result.Value.Status.Should().Be(account.Status);
        result.Value.InvoiceAddressId.Should().Be(account.InvoiceAddressId);

        // Verify it was persisted — all fields, not just the changed one
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Accounts.FirstOrDefault(a => a.Id == account.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(account.Id);
        persisted.TenantId.Should().Be(account.TenantId);
        persisted.Name.Should().Be("Updated Name");
        persisted.Email.Should().Be(account.Email);
        persisted.Tier.Should().Be(account.Tier);
        persisted.Status.Should().Be(AccountStatus.Active);
        persisted.SuspendedDate.Should().BeNull();
        persisted.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
    }

    [Fact]
    public async Task Handle_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Default Invoice Address",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "12345",
                street: "Main St",
                number: "10"
            ).Value!
        ).Value!;

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Original Name",
            email: "original@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new UpdateAccountCommand
        {
            Id = account.Id,
            Name = string.Empty
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<ValidationError>();

        // Verify state was not changed
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Accounts.FirstOrDefault(a => a.Id == account.Id);
        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Original Name");
    }
}