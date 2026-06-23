using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class GetAccountByIdTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAccountById_ReturnsNotFoundWhenAccountDoesNotExist()
    {
        // Arrange
        var accountIdToNotGet = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/{accountIdToNotGet}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Account with Id {accountIdToNotGet} not found.");
    }

    [Fact]
    public async Task GetAccountById_ReturnsAccount()
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

        var accountToGet = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Enterprise,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.Accounts.Add(accountToGet);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/{accountToGet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var account = await response.Content.ReadFromJsonAsync<V1Contracts.Account>();
        account.Should().NotBeNull();
        account!.Id.Should().Be(accountToGet.Id);
        account.TenantId.Should().Be(accountToGet.TenantId);
        account.Name.Should().Be(accountToGet.Name);
        account.Email.Should().Be(accountToGet.Email);
        account.Tier.Should().Be((V1Contracts.AccountTier)accountToGet.Tier);
        account.Status.Should().Be((V1Contracts.AccountStatus)accountToGet.Status);
        account.SuspendedDate.Should().BeNull();
        account.InvoiceAddressId.Should().Be(accountToGet.InvoiceAddressId);
    }
}
