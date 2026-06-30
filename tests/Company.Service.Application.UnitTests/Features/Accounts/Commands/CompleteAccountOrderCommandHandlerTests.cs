using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Accounts.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;

namespace Company.Service.Application.UnitTests.Features.Accounts.Commands;

public class CompleteAccountOrderCommandHandlerTests : DbContextTestBase
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly CompleteAccountOrderCommandHandler _sut;

    public CompleteAccountOrderCommandHandlerTests()
    {
        _fakeTimeProvider = new();
        _sut = new(DbContext, _fakeTimeProvider);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new CompleteAccountOrderCommand
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
    public async Task Handle_WithOrderNotProcessing_ReturnsBadRequestError()
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

        var command = new CompleteAccountOrderCommand
        {
            AccountOrderId = accountOrder.Id
        };

        // Act — order is in Pending state, not Processing
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<BadRequestError>();
    }

    [Fact]
    public async Task Handle_WithValidProcessingOrder_CompletesAndCreatesAccount()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
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
            createdDate: _fakeTimeProvider.GetUtcNow().DateTime
        ).Value!;

        // Transition to Processing
        accountOrder.StartProcessing();

        DbContext.AccountOrders.Add(accountOrder);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Advance time for the completion
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow.AddDays(1));

        var command = new CompleteAccountOrderCommand
        {
            AccountOrderId = accountOrder.Id
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(accountOrder.Id);
        result.Value.Status.Should().Be(AccountOrderStatus.Completed);
        result.Value.AccountId.Should().NotBeNull();
        result.Value.CompletedDate.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);

        // Verify the order was persisted
        DbContext.ChangeTracker.Clear();
        var persistedOrder = DbContext.AccountOrders.FirstOrDefault(o => o.Id == accountOrder.Id);
        persistedOrder.Should().NotBeNull();
        persistedOrder!.TenantId.Should().Be(accountOrder.TenantId);
        persistedOrder.AccountDetails.Name.Should().Be(accountOrder.AccountDetails.Name);
        persistedOrder.AccountDetails.Email.Should().Be(accountOrder.AccountDetails.Email);
        persistedOrder.AccountDetails.Tier.Should().Be(accountOrder.AccountDetails.Tier);
        persistedOrder.AccountDetails.InvoiceAddressId.Should().Be(accountOrder.AccountDetails.InvoiceAddressId);
        persistedOrder.ContactInformation.FirstName.Should().Be(accountOrder.ContactInformation.FirstName);
        persistedOrder.ContactInformation.LastName.Should().Be(accountOrder.ContactInformation.LastName);
        persistedOrder.ContactInformation.Email.Should().Be(accountOrder.ContactInformation.Email);
        persistedOrder.ContactInformation.PhoneNumber.Should().Be(accountOrder.ContactInformation.PhoneNumber);
        persistedOrder.Status.Should().Be(AccountOrderStatus.Completed);
        persistedOrder.CreatedDate.Should().Be(accountOrder.CreatedDate);
        persistedOrder.AccountId.Should().NotBeNull();
        persistedOrder.CompletedDate.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);

        // Verify the account was created
        var persistedAccount = DbContext.Accounts.FirstOrDefault(a => a.Id == persistedOrder.AccountId!.Value);
        persistedAccount.Should().NotBeNull();
        persistedAccount!.TenantId.Should().Be(tenantId);
        persistedAccount.Name.Should().Be(accountOrder.AccountDetails.Name);
        persistedAccount.Email.Should().Be(accountOrder.AccountDetails.Email);
        persistedAccount.Tier.Should().Be(accountOrder.AccountDetails.Tier);
        persistedAccount.InvoiceAddressId.Should().Be(accountOrder.AccountDetails.InvoiceAddressId);
        persistedAccount.Status.Should().Be(AccountStatus.Active);
        persistedAccount.SuspendedDate.Should().BeNull();
    }

}

