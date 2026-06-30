using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class RemoveAccountTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task RemoveAccount_ReturnsNotFoundWhenAccountDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{nonExistentId}/remove", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Account with Id {nonExistentId} not found.");
    }

    [Fact]
    public async Task RemoveAccount_ReturnsBadRequestWhenAccountIsActive()
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

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act — trying to remove an active account
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/remove", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Can't remove the account it is not in {AccountStatus.Suspended} state!");
    }

    [Fact]
    public async Task RemoveAccount_ReturnsOkAndRemovesAccount()
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

        // Suspend first (remove requires Suspended state)
        account.Suspend(DateTime.UtcNow);

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}/remove", (object?)null);

        // Assert — response contract
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<V1Contracts.Account>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
        result.TenantId.Should().Be(account.TenantId);
        result.Name.Should().Be(account.Name);
        result.Email.Should().Be(account.Email);
        result.Tier.Should().Be((V1Contracts.AccountTier)account.Tier);
        result.Status.Should().Be(V1Contracts.AccountStatus.Removed);
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
        persisted.Status.Should().Be(AccountStatus.Removed);
        persisted.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
    }
}