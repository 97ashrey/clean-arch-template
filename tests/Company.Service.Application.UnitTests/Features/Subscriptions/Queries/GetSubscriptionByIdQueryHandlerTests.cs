using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Subscriptions.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Queries;

public class GetSubscriptionByIdQueryHandlerTests : DbContextTestBase
{
    private readonly GetSubscriptionByIdQueryHandler _sut;

    public GetSubscriptionByIdQueryHandlerTests()
    {
        _sut = new GetSubscriptionByIdQueryHandler(DbContext);
    }

    [Fact]
    public async Task Handle_WithValidId_ReturnsSubscription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

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
            friendlyName: "Premium Subscription",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;

        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetSubscriptionByIdQuery
        {
            AccountId = account.Id,
            Id = subscription.Id
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(subscription.Id);
        result.Value.AccountId.Should().Be(subscription.AccountId);
        result.Value.Name.Should().Be(subscription.Name);
        result.Value.FriendlyName.Should().Be(subscription.FriendlyName);
        result.Value.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        result.Value.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        result.Value.BillCycle.Should().Be(subscription.BillCycle);
        result.Value.StartDate.Should().Be(subscription.StartDate);
        result.Value.EndDate.Should().Be(subscription.EndDate);
        result.Value.ProductId.Should().Be(subscription.ProductId);
        result.Value.Status.Should().Be(SubscriptionStatus.Active);
        result.Value.SuspendedDate.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetSubscriptionByIdQuery
        {
            AccountId = Guid.NewGuid(),
            Id = Guid.NewGuid()
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_WithValidIdButWrongAccountId_ReturnsNotFoundError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

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
            friendlyName: "Premium Subscription",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;

        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetSubscriptionByIdQuery
        {
            AccountId = Guid.NewGuid(), // wrong account
            Id = subscription.Id
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task Handle_WithMultipleSubscriptions_ReturnsOnlyRequestedSubscription()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

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

        var subscription1 = Subscription.CreateNew(
            accountId: account.Id,
            name: "Premium",
            friendlyName: "Premium Subscription",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId1
        ).Value!;

        var subscription2 = Subscription.CreateNew(
            accountId: account.Id,
            name: "Basic",
            friendlyName: "Basic Subscription",
            purchasePrice: Price.CreateNew(9.99m, "USD").Value!,
            billCycle: BillCycle.Yearly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId2
        ).Value!;

        DbContext.Subscriptions.AddRange(subscription1, subscription2);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        var query = new GetSubscriptionByIdQuery
        {
            AccountId = account.Id,
            Id = subscription1.Id
        };

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(subscription1.Id);
        result.Value.Name.Should().Be(subscription1.Name);
        result.Value.Id.Should().NotBe(subscription2.Id);
    }
}