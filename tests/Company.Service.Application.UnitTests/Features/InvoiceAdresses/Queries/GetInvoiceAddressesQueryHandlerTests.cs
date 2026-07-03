//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Features.InvoiceAddresses.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.InvoiceAdresses.Queries;

public class GetInvoiceAddressesQueryHandlerTests : DbContextTestBase
{
    private readonly GetInvoiceAddressesQueryHandler _sut;

    public GetInvoiceAddressesQueryHandlerTests()
    {
        _sut = new GetInvoiceAddressesQueryHandler(DbContext);
    }

    public static TheoryData<InvoiceAdressesTestCase> Data =>
    [
        new()
        {
            Name = "Get all invoice addresses",
            Seed = [
                InvoiceAddress.CreateNew(
                    tenantId: Guid.NewGuid(),
                    name: "Home",
                    address: Address.CreateNew(
                        country: "TestCountry",
                        city: "TestCity",
                        zipCode: "TestZip",
                        street: "HomeStreet",
                        number: "20/34"
                    ).Value!
                ).Value!,
                InvoiceAddress.CreateNew(
                    tenantId: Guid.NewGuid(),
                    name: "Work",
                    address: Address.CreateNew(
                        country: "TestCountry",
                        city: "TestCity",
                        zipCode: "TestZip",
                        street: "WorkStreet",
                        number: "10/17"
                    ).Value!
                ).Value!
            ],
            Query = new(),
            Assertion = (result, seed) =>
            {
                result.Items.Should().BeEquivalentTo(seed);
            }
        },
        InvoiceAdressesTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();

            var homeAdress = InvoiceAddress.CreateNew(
                tenantId: tenantId,
                name: "Home",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "HomeStreet",
                    number: "20/34"
                ).Value!
            ).Value!;

            var workAdress = InvoiceAddress.CreateNew(
                tenantId: tenantId,
                name: "Work",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "WorkStreet",
                    number: "10/17"
                ).Value!
            ).Value!;

            return new()
            {
                Name = "Get all invoice addresses by tenantIds",
                Seed = [
                    homeAdress,
                    workAdress,
                    InvoiceAddress.CreateNew(
                        tenantId: Guid.NewGuid(),
                        name: "Other tenant Work",
                        address: Address.CreateNew(
                            country: "TestCountry",
                            city: "TestCity",
                            zipCode: "TestZip",
                            street: "WorkStreet",
                            number: "10/17"
                        ).Value!
                    ).Value!
                ],
                Query = new()
                {
                    TenantIds = [tenantId]
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo([homeAdress, workAdress]);
                }
            };
        }),
        InvoiceAdressesTestCase.CreateFromFactory(() =>
        {
            var address1 = InvoiceAddress.CreateNew(
                tenantId: Guid.NewGuid(),
                name: "Address 1",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "Street1",
                    number: "1"
                ).Value!
            ).Value!;

            var address2 = InvoiceAddress.CreateNew(
                tenantId: Guid.NewGuid(),
                name: "Address 2",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "Street2",
                    number: "2"
                ).Value!
            ).Value!;

            var address3 = InvoiceAddress.CreateNew(
                tenantId: Guid.NewGuid(),
                name: "Address 3",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "Street3",
                    number: "3"
                ).Value!
            ).Value!;

            return new()
            {
                Name = "Filter by specific invoice address IDs",
                Seed = [address1, address2, address3],
                Query = new()
                {
                    InvoiceAddressIds = [address1.Id, address3.Id]
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo([address1, address3]);
                    result.Items.Should().NotContain(address2);
                }
            };
        }),
        InvoiceAdressesTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();

            var address1 = InvoiceAddress.CreateNew(
                tenantId: tenantId,
                name: "Address 1",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "Street1",
                    number: "1"
                ).Value!
            ).Value!;

            var address2 = InvoiceAddress.CreateNew(
                tenantId: tenantId,
                name: "Address 2",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "Street2",
                    number: "2"
                ).Value!
            ).Value!;

            var otherTenantAddress = InvoiceAddress.CreateNew(
                tenantId: Guid.NewGuid(),
                name: "Other Tenant Address 1",
                address: Address.CreateNew(
                    country: "TestCountry",
                    city: "TestCity",
                    zipCode: "TestZip",
                    street: "OtherStreet",
                    number: "1"
                ).Value!
            ).Value!;

            return new()
            {
                Name = "Filter by tenant ID and specific invoice address IDs",
                Seed = [address1, address2, otherTenantAddress],
                Query = new()
                {
                    TenantIds = [tenantId],
                    InvoiceAddressIds = [address1.Id]
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().ContainEquivalentOf(address1);
                    result.Items.Should().NotContain(address2);
                    result.Items.Should().NotContain(otherTenantAddress);
                }
            };
        }),
        InvoiceAdressesTestCase.CreateFromFactory(() =>
        {
            var addresses = Enumerable.Range(1, 25)
                .Select(i => InvoiceAddress.CreateNew(
                    tenantId: Guid.NewGuid(),
                    name: $"Address {i}",
                    address: Address.CreateNew(
                        country: "TestCountry",
                        city: "TestCity",
                        zipCode: "TestZip",
                        street: $"Street{i}",
                        number: i.ToString()
                    ).Value!
                ).Value!)
                .ToList();

            return new()
            {
                Name = "Pagination - get first page with 10 items",
                Seed = addresses,
                Query = new()
                {
                    PageNumber = 1,
                    PageSize = 10
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(10);
                    result.CurrentPage.Should().Be(1);
                    result.PageSize.Should().Be(10);
                    result.TotalCount.Should().Be(25);
                }
            };
        }),
        InvoiceAdressesTestCase.CreateFromFactory(() =>
        {
            var addresses = Enumerable.Range(1, 25)
                .Select(i => InvoiceAddress.CreateNew(
                    tenantId: Guid.NewGuid(),
                    name: $"Address {i}",
                    address: Address.CreateNew(
                        country: "TestCountry",
                        city: "TestCity",
                        zipCode: "TestZip",
                        street: $"Street{i}",
                        number: i.ToString()
                    ).Value!
                ).Value!)
                .ToList();

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                Seed = addresses,
                Query = new()
                {
                    PageNumber = 2,
                    PageSize = 10
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(10);
                    result.CurrentPage.Should().Be(2);
                    result.PageSize.Should().Be(10);
                    result.TotalCount.Should().Be(25);
                }
            };
        }),
        InvoiceAdressesTestCase.CreateFromFactory(() =>
        {
            var addresses = Enumerable.Range(1, 25)
                .Select(i => InvoiceAddress.CreateNew(
                    tenantId: Guid.NewGuid(),
                    name: $"Address {i}",
                    address: Address.CreateNew(
                        country: "TestCountry",
                        city: "TestCity",
                        zipCode: "TestZip",
                        street: $"Street{i}",
                        number: i.ToString()
                    ).Value!
                ).Value!)
                .ToList();

            return new()
            {
                Name = "Pagination - get last page with remaining items",
                Seed = addresses,
                Query = new()
                {
                    PageNumber = 3,
                    PageSize = 10
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(5);
                    result.CurrentPage.Should().Be(3);
                    result.PageSize.Should().Be(10);
                    result.TotalCount.Should().Be(25);
                }
            };
        }),
        new()
        {
            Name = "Empty result - no invoice addresses",
            Seed = [],
            Query = new(),
            Assertion = (result, seed) =>
            {
                result.Items.Should().BeEmpty();
                result.TotalCount.Should().Be(0);
                result.CurrentPage.Should().Be(1);
            }
        },
        new()
        {
            Name = "Filter with no matching results",
            Seed = [
                InvoiceAddress.CreateNew(
                    tenantId: Guid.NewGuid(),
                    name: "Address 1",
                    address: Address.CreateNew(
                        country: "TestCountry",
                        city: "TestCity",
                        zipCode: "TestZip",
                        street: "Street1",
                        number: "1"
                    ).Value!
                ).Value!
            ],
            Query = new()
            {
                TenantIds = [Guid.NewGuid()]
            },
            Assertion = (result, seed) =>
            {
                result.Items.Should().BeEmpty();
                result.TotalCount.Should().Be(0);
            }
        }
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public async Task Handle_ReturnsInvoiceAdresses(InvoiceAdressesTestCase testCase)
    {
        // Arrange
        DbContext.InvoiceAdresses.AddRange(testCase.Seed);

        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var queryResult = await _sut.Handle(testCase.Query, default);

        // Assert
        queryResult.IsSuccess.Should().BeTrue();

        testCase.Assert(queryResult.Value!);
    }

    public class InvoiceAdressesTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> Seed { get; init; }

        public required GetInvoiceAddressesQuery Query { get; init; }

        public required Action<PagedList<InvoiceAddress>, List<InvoiceAddress>> Assertion { private get; init; }

        public void Assert(PagedList<InvoiceAddress> result)
        {
            Assertion(result, Seed);
        }

        public static InvoiceAdressesTestCase CreateFromFactory(Func<InvoiceAdressesTestCase> factory) => factory();
    }
}
//__EXAMPLE_END__
