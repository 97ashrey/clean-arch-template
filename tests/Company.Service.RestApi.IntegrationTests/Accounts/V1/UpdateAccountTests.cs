using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class UpdateAccountTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task UpdateAccount_ReturnsNotFoundWhenAccountDoesNotExist()
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = new V1Contracts.UpdateAccountRequest
        {
            Name = "New Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{accountId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Account with Id {accountId} not found.");
    }

    [Fact]
    public async Task UpdateAccount_WithEmptyName_ReturnsBadRequest()
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
            name: "Original Name",
            email: "original@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var request = new V1Contracts.UpdateAccountRequest
        {
            Name = string.Empty
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validationProblem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        validationProblem.Should().NotBeNull();
        validationProblem!.Errors.Should().ContainKey("name");
    }

    [Fact]
    public async Task UpdateAccount_WithValidName_UpdatesAndReturnsAccount()
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
            name: "Original Name",
            email: "original@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var request = new V1Contracts.UpdateAccountRequest
        {
            Name = "Updated Name"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/{account.Id}", request);

        // Assert — response contract
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<V1Contracts.Account>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(account.Id);
        result.TenantId.Should().Be(account.TenantId);
        result.Name.Should().Be("Updated Name");
        result.Email.Should().Be(account.Email);
        result.Tier.Should().Be((V1Contracts.AccountTier)account.Tier);
        result.Status.Should().Be((V1Contracts.AccountStatus)account.Status);
        result.SuspendedDate.Should().BeNull();
        result.InvoiceAddressId.Should().Be(account.InvoiceAddressId);

        // Assert — persistence
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.Accounts.FindAsync([account.Id]);
        persisted.Should().NotBeNull();
        persisted!.Id.Should().Be(account.Id);
        persisted.TenantId.Should().Be(account.TenantId);
        persisted.Name.Should().Be("Updated Name");
        persisted.Email.Should().Be(account.Email);
        persisted.Tier.Should().Be(account.Tier);
        persisted.Status.Should().Be(AccountStatus.Active);
        persisted.SuspendedDate.Should().BeNull();
        persisted.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
    }
}