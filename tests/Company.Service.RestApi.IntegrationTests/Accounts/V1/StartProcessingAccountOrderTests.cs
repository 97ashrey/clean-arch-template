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
    public async Task StartProcessingAccountOrder_ReturnsInternalServerErrorWhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{nonExistentOrderId}/start-processing", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

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

        DbContext.AccountOrders.Add(accountOrder);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Start processing — first call succeeds
        var firstResponse = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{accountOrder.Id}/start-processing", (object?)null);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        DbContext.ChangeTracker.Clear();

        // Act — second call should fail because status is now Processing
        var secondResponse = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{accountOrder.Id}/start-processing", (object?)null);

        // Assert
        secondResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();
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
        updatedOrder.Status.Should().Be(V1Contracts.AccountOrderStatus.Processing);

        // Verify it was persisted
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.AccountOrders.FindAsync(new object[] { accountOrder.Id }, CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.Status.Should().Be(AccountOrderStatus.Processing);
    }
}
