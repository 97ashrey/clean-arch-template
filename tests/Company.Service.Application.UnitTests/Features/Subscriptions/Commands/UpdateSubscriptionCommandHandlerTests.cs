using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Subscriptions.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Commands;

public class UpdateSubscriptionCommandHandlerTests : DbContextTestBase
{
    private readonly UpdateSubscriptionCommandHandler _sut;

    public UpdateSubscriptionCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new UpdateSubscriptionCommand
        {
            AccountId = Guid.NewGuid(),
            Id = Guid.NewGuid(),
            FriendlyName = "New Friendly Name"
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Subscription with Id {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithValidIdAndFriendlyName_UpdatesAndReturnsSubscription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 12, 31);

        // Seed an InvoiceAddress and Account — required by FK constraints
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

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        var subscription = Subscription.CreateNew(
            accountId: account.Id,
            name: "Premium",
            friendlyName: "Original Friendly Name",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: startDate,
            endDate: endDate,
            productId: productId
        ).Value!;

        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new UpdateSubscriptionCommand
        {
            AccountId = account.Id,
            Id = subscription.Id,
            FriendlyName = "Updated Friendly Name"
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(subscription.Id);
        result.Value.AccountId.Should().Be(account.Id);
        result.Value.Name.Should().Be(subscription.Name);
        result.Value.FriendlyName.Should().Be("Updated Friendly Name");
        result.Value.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        result.Value.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        result.Value.BillCycle.Should().Be(subscription.BillCycle);
        result.Value.StartDate.Should().Be(subscription.StartDate);
        result.Value.EndDate.Should().Be(subscription.EndDate);
        result.Value.ProductId.Should().Be(subscription.ProductId);
        result.Value.Status.Should().Be(SubscriptionStatus.Active);
        result.Value.SuspendedDate.Should().BeNull();

        // Verify it was persisted — all fields, not just the changed one
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Subscriptions.FirstOrDefault(s => s.Id == subscription.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(subscription.Id);
        persisted.AccountId.Should().Be(account.Id);
        persisted.Name.Should().Be(subscription.Name);
        persisted.FriendlyName.Should().Be("Updated Friendly Name");
        persisted.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        persisted.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        persisted.BillCycle.Should().Be(subscription.BillCycle);
        persisted.StartDate.Should().Be(subscription.StartDate);
        persisted.EndDate.Should().Be(subscription.EndDate);
        persisted.ProductId.Should().Be(subscription.ProductId);
        persisted.Status.Should().Be(SubscriptionStatus.Active);
        persisted.SuspendedDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithEmptyFriendlyName_ReturnsValidationError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        // Seed an InvoiceAddress and Account — required by FK constraints
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

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        var subscription = Subscription.CreateNew(
            accountId: account.Id,
            name: "Premium",
            friendlyName: "Original Friendly Name",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;

        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new UpdateSubscriptionCommand
        {
            AccountId = account.Id,
            Id = subscription.Id,
            FriendlyName = string.Empty
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<ValidationError>();

        // Verify state was not changed
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Subscriptions.FirstOrDefault(s => s.Id == subscription.Id);
        persisted.Should().NotBeNull();
        persisted!.FriendlyName.Should().Be("Original Friendly Name");
    }
}