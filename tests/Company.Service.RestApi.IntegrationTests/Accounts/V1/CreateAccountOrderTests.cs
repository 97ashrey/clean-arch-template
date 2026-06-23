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
    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenTenantIdIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
        {
            TenantId = Guid.Empty,
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
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("TenantId");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenAccountDetailsNameIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = string.Empty,
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
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("AccountDetails.Name");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenAccountDetailsEmailIsInvalid()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "not-an-email",
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
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("AccountDetails.Email");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenAccountDetailsInvoiceAddressIdIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
        {
            TenantId = Guid.NewGuid(),
            AccountDetails = new()
            {
                Name = "Test Account",
                Email = "test@example.com",
                Tier = V1Contracts.AccountTier.Business,
                InvoiceAddressId = Guid.Empty
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
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("AccountDetails.InvoiceAddressId");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenContactInformationFirstNameIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
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
                FirstName = string.Empty,
                LastName = "Doe",
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("ContactInformation.FirstName");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenContactInformationLastNameIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
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
                LastName = string.Empty,
                Email = "john@example.com",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("ContactInformation.LastName");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenContactInformationEmailIsInvalid()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
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
                Email = "not-an-email",
                PhoneNumber = "+1234567890"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("ContactInformation.Email");
    }

    [Fact]
    public async Task CreateAccountOrder_ReturnsBadRequestWhenContactInformationPhoneNumberIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateAccountOrderRequest
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
                PhoneNumber = string.Empty
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/accounts/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("ContactInformation.PhoneNumber");
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
        createdAccountOrder.TenantId.Should().Be(tenantId);
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

        // Verify it was saved to database
        var persisted = await DbContext.AccountOrders.FindAsync([createdAccountOrder.Id], CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(tenantId);
        persisted.Status.Should().Be(AccountOrderStatus.Pending);

        // Verify integration event was published
        var eventPublished = await MassTransitTestHarness.Published<AccountOrderCreatedEvent>();
        eventPublished.Should().BeTrue();
    }
}
