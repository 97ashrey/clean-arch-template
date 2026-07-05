using AwesomeAssertions;
using Company.Service.Application.Common.Utils;
using Company.Service.Application.IntegrationEvents.V1.Subscriptions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Subscriptions.V1;

public class CreateSubscriptionTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<string, Func<V1Contracts.CreateSubscriptionRequest, V1Contracts.CreateSubscriptionRequest>> ValidationTestCases
    {
        get
        {
            var data = new TheoryData<string, Func<V1Contracts.CreateSubscriptionRequest, V1Contracts.CreateSubscriptionRequest>>
            {
                { "Name", r => r with { Name = string.Empty } },
                { "FriendlyName", r => r with { FriendlyName = string.Empty } },
                { "PurchasePrice.Value", r => r with { PurchasePrice = r.PurchasePrice with { Value = 0 } } },
                { "PurchasePrice.Currency", r => r with { PurchasePrice = r.PurchasePrice with { Currency = string.Empty } } },
                { "PurchasePrice.Currency", r => r with { PurchasePrice = r.PurchasePrice with { Currency = "US" } } },
                { "ProductId", r => r with { ProductId = Guid.Empty } }
            };
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ValidationTestCases))]
    public async Task CreateSubscription_ReturnsBadRequest_WhenFieldIsInvalid(
        string expectedErrorKey,
        Func<V1Contracts.CreateSubscriptionRequest, V1Contracts.CreateSubscriptionRequest> invalidate)
    {
        // Arrange
        var accountId = Guid.NewGuid();
        var request = invalidate(new()
        {
            Name = "Premium",
            FriendlyName = "Premium Subscription",
            PurchasePrice = new V1Contracts.Price { Value = 29.99m, Currency = "USD" },
            BillCycle = V1Contracts.BillCycle.Monthly,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            ProductId = Guid.NewGuid()
        });

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/accounts/{accountId}/subscriptions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey(expectedErrorKey);
    }

    [Fact]
    public async Task CreateSubscription_ReturnsOkAndSavesToDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

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

        DbContext.ChangeTracker.Clear();

        FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);

        var request = new V1Contracts.CreateSubscriptionRequest
        {
            Name = "Premium",
            FriendlyName = "Premium Subscription",
            PurchasePrice = new V1Contracts.Price { Value = 29.99m, Currency = "USD" },
            BillCycle = V1Contracts.BillCycle.Monthly,
            StartDate = FakeTimeProvider.GetUtcNowDateTime(),
            EndDate = FakeTimeProvider.GetUtcNowDateTime().AddYears(1),
            ProductId = Guid.NewGuid()
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/accounts/{account.Id}/subscriptions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdSubscription = await response.Content.ReadFromJsonAsync<V1Contracts.Subscription>();
        createdSubscription.Should().NotBeNull();
        createdSubscription!.Id.Should().NotBeEmpty();
        createdSubscription.AccountId.Should().Be(account.Id);
        createdSubscription.Name.Should().Be(request.Name);
        createdSubscription.FriendlyName.Should().Be(request.FriendlyName);
        createdSubscription.PurchasePrice.Value.Should().Be(request.PurchasePrice.Value);
        createdSubscription.PurchasePrice.Currency.Should().Be(request.PurchasePrice.Currency);
        createdSubscription.BillCycle.Should().Be(request.BillCycle);
        createdSubscription.StartDate.Should().Be(request.StartDate);
        createdSubscription.EndDate.Should().Be(request.EndDate);
        createdSubscription.ProductId.Should().Be(request.ProductId);
        createdSubscription.Status.Should().Be(V1Contracts.SubscriptionStatus.Active);
        createdSubscription.SuspendedDate.Should().BeNull();

        // Verify it was saved to database
        var persisted = await DbContext.Subscriptions.FindAsync([createdSubscription.Id], CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.AccountId.Should().Be(account.Id);
        persisted.Name.Should().Be(request.Name);
        persisted.FriendlyName.Should().Be(request.FriendlyName);
        persisted.PurchasePrice.Value.Should().Be(request.PurchasePrice.Value);
        persisted.PurchasePrice.Currency.Should().Be(request.PurchasePrice.Currency);
        persisted.BillCycle.Should().Be((Domain.Entities.BillCycle)request.BillCycle);
        persisted.StartDate.Should().Be(request.StartDate);
        persisted.EndDate.Should().Be(request.EndDate);
        persisted.ProductId.Should().Be(request.ProductId);
        persisted.Status.Should().Be(Domain.Entities.SubscriptionStatus.Active);
        persisted.SuspendedDate.Should().BeNull();

        // Verify integration event was published
        var eventPublished = await MassTransitTestHarness.Published<SubscriptionCreatedEvent>();
        eventPublished.Should().BeTrue();
    }
}