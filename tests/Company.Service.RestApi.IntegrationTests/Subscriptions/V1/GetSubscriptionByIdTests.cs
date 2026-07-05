using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Subscriptions.V1;

public class GetSubscriptionByIdTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetSubscriptionById_ReturnsNotFoundWhenSubscriptionDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/{accountId}/subscriptions/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Subscription with Id {nonExistentId} not found.");
    }

    [Fact]
    public async Task GetSubscriptionById_ReturnsSubscription()
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

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/{account.Id}/subscriptions/{subscription.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<V1Contracts.Subscription>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(subscription.Id);
        result.AccountId.Should().Be(subscription.AccountId);
        result.Name.Should().Be(subscription.Name);
        result.FriendlyName.Should().Be(subscription.FriendlyName);
        result.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        result.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        result.BillCycle.Should().Be((V1Contracts.BillCycle)subscription.BillCycle);
        result.StartDate.Should().Be(subscription.StartDate);
        result.EndDate.Should().Be(subscription.EndDate);
        result.ProductId.Should().Be(subscription.ProductId);
        result.Status.Should().Be(V1Contracts.SubscriptionStatus.Active);
        result.SuspendedDate.Should().BeNull();
    }
}