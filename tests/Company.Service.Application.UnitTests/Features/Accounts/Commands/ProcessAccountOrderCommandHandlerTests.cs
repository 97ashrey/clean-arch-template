using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Accounts.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Accounts.Commands;

public class ProcessAccountOrderCommandHandlerTests : DbContextTestBase
{
    private readonly ProcessAccountOrderCommandHandler _sut;

    public ProcessAccountOrderCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new ProcessAccountOrderCommand
        {
            AccountOrderId = Guid.NewGuid()
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"AccountOrder with Id {command.AccountOrderId} not found!");
    }

    [Fact]
    public async Task Handle_WithOrderNotPending_ReturnsBadRequestError()
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

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        var accountOrder = AccountOrder.CreateNew(
            tenantId: tenantId,
            accountDetails: AccountDetails.CreateNew(
                name: "Test Account",
                email: "test@example.com",
                tier: AccountTier.Business,
                invoiceAdressId: invoiceAddress.Id
            ).Value!,
            contactInformation: ContactInformation.CreateNew(
                firstName: "John",
                lastName: "Doe",
                email: "john@example.com",
                phoneNumber: "+1234567890"
            ).Value!,
            createdDate: DateTime.UtcNow
        ).Value!;

        // Transition to Processing
        accountOrder.StartProcessing();

        DbContext.AccountOrders.Add(accountOrder);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new ProcessAccountOrderCommand
        {
            AccountOrderId = accountOrder.Id
        };

        // Act — order is in Processing state, not Pending
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<BadRequestError>();
    }

    [Fact]
    public async Task Handle_WithValidPendingOrder_StartsProcessing()
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

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        var accountOrder = AccountOrder.CreateNew(
            tenantId: tenantId,
            accountDetails: AccountDetails.CreateNew(
                name: "Test Account",
                email: "test@example.com",
                tier: AccountTier.Business,
                invoiceAdressId: invoiceAddress.Id
            ).Value!,
            contactInformation: ContactInformation.CreateNew(
                firstName: "John",
                lastName: "Doe",
                email: "john@example.com",
                phoneNumber: "+1234567890"
            ).Value!,
            createdDate: DateTime.UtcNow
        ).Value!;

        DbContext.AccountOrders.Add(accountOrder);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new ProcessAccountOrderCommand
        {
            AccountOrderId = accountOrder.Id
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(accountOrder.Id);
        result.Value.Status.Should().Be(AccountOrderStatus.Processing);

        // Verify it was persisted
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.AccountOrders.FirstOrDefault(o => o.Id == accountOrder.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(accountOrder.Id);
        persisted.TenantId.Should().Be(accountOrder.TenantId);
        persisted.AccountDetails.Name.Should().Be(accountOrder.AccountDetails.Name);
        persisted.AccountDetails.Email.Should().Be(accountOrder.AccountDetails.Email);
        persisted.AccountDetails.Tier.Should().Be(accountOrder.AccountDetails.Tier);
        persisted.AccountDetails.InvoiceAddressId.Should().Be(accountOrder.AccountDetails.InvoiceAddressId);
        persisted.ContactInformation.FirstName.Should().Be(accountOrder.ContactInformation.FirstName);
        persisted.ContactInformation.LastName.Should().Be(accountOrder.ContactInformation.LastName);
        persisted.ContactInformation.Email.Should().Be(accountOrder.ContactInformation.Email);
        persisted.ContactInformation.PhoneNumber.Should().Be(accountOrder.ContactInformation.PhoneNumber);
        persisted.Status.Should().Be(AccountOrderStatus.Processing);
        persisted.CreatedDate.Should().Be(accountOrder.CreatedDate);
        persisted.AccountId.Should().BeNull();
        persisted.CompletedDate.Should().BeNull();
    }
}