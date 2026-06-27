using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Company.Service.RestApi.Api;
using Flurl;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.InvoiceAddresses.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.InvoiceAddresses.V1;

public class GetInvoiceAddressesTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<GetInvoiceAddressesTestCase> Data =>
    [
        new()
        {
            Name = "Get all invoice addresses",
            Seed = [
                CreateInvoiceAddress(Guid.NewGuid(), "Home", "HomeStreet", "20/34"),
                CreateInvoiceAddress(Guid.NewGuid(), "Work", "WorkStreet", "10/17")
            ],
            Request = new(),
            Assert = (pagedResponse, seed) =>
            {
                pagedResponse.Items.Should().HaveCount(seed.Count);
                pagedResponse.TotalCount.Should().Be(seed.Count);
                pagedResponse.CurrentPage.Should().Be(1);
            }
        },
        GetInvoiceAddressesTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var homeAddress = CreateInvoiceAddress(tenantId, "Home", "HomeStreet", "20/34");
            var workAddress = CreateInvoiceAddress(tenantId, "Work", "WorkStreet", "10/17");
            var otherTenantAddress = CreateInvoiceAddress(Guid.NewGuid(), "Other", "OtherStreet", "1");

            return new()
            {
                Name = "Filter by tenant IDs",
                Seed = [homeAddress, workAddress, otherTenantAddress],
                Request = new()
                {
                    TenantIds = [tenantId]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(item => item.TenantId == tenantId);
                    pagedResponse.Items.Should().NotContain(item => item.TenantId != tenantId);
                }
            };
        }),
        GetInvoiceAddressesTestCase.CreateFromFactory(() =>
        {
            var address1 = CreateInvoiceAddress(Guid.NewGuid(), "Address 1", "Street1", "1");
            var address2 = CreateInvoiceAddress(Guid.NewGuid(), "Address 2", "Street2", "2");
            var address3 = CreateInvoiceAddress(Guid.NewGuid(), "Address 3", "Street3", "3");

            return new()
            {
                Name = "Filter by specific invoice address IDs",
                Seed = [address1, address2, address3],
                Request = new()
                {
                    InvoiceAddressIds = [address1.Id, address3.Id]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(item => item.Id == address1.Id);
                    pagedResponse.Items.Should().Contain(item => item.Id == address3.Id);
                    pagedResponse.Items.Should().NotContain(item => item.Id == address2.Id);
                }
            };
        }),
        GetInvoiceAddressesTestCase.CreateFromFactory(() =>
        {
            List<InvoiceAddress> addresses = [.. Enumerable.Range(1, 25)
                .Select(i => CreateInvoiceAddress(Guid.NewGuid(), $"Address {i}", $"Street{i}", i.ToString()))];

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                Seed = addresses,
                Request = new()
                {
                    PageNumber = 2,
                    PageSize = 10
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(10);
                    pagedResponse.CurrentPage.Should().Be(2);
                    pagedResponse.PageSize.Should().Be(10);
                    pagedResponse.TotalCount.Should().Be(25);
                }
            };
        }),
        new()
        {
            Name = "Empty result - no invoice addresses",
            Seed = [],
            Request = new(),
            Assert = (pagedResponse, seed) =>
            {
                pagedResponse.Items.Should().BeEmpty();
                pagedResponse.TotalCount.Should().Be(0);
                pagedResponse.CurrentPage.Should().Be(1);
            }
        }
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public async Task GetInvoiceAddresses_ReturnsExpectedResults(GetInvoiceAddressesTestCase testCase)
    {
        // Arrange
        DbContext.InvoiceAdresses.AddRange(testCase.Seed);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync("/api/v1/invoice-addresses".SetQueryParams(testCase.Request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.InvoiceAddress>>();
        pagedResponse.Should().NotBeNull();

        testCase.AssertResponse(pagedResponse, testCase.Seed);
    }

    [Fact]
    public async Task GetInvoiceAddresses_ReturnsFullContractWithAllPropertiesMappedCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var invoiceAddress = InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: "Primary Office",
            address: Address.CreateNew(
                country: "USA",
                city: "New York",
                zipCode: "10001",
                street: "Broadway",
                number: "350"
            ).Value!
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync("/api/v1/invoice-addresses");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.InvoiceAddress>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(1);

        var item = pagedResponse.Items[0];

        item.Id.Should().Be(invoiceAddress.Id);
        item.TenantId.Should().Be(invoiceAddress.TenantId);
        item.Name.Should().Be(invoiceAddress.Name);

        item.Address.Should().NotBeNull();
        item.Address.Country.Should().Be(invoiceAddress.Address.Country);
        item.Address.City.Should().Be(invoiceAddress.Address.City);
        item.Address.ZipCode.Should().Be(invoiceAddress.Address.ZipCode);
        item.Address.Street.Should().Be(invoiceAddress.Address.Street);
        item.Address.Number.Should().Be(invoiceAddress.Address.Number);
    }

    private static InvoiceAddress CreateInvoiceAddress(Guid tenantId, string name, string street, string number)
        => InvoiceAddress.CreateNew(
            tenantId: tenantId,
            name: name,
            address: Address.CreateNew(
                country: "TestCountry",
                city: "TestCity",
                zipCode: "TestZip",
                street: street,
                number: number
            ).Value!
        ).Value!;

    public class GetInvoiceAddressesTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> Seed { get; init; }

        public required V1Contracts.GetInvoiceAddressesRequest Request { get; init; }

        public required Action<PagedResponse<V1Contracts.InvoiceAddress>, List<InvoiceAddress>> Assert { private get; init; }

        public void AssertResponse(PagedResponse<V1Contracts.InvoiceAddress> response, List<InvoiceAddress> seed)
        {
            Assert(response, seed);
        }

        public static GetInvoiceAddressesTestCase CreateFromFactory(Func<GetInvoiceAddressesTestCase> factory) => factory();
    }
}