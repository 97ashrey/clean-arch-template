using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class GetAccountOrderByIdTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetAccountOrderById_ReturnsNotFoundWhenAccountOrderDoesNotExist()
    {
        // Arrange
        var orderIdToNotGet = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/orders/{orderIdToNotGet}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Account order with Id {orderIdToNotGet} not found.");
    }

    [Fact]
    public async Task GetAccountOrderById_ReturnsAccountOrder()
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

        var accountOrderToGet = AccountOrder.CreateNew(
            tenantId: tenantId,
            accountDetails: AccountDetails.CreateNew(
                name: "Test Account",
                email: "test@example.com",
                tier: AccountTier.Business,
                invoiceAdressId: invoiceAddress.Id
            ).Value!,
            contactInformation: ContactInformation.CreateNew(
                firstName: "John",
                lastName: "Doe",
                email: "john@example.com",
                phoneNumber: "+1234567890"
            ).Value!,
            createdDate: DateTime.UtcNow
        ).Value!;

        DbContext.AccountOrders.Add(accountOrderToGet);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/orders/{accountOrderToGet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var accountOrder = await response.Content.ReadFromJsonAsync<V1Contracts.AccountOrder>();
        accountOrder.Should().NotBeNull();
        accountOrder!.Id.Should().Be(accountOrderToGet.Id);
        accountOrder.TenantId.Should().Be(accountOrderToGet.TenantId);
        accountOrder.AccountId.Should().BeNull();
        accountOrder.Status.Should().Be(V1Contracts.AccountOrderStatus.Pending);
        accountOrder.AccountDetails.Name.Should().Be(accountOrderToGet.AccountDetails.Name);
        accountOrder.AccountDetails.Email.Should().Be(accountOrderToGet.AccountDetails.Email);
        accountOrder.AccountDetails.Tier.Should().Be((V1Contracts.AccountTier)accountOrderToGet.AccountDetails.Tier);
        accountOrder.AccountDetails.InvoiceAddressId.Should().Be(accountOrderToGet.AccountDetails.InvoiceAddressId);
        accountOrder.ContactInformation.FirstName.Should().Be(accountOrderToGet.ContactInformation.FirstName);
        accountOrder.ContactInformation.LastName.Should().Be(accountOrderToGet.ContactInformation.LastName);
        accountOrder.ContactInformation.Email.Should().Be(accountOrderToGet.ContactInformation.Email);
        accountOrder.ContactInformation.PhoneNumber.Should().Be(accountOrderToGet.ContactInformation.PhoneNumber);
        accountOrder.CreatedDate.Should().BeCloseTo(accountOrderToGet.CreatedDate, TimeSpan.FromSeconds(1));
    }
}
