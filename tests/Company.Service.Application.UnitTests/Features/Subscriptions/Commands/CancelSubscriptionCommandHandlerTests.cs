using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Subscriptions.Commands;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Commands;

public class CancelSubscriptionCommandHandlerTests : DbContextTestBase
{
    private readonly CancelSubscriptionCommandHandler _sut;

    public CancelSubscriptionCommandHandlerTests()
    {
        _sut = new(DbContext);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var command = new CancelSubscriptionCommand { AccountId = Guid.NewGuid(), Id = Guid.NewGuid() };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
        result.Error!.Message.Should().Be($"Subscription with Id {command.Id} not found.");
    }

    [Fact]
    public async Task Handle_WithActiveSubscription_ReturnsBadRequestError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        var subscription = Subscription.CreateNew(
            accountId: account.Id,
            name: "Premium",
            friendlyName: "Premium Subscription",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new CancelSubscriptionCommand { AccountId = account.Id, Id = subscription.Id };

        // Act — trying to cancel an active subscription (must be Suspended first)
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<BadRequestError>();
    }

    [Fact]
    public async Task Handle_WithSuspendedSubscription_CancelsAndReturnsSubscription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        var subscription = Subscription.CreateNew(
            accountId: account.Id,
            name: "Premium",
            friendlyName: "Premium Subscription",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;

        subscription.Suspend(DateTime.UtcNow);

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var command = new CancelSubscriptionCommand { AccountId = account.Id, Id = subscription.Id };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(subscription.Id);
        result.Value.Status.Should().Be(Domain.Entities.SubscriptionStatus.Canceled);

        // Verify persistence — all fields
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Subscriptions.FirstOrDefault(s => s.Id == subscription.Id);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(subscription.Id);
        persisted.AccountId.Should().Be(account.Id);
        persisted.Name.Should().Be(subscription.Name);
        persisted.FriendlyName.Should().Be(subscription.FriendlyName);
        persisted.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        persisted.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        persisted.BillCycle.Should().Be(subscription.BillCycle);
        persisted.StartDate.Should().Be(subscription.StartDate);
        persisted.EndDate.Should().Be(subscription.EndDate);
        persisted.ProductId.Should().Be(subscription.ProductId);
        persisted.Status.Should().Be(Domain.Entities.SubscriptionStatus.Canceled);
        persisted.SuspendedDate.Should().NotBeNull();
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