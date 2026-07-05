using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Subscriptions.V1;

public class UpdateSubscriptionTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdateSubscription_ReturnsNotFoundWhenSubscriptionDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();
        var request = new V1Contracts.UpdateSubscriptionRequest
        {
            FriendlyName = "New Friendly Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{accountId}/subscriptions/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Subscription with Id {nonExistentId} not found.");
    }

    [Fact]
    public async Task UpdateSubscription_WithEmptyFriendlyName_ReturnsBadRequest()
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

        var request = new V1Contracts.UpdateSubscriptionRequest
        {
            FriendlyName = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/subscriptions/{subscription.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblem.Should().NotBeNull();
        validationProblem!.Errors.Should().ContainKey("FriendlyName");
    }

    [Fact]
    public async Task UpdateSubscription_WithValidFriendlyName_UpdatesAndReturnsSubscription()
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

        var request = new V1Contracts.UpdateSubscriptionRequest
        {
            FriendlyName = "Updated Friendly Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/subscriptions/{subscription.Id}", request);

        // Assert — response contract
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<V1Contracts.Subscription>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(subscription.Id);
        result.AccountId.Should().Be(account.Id);
        result.Name.Should().Be(subscription.Name);
        result.FriendlyName.Should().Be("Updated Friendly Name");
        result.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        result.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        result.BillCycle.Should().Be((V1Contracts.BillCycle)subscription.BillCycle);
        result.StartDate.Should().Be(subscription.StartDate);
        result.EndDate.Should().Be(subscription.EndDate);
        result.ProductId.Should().Be(subscription.ProductId);
        result.Status.Should().Be(V1Contracts.SubscriptionStatus.Active);
        result.SuspendedDate.Should().BeNull();

        // Assert — persistence
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.Subscriptions.FindAsync([subscription.Id]);
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
        persisted.Status.Should().Be(Domain.Entities.SubscriptionStatus.Active);
        persisted.SuspendedDate.Should().BeNull();
    }
}