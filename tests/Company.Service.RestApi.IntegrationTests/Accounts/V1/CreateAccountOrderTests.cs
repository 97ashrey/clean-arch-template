using AwesomeAssertions;
using Company.Service.Application.IntegrationEvents.V1.Accounts;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class CreateAccountOrderTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<string, Func<V1Contracts.CreateAccountOrderRequest, V1Contracts.CreateAccountOrderRequest>> ValidationTestCases
    {
        get
        {
            var data = new TheoryData<string, Func<V1Contracts.CreateAccountOrderRequest, V1Contracts.CreateAccountOrderRequest>>
            {
                { "TenantId", r => r with { TenantId = Guid.Empty } },
                { "AccountDetails.Name", r => r with { AccountDetails = r.AccountDetails with { Name = string.Empty } } },
                { "AccountDetails.Email", r => r with { AccountDetails = r.AccountDetails with { Email = "not-an-email" } } },
                { "AccountDetails.InvoiceAddressId", r => r with { AccountDetails = r.AccountDetails with { InvoiceAddressId = Guid.Empty } } },
                { "ContactInformation.FirstName", r => r with { ContactInformation = r.ContactInformation with { FirstName = string.Empty } } },
                { "ContactInformation.LastName", r => r with { ContactInformation = r.ContactInformation with { LastName = string.Empty } } },
                { "ContactInformation.Email", r => r with { ContactInformation = r.ContactInformation with { Email = "not-an-email" } } },
                { "ContactInformation.PhoneNumber", r => r with { ContactInformation = r.ContactInformation with { PhoneNumber = string.Empty } } }
            };
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(ValidationTestCases))]
    public async Task CreateAccountOrder_ReturnsBadRequest_WhenFieldIsInvalid(
        string expectedErrorKey,
        Func<V1Contracts.CreateAccountOrderRequest, V1Contracts.CreateAccountOrderRequest> invalidate)
    {
        // Arrange
        var request = invalidate(new()
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = V1Contracts.AccountTier.Business,
                InvoiceAddressId = Guid.NewGuid()
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        });

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey(expectedErrorKey);
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsOkAndSavesToDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Create an InvoiceAddress first — required by FK constraint
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

        DbContext.ChangeTracker.Clear();

        var request = new V1Contracts.CreateAccountOrderRequest
        {
            TenantId = tenantId,
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = V1Contracts.AccountTier.Business,
                InvoiceAddressId = invoiceAddress.Id
            },
            ContactInformation = new()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdAccountOrder = await response.Content.ReadFromJsonAsync<V1Contracts.AccountOrder>();
        createdAccountOrder.Should().NotBeNull();
        createdAccountOrder!.Id.Should().NotBeEmpty();
        createdAccountOrder.TenantId.Should().Be(request.TenantId);
        createdAccountOrder.AccountDetails.Name.Should().Be(request.AccountDetails.Name);
        createdAccountOrder.AccountDetails.Email.Should().Be(request.AccountDetails.Email);
        createdAccountOrder.AccountDetails.Tier.Should().Be(request.AccountDetails.Tier);
        createdAccountOrder.AccountDetails.InvoiceAddressId.Should().Be(invoiceAddress.Id);
        createdAccountOrder.ContactInformation.FirstName.Should().Be(request.ContactInformation.FirstName);
        createdAccountOrder.ContactInformation.LastName.Should().Be(request.ContactInformation.LastName);
        createdAccountOrder.ContactInformation.Email.Should().Be(request.ContactInformation.Email);
        createdAccountOrder.ContactInformation.PhoneNumber.Should().Be(request.ContactInformation.PhoneNumber);
        createdAccountOrder.Status.Should().Be(V1Contracts.AccountOrderStatus.Pending);
        createdAccountOrder.AccountId.Should().BeNull();
        createdAccountOrder.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify it was saved to database
        var persisted = await DbContext.AccountOrders.FindAsync([createdAccountOrder.Id], CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(request.TenantId);
        persisted.AccountId.Should().BeNull();
        persisted.Status.Should().Be(AccountOrderStatus.Pending);
        persisted.AccountDetails.Name.Should().Be(request.AccountDetails.Name);
        persisted.AccountDetails.Email.Should().Be(request.AccountDetails.Email);
        persisted.AccountDetails.Tier.Should().Be((AccountTier)request.AccountDetails.Tier);
        persisted.AccountDetails.InvoiceAddressId.Should().Be(request.AccountDetails.InvoiceAddressId);
        persisted.ContactInformation.FirstName.Should().Be(request.ContactInformation.FirstName);
        persisted.ContactInformation.LastName.Should().Be(request.ContactInformation.LastName);
        persisted.ContactInformation.Email.Should().Be(request.ContactInformation.Email);
        persisted.ContactInformation.PhoneNumber.Should().Be(request.ContactInformation.PhoneNumber);
        persisted.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify integration event was published
        var eventPublished = await MassTransitTestHarness.Published<AccountOrderCreatedEvent>();
        eventPublished.Should().BeTrue();
    }
}
