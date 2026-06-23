using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class CompleteAccountOrderTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CompleteAccountOrder_ReturnsNotFoundWhenOrderDoesNotExist()
    {
        // Arrange
        var nonExistentOrderId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{nonExistentOrderId}/complete", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"AccountOrder with Id {nonExistentOrderId} not found!");
    }

    [Fact]
    public async Task CompleteAccountOrder_ReturnsBadRequestWhenOrderIsNotProcessing()
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

        // Act — order is in Pending state, not Processing
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{accountOrder.Id}/complete", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Can't complete order. It is not in {AccountOrderStatus.Processing} state!.");
    }

    [Fact]
    public async Task CompleteAccountOrder_ReturnsOkAndCompletesOrder()
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

        // Act — now complete
        var response = await Client.PutAsJsonAsync($"/api/v1/accounts/orders/{accountOrder.Id}/complete", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var completedOrder = await response.Content.ReadFromJsonAsync<V1Contracts.AccountOrder>();
        completedOrder.Should().NotBeNull();
        completedOrder!.Id.Should().Be(accountOrder.Id);
        completedOrder.TenantId.Should().Be(accountOrder.TenantId);
        completedOrder.AccountId.Should().NotBeNull();
        completedOrder.Status.Should().Be(V1Contracts.AccountOrderStatus.Completed);
        completedOrder.AccountDetails.Name.Should().Be(accountOrder.AccountDetails.Name);
        completedOrder.AccountDetails.Email.Should().Be(accountOrder.AccountDetails.Email);
        completedOrder.AccountDetails.Tier.Should().Be((V1Contracts.AccountTier)accountOrder.AccountDetails.Tier);
        completedOrder.AccountDetails.InvoiceAddressId.Should().Be(accountOrder.AccountDetails.InvoiceAddressId);
        completedOrder.ContactInformation.FirstName.Should().Be(accountOrder.ContactInformation.FirstName);
        completedOrder.ContactInformation.LastName.Should().Be(accountOrder.ContactInformation.LastName);
        completedOrder.ContactInformation.Email.Should().Be(accountOrder.ContactInformation.Email);
        completedOrder.ContactInformation.PhoneNumber.Should().Be(accountOrder.ContactInformation.PhoneNumber);
        completedOrder.CreatedDate.Should().BeCloseTo(accountOrder.CreatedDate, TimeSpan.FromSeconds(1));

        // Verify the order was persisted with completed status
        DbContext.ChangeTracker.Clear();
        var persistedOrder = await DbContext.AccountOrders.FindAsync([accountOrder.Id], CancellationToken.None);
        persistedOrder.Should().NotBeNull();
        persistedOrder!.Status.Should().Be(AccountOrderStatus.Completed);
        persistedOrder.AccountId.Should().NotBeNull();
        persistedOrder.CompletedDate.Should().NotBeNull();

        // Verify the account was created
        var persistedAccount = await DbContext.Accounts.FindAsync([persistedOrder.AccountId!.Value], CancellationToken.None);
        persistedAccount.Should().NotBeNull();
        persistedAccount!.Id.Should().Be(persistedOrder.AccountId!.Value);
        persistedAccount.TenantId.Should().Be(accountOrder.TenantId);
        persistedAccount.Name.Should().Be(accountOrder.AccountDetails.Name);
        persistedAccount.Email.Should().Be(accountOrder.AccountDetails.Email);
        persistedAccount.Tier.Should().Be(accountOrder.AccountDetails.Tier);
        persistedAccount.InvoiceAddressId.Should().Be(accountOrder.AccountDetails.InvoiceAddressId);
        persistedAccount.Status.Should().Be(AccountStatus.Active);
        persistedAccount.SuspendedDate.Should().BeNull();
    }
}
