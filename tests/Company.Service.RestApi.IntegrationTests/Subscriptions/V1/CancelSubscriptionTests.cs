using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Subscriptions.V1;

public class CancelSubscriptionTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CancelSubscription_ReturnsNotFoundWhenSubscriptionDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{accountId}/subscriptions/{nonExistentId}/cancel", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Subscription with Id {nonExistentId} not found.");
    }

    [Fact]
    public async Task CancelSubscription_ReturnsBadRequestWhenSubscriptionIsActive()
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

        // Act — trying to cancel an active subscription (must be Suspended first)
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/subscriptions/{subscription.Id}/cancel", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Can't cancel the subscription it is not in {Domain.Entities.SubscriptionStatus.Suspended} state!");
    }

    [Fact]
    public async Task CancelSubscription_ReturnsOkAndCancelsSubscription()
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

        // Suspend first (Cancel requires Suspended state)
        subscription.Suspend(DateTime.UtcNow);

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/subscriptions/{subscription.Id}/cancel", (object?)null);

        // Assert — response contract
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<V1Contracts.Subscription>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(subscription.Id);
        result.AccountId.Should().Be(account.Id);
        result.Name.Should().Be(subscription.Name);
        result.FriendlyName.Should().Be(subscription.FriendlyName);
        result.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        result.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        result.BillCycle.Should().Be((V1Contracts.BillCycle)subscription.BillCycle);
        result.StartDate.Should().Be(subscription.StartDate);
        result.EndDate.Should().Be(subscription.EndDate);
        result.ProductId.Should().Be(subscription.ProductId);
        result.Status.Should().Be(V1Contracts.SubscriptionStatus.Canceled);
        result.SuspendedDate.Should().NotBeNull();

        // Assert — persistence
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.Subscriptions.FindAsync([subscription.Id]);
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
}