//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Application.IntegrationEvents.V1.InvoiceAddresses;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.InvoiceAddresses.V1;

public class CreateInvoiceAddressTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<string, Func<V1Contracts.CreateInvoiceAddressRequest, V1Contracts.CreateInvoiceAddressRequest>> ValidationTestCases
    {
        get
        {
            var data = new TheoryData<string, Func<V1Contracts.CreateInvoiceAddressRequest, V1Contracts.CreateInvoiceAddressRequest>>
            {
                { "TenantId", r => r with { TenantId = Guid.Empty } },
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

    [Theory]
    [MemberData(nameof(ValidationTestCases))]
    public async Task CreateInvoiceAddress_ReturnsBadRequest_WhenFieldIsInvalid(
        string expectedErrorKey,
        Func<V1Contracts.CreateInvoiceAddressRequest, V1Contracts.CreateInvoiceAddressRequest> invalidate)
    {
        // Arrange
        var request = invalidate(new()
        {
            TenantId = Guid.NewGuid(),
            Name = "Home",
            Address = new V1Contracts.AddressRequest
            {
                Street = "Main St",
                City = "TestCity",
                ZipCode = "12345",
                Country = "USA",
                Number = "10"
            }
        });

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/invoice-addresses", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Errors.Should().ContainKey(expectedErrorKey);
    }

    [Fact]
    public async Task CreateInvoiceAddress_ReturnsOkAndSavesToDatabase()
    {
        // Arrange
        FakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
        var tenantId = Guid.NewGuid();
        var request = new V1Contracts.CreateInvoiceAddressRequest
        {
            TenantId = tenantId,
            Name = "Home",
            Address = new V1Contracts.AddressRequest
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdInvoiceAddress = await response.Content.ReadFromJsonAsync<V1Contracts.InvoiceAddress>();
        createdInvoiceAddress.Should().NotBeNull();
        createdInvoiceAddress!.Id.Should().NotBeEmpty();
        createdInvoiceAddress.TenantId.Should().Be(request.TenantId);
        createdInvoiceAddress.Name.Should().Be(request.Name);
        createdInvoiceAddress.Address.Street.Should().Be(request.Address.Street);
        createdInvoiceAddress.Address.City.Should().Be(request.Address.City);
        createdInvoiceAddress.Address.ZipCode.Should().Be(request.Address.ZipCode);
        createdInvoiceAddress.Address.Country.Should().Be(request.Address.Country);
        createdInvoiceAddress.Address.Number.Should().Be(request.Address.Number);

        // Verify it was saved to database
        var persisted = await DbContext.InvoiceAdresses.FindAsync([createdInvoiceAddress.Id], CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.TenantId.Should().Be(request.TenantId);
        persisted.Name.Should().Be(request.Name);
        persisted.Address.Street.Should().Be(request.Address.Street);
        persisted.Address.City.Should().Be(request.Address.City);
        persisted.Address.ZipCode.Should().Be(request.Address.ZipCode);
        persisted.Address.Country.Should().Be(request.Address.Country);
        persisted.Address.Number.Should().Be(request.Address.Number);

        // Verify integration event was published
        var eventPublished = await MassTransitTestHarness.Published<InvoiceAddressCreatedEvent>();
        eventPublished.Should().BeTrue();
    }

}
//__EXAMPLE_END__