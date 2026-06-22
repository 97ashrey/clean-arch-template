using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.InvoiceAddresses.V1;

public class GetInvoiceAddressByIdTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetInvoiceAddressById_ReturnsNotFoundWhenInvoiceAddressDoesNotExist()
    {
        // Arrange
        var invoiceAddressToNotGet = InvoiceAddress.CreateNew(
            tenantId: Guid.NewGuid(),
            name: "Home",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "HomeStreet",
                number: "20/34"
            ).Value!
        ).Value!;

        // Act
        var response = await Client.GetAsync($"/api/v1/invoice-addresses/{invoiceAddressToNotGet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Invoice address with Id {invoiceAddressToNotGet.Id} not found.");
    }

    [Fact]
    public async Task GetInvoiceAddressById_ReturnsInvoiceAddress()
    {
        // Arrange
        var invoiceAddressToGet = InvoiceAddress.CreateNew(
            tenantId: Guid.NewGuid(),
            name: "Home",
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: "HomeStreet",
                number: "20/34"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddressToGet);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync($"/api/v1/invoice-addresses/{invoiceAddressToGet.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var invoiceAddress = await response.Content.ReadFromJsonAsync<V1Contracts.InvoiceAddress>();

        invoiceAddress.Should().NotBeNull();
        invoiceAddress!.Id.Should().Be(invoiceAddressToGet.Id);
    }
}