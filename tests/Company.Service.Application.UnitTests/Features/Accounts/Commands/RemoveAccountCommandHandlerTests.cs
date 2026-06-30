using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Accounts.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Accounts.Commands;

public class RemoveAccountCommandHandlerTests : DbContextTestBase
{
    private readonly RemoveAccountCommandHandler _sut;

    public RemoveAccountCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new RemoveAccountCommand { Id = Guid.NewGuid() };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Account with Id {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithActiveAccount_ReturnsBadRequestError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new RemoveAccountCommand { Id = account.Id };

        // Act — trying to remove an active account (must be suspended first)
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<BadRequestError>();
    }

    [Fact]
    public async Task Handle_WithSuspendedAccount_RemovesAndReturnsAccount()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        account.Suspend(DateTime.UtcNow);

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new RemoveAccountCommand { Id = account.Id };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(account.Id);
        result.Value.Status.Should().Be(AccountStatus.Removed);

        // Verify persistence — all fields
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Accounts.FirstOrDefault(a => a.Id == account.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(account.Id);
        persisted.TenantId.Should().Be(account.TenantId);
        persisted.Name.Should().Be(account.Name);
        persisted.Email.Should().Be(account.Email);
        persisted.Tier.Should().Be(account.Tier);
        persisted.Status.Should().Be(AccountStatus.Removed);
        persisted.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
    }

    private static InvoiceAddress CreateInvoiceAddress(Guid tenantId)
    {
        return InvoiceAddress.CreateNew(
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
    }
}