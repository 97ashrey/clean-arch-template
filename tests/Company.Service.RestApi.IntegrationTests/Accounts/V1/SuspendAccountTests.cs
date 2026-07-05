using AwesomeAssertions;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using DomainSubscriptionStatus = Company.Service.Domain.Entities.SubscriptionStatus;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class SuspendAccountTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task SuspendAccount_ReturnsNotFoundWhenAccountDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{nonExistentId}/suspend", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Account with Id {nonExistentId} not found.");
    }

    [Fact]
    public async Task SuspendAccount_ReturnsBadRequestWhenAccountIsAlreadySuspended()
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
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        // Suspend before persisting
        account.Suspend(DateTime.UtcNow);

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act — trying to suspend an already-suspended account
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/suspend", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Can't suspend the account it is not in {AccountStatus.Active} state!");
    }

    [Fact]
    public async Task SuspendAccount_ReturnsBadRequestWhenAccountHasNonCanceledSubscriptions()
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

        // Act — trying to suspend an account with active subscriptions
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/suspend", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be("Can't suspend the account because it has non-canceled subscriptions!");
    }

    [Fact]
    public async Task SuspendAccount_ReturnsOkAndSuspendsAccount()
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

        FakeTimeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 28, 12, 0, 0, TimeSpan.Zero));

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

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/suspend", (object?)null);

        // Assert — response contract
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<V1Contracts.Account>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
        result.TenantId.Should().Be(account.TenantId);
        result.Name.Should().Be(account.Name);
        result.Email.Should().Be(account.Email);
        result.Tier.Should().Be((V1Contracts.AccountTier)account.Tier);
        result.Status.Should().Be(V1Contracts.AccountStatus.Suspended);
        result.SuspendedDate.Should().Be(FakeTimeProvider.GetUtcNowDateTime());
        result.InvoiceAddressId.Should().Be(account.InvoiceAddressId);

        // Assert — persistence
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.Accounts.FindAsync([account.Id]);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(account.Id);
        persisted.TenantId.Should().Be(account.TenantId);
        persisted.Name.Should().Be(account.Name);
        persisted.Email.Should().Be(account.Email);
        persisted.Tier.Should().Be(account.Tier);
        persisted.Status.Should().Be(AccountStatus.Suspended);
        persisted.SuspendedDate.Should().Be(FakeTimeProvider.GetUtcNowDateTime());
        persisted.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
    }
}