using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

using V1Contracts = Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.InvoiceAddresses.V1;

public class CreateInvoiceAddressTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenTenantIdIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.Empty,
            Name = "Home",
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = "Main St",
                City = "TestCity",
                ZipCode = "12345",
                Country = "USA",
                Number = "10"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("TenantId");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenNameIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.NewGuid(),
            Name = string.Empty,
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = "Main St",
                City = "TestCity",
                ZipCode = "12345",
                Country = "USA",
                Number = "10"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Name");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenStreetIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.NewGuid(),
            Name = "Home",
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = string.Empty,
                City = "TestCity",
                ZipCode = "12345",
                Country = "USA",
                Number = "10"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Address.Street");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenCityIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.NewGuid(),
            Name = "Home",
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = "Main St",
                City = string.Empty,
                ZipCode = "12345",
                Country = "USA",
                Number = "10"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Address.City");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenZipCodeIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.NewGuid(),
            Name = "Home",
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = "Main St",
                City = "TestCity",
                ZipCode = string.Empty,
                Country = "USA",
                Number = "10"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Address.ZipCode");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenCountryIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.NewGuid(),
            Name = "Home",
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = "Main St",
                City = "TestCity",
                ZipCode = "12345",
                Country = string.Empty,
                Number = "10"
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Address.Country");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsBadRequestWhenNumberIsEmpty()
    {
        // Arrange
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = Guid.NewGuid(),
            Name = "Home",
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = "Main St",
                City = "TestCity",
                ZipCode = "12345",
                Country = "USA",
                Number = string.Empty
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey("Address.Number");
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsCreatedAndSavesToDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddressToCreate = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Home",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "12345",
                street: "Main St",
                number: "10"
            ).Value!
        ).Value!;

        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = tenantId,
            Name = invoiceAddressToCreate.Name,
            Address = new V1Contracts.CreateInvoiceAddressRequest.AddressRequest
            {
                Street = invoiceAddressToCreate.Address.Street,
                City = invoiceAddressToCreate.Address.City,
                ZipCode = invoiceAddressToCreate.Address.ZipCode,
                Country = invoiceAddressToCreate.Address.Country,
                Number = invoiceAddressToCreate.Address.Number
            }
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdInvoiceAddress = await response.Content.ReadFromJsonAsync<V1Contracts.InvoiceAddress>();
        createdInvoiceAddress.Should().NotBeNull();
        createdInvoiceAddress!.Id.Should().NotBeEmpty();
        createdInvoiceAddress!.TenantId.Should().Be(tenantId);
        createdInvoiceAddress!.Name.Should().Be(invoiceAddressToCreate.Name);

        // Verify it was saved to database
        var persisted = await DbContext.InvoiceAdresses.FindAsync(new object[] { createdInvoiceAddress.Id }, CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(tenantId);
    }
}