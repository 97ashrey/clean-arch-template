//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.InvoiceAddresses.V1;

public class UpdateInvoiceAddressTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<string, Func<V1Contracts.UpdateInvoiceAddressRequest, V1Contracts.UpdateInvoiceAddressRequest>> ValidationTestCases
    {
        get
        {
            var data = new TheoryData<string, Func<V1Contracts.UpdateInvoiceAddressRequest, V1Contracts.UpdateInvoiceAddressRequest>>
            {
                { "Name", r => r with { Name = string.Empty } },
                { "Address.Street", r => r with { Address = r.Address with { Street = string.Empty } } },
                { "Address.City", r => r with { Address = r.Address with { City = string.Empty } } },
                { "Address.ZipCode", r => r with { Address = r.Address with { ZipCode = string.Empty } } },
                { "Address.Country", r => r with { Address = r.Address with { Country = string.Empty } } },
                { "Address.Number", r => r with { Address = r.Address with { Number = string.Empty } } }
            };
            return data;
        }
    }

    [Fact]
    public async Task UpdateInvoiceAddress_ReturnsNotFound_WhenInvoiceAddressDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new V1Contracts.UpdateInvoiceAddressRequest
        {
            Name = "Updated Name",
            Address = new V1Contracts.AddressRequest
            {
                Street = "New St",
                City = "NewCity",
                ZipCode = "54321",
                Country = "Canada",
                Number = "42"
            }
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/invoice-addresses/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().Be($"Invoice address with Id {nonExistentId} not found.");
    }

    [Theory]
    [MemberData(nameof(ValidationTestCases))]
    public async Task UpdateInvoiceAddress_ReturnsBadRequest_WhenFieldIsInvalid(
        string expectedErrorKey,
        Func<V1Contracts.UpdateInvoiceAddressRequest, V1Contracts.UpdateInvoiceAddressRequest> invalidate)
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Original Name",
            address: Address.CreateNew(
                country: "USA",
                city: "OriginalCity",
                zipCode: "12345",
                street: "Original St",
                number: "10"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var request = invalidate(new()
        {
            Name = "Updated Name",
            Address = new V1Contracts.AddressRequest
            {
                Street = "New St",
                City = "NewCity",
                ZipCode = "54321",
                Country = "Canada",
                Number = "42"
            }
        });

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/invoice-addresses/{invoiceAddress.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey(expectedErrorKey);
    }

    [Fact]
    public async Task UpdateInvoiceAddress_ReturnsOkAndSavesToDatabase()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Original Name",
            address: Address.CreateNew(
                country: "USA",
                city: "OriginalCity",
                zipCode: "12345",
                street: "Original St",
                number: "10"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        var request = new V1Contracts.UpdateInvoiceAddressRequest
        {
            Name = "Updated Name",
            Address = new V1Contracts.AddressRequest
            {
                Street = "New St",
                City = "NewCity",
                ZipCode = "54321",
                Country = "Canada",
                Number = "42"
            }
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/v1/invoice-addresses/{invoiceAddress.Id}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updatedInvoiceAddress = await response.Content.ReadFromJsonAsync<V1Contracts.InvoiceAddress>();
        updatedInvoiceAddress.Should().NotBeNull();
        updatedInvoiceAddress!.Id.Should().Be(invoiceAddress.Id);
        updatedInvoiceAddress.TenantId.Should().Be(invoiceAddress.TenantId);
        updatedInvoiceAddress.Name.Should().Be(request.Name);
        updatedInvoiceAddress.Address.Street.Should().Be(request.Address.Street);
        updatedInvoiceAddress.Address.City.Should().Be(request.Address.City);
        updatedInvoiceAddress.Address.ZipCode.Should().Be(request.Address.ZipCode);
        updatedInvoiceAddress.Address.Country.Should().Be(request.Address.Country);
        updatedInvoiceAddress.Address.Number.Should().Be(request.Address.Number);

        // Verify it was saved to database
        DbContext.ChangeTracker.Clear();
        var persisted = await DbContext.InvoiceAdresses.FindAsync([invoiceAddress.Id], CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(invoiceAddress.TenantId);
        persisted.Name.Should().Be(request.Name);
        persisted.Address.Street.Should().Be(request.Address.Street);
        persisted.Address.City.Should().Be(request.Address.City);
        persisted.Address.ZipCode.Should().Be(request.Address.ZipCode);
        persisted.Address.Country.Should().Be(request.Address.Country);
        persisted.Address.Number.Should().Be(request.Address.Number);
    }
}
//__EXAMPLE_END__