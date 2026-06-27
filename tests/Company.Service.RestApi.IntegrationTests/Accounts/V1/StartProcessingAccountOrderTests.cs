using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class StartProcessingAccountOrderTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task StartProcessingAccountOrder_ReturnsNotFoundWhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{nonExistentOrderId}/start-processing", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"AccountOrder with Id {nonExistentOrderId} not found!");
    }

    [Fact]
    public async Task StartProcessingAccountOrder_ReturnsBadRequestWhenOrderIsNotPending()
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

        var accountOrder = AccountOrder.CreateNew(
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

        // Transition to Processing using domain method before persisting
        accountOrder.StartProcessing();

        DbContext.AccountOrders.Add(accountOrder);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act — order is now Processing, so start-processing should fail
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{accountOrder.Id}/start-processing", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Can't start processing order. It is not in {AccountOrderStatus.Pending} state!");
    }

    [Fact]
    public async Task StartProcessingAccountOrder_ReturnsOkAndUpdatesStatus()
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

        var accountOrder = AccountOrder.CreateNew(
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

        DbContext.AccountOrders.Add(accountOrder);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{accountOrder.Id}/start-processing", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedOrder = await response.Content.ReadFromJsonAsync<V1Contracts.AccountOrder>();
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Id.Should().Be(accountOrder.Id);
        updatedOrder.TenantId.Should().Be(accountOrder.TenantId);
        updatedOrder.AccountId.Should().BeNull();
        updatedOrder.Status.Should().Be(V1Contracts.AccountOrderStatus.Processing);
        updatedOrder.AccountDetails.Name.Should().Be(accountOrder.AccountDetails.Name);
        updatedOrder.AccountDetails.Email.Should().Be(accountOrder.AccountDetails.Email);
        updatedOrder.AccountDetails.Tier.Should().Be((V1Contracts.AccountTier)accountOrder.AccountDetails.Tier);
        updatedOrder.AccountDetails.InvoiceAddressId.Should().Be(accountOrder.AccountDetails.InvoiceAddressId);
        updatedOrder.ContactInformation.FirstName.Should().Be(accountOrder.ContactInformation.FirstName);
        updatedOrder.ContactInformation.LastName.Should().Be(accountOrder.ContactInformation.LastName);
        updatedOrder.ContactInformation.Email.Should().Be(accountOrder.ContactInformation.Email);
        updatedOrder.ContactInformation.PhoneNumber.Should().Be(accountOrder.ContactInformation.PhoneNumber);
        updatedOrder.CreatedDate.Should().BeCloseTo(accountOrder.CreatedDate, TimeSpan.FromSeconds(1));

        // Verify it was persisted
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.AccountOrders.FindAsync([accountOrder.Id], CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(AccountOrderStatus.Processing);
    }
}