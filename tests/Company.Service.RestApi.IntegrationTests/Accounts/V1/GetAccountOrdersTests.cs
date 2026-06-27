using AwesomeAssertions;
using Company.Service.Application.Common.Utils;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Company.Service.RestApi.Api;
using Flurl;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class GetAccountOrdersTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<GetAccountOrdersTestCase> Data =>
    [
        GetAccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (tenantId, invoiceAddress) = CreateInvoiceAddress(Guid.NewGuid());
            var order1 = CreateAccountOrder(invoiceAddress, tenantId, "Account 1");
            var order2 = CreateAccountOrder(invoiceAddress, tenantId, "Account 2");

            return new()
            {
                Name = "Get all account orders",
                InvoiceAddresses = [invoiceAddress],
                Seed = [order1, order2],
                Request = new(),
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(seed.Count);
                    pagedResponse.TotalCount.Should().Be(seed.Count);
                    pagedResponse.CurrentPage.Should().Be(1);
                }
            };
        }),
        GetAccountOrdersTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var (_, otherInvoiceAddress) = CreateInvoiceAddress(Guid.NewGuid());
            var order1 = CreateAccountOrder(invoiceAddress, tenantId, "Account 1");
            var order2 = CreateAccountOrder(invoiceAddress, tenantId, "Account 2");
            var otherTenantOrder = CreateAccountOrder(otherInvoiceAddress, Guid.NewGuid(), "Other Tenant");

            return new()
            {
                Name = "Filter by tenant IDs",
                InvoiceAddresses = [invoiceAddress, otherInvoiceAddress],
                Seed = [order1, order2, otherTenantOrder],
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
        GetAccountOrdersTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var order1 = CreateAccountOrder(invoiceAddress, tenantId, "Account 1");
            var order2 = CreateAccountOrder(invoiceAddress, tenantId, "Account 2");
            var order3 = CreateAccountOrder(invoiceAddress, tenantId, "Account 3");

            return new()
            {
                Name = "Filter by specific order IDs",
                InvoiceAddresses = [invoiceAddress],
                Seed = [order1, order2, order3],
                Request = new()
                {
                    OrderIds = [order1.Id, order3.Id]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(item => item.Id == order1.Id);
                    pagedResponse.Items.Should().Contain(item => item.Id == order3.Id);
                    pagedResponse.Items.Should().NotContain(item => item.Id == order2.Id);
                }
            };
        }),
        GetAccountOrdersTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var order1 = CreateAccountOrder(invoiceAddress, tenantId, "Account 1");
            var order2 = CreateAccountOrder(invoiceAddress, tenantId, "Account 2");
            var order3 = CreateAccountOrder(invoiceAddress, tenantId, "Account 3");

            return new()
            {
                Name = "Filter by statuses",
                InvoiceAddresses = [invoiceAddress],
                Seed = [order1, order2, order3],
                Request = new()
                {
                    Statuses = [V1Contracts.AccountOrderStatus.Pending]
                },
                Assert = (pagedResponse, seed) =>
                {
                    // All seeded orders are Pending, so all 3 match
                    pagedResponse.Items.Should().HaveCount(3);
                    pagedResponse.Items.Should().AllSatisfy(item =>
                        item.Status.Should().Be(V1Contracts.AccountOrderStatus.Pending)
                    );
                }
            };
        }),
        GetAccountOrdersTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            List<AccountOrder> orders = [.. Enumerable.Range(1, 25)
                .Select(i => CreateAccountOrder(invoiceAddress, tenantId, $"Account {i}"))];

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Seed = orders,
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
        GetAccountOrdersTestCase.CreateFromFactory(() =>
        {
            return new()
            {
                Name = "Empty result - no account orders",
                InvoiceAddresses = [],
                Seed = [],
                Request = new(),
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().BeEmpty();
                    pagedResponse.TotalCount.Should().Be(0);
                    pagedResponse.CurrentPage.Should().Be(1);
                }
            };
        })
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public async Task GetAccountOrders_ReturnsExpectedResults(GetAccountOrdersTestCase testCase)
    {
        // Arrange — seed InvoiceAddresses first (FK constraint), then AccountOrders
        DbContext.InvoiceAdresses.AddRange(testCase.InvoiceAddresses);
        DbContext.AccountOrders.AddRange(testCase.Seed);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync("/api/v1/accounts/orders".SetQueryParams(testCase.Request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.AccountOrder>>();
        pagedResponse.Should().NotBeNull();
        
        testCase.AssertResponse(pagedResponse, testCase.Seed);
    }

    [Fact]
    public async Task GetAccountOrders_ReturnsFullContractWithAllPropertiesMappedCorrectly()
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

        FakeTimeProvider.SetUtcNow(new DateTimeOffset(2026, 6, 27, 12, 0, 0, TimeSpan.Zero));
        var createdDate = FakeTimeProvider.GetUtcNowDateTime();

        var order = AccountOrder.CreateNew(
            tenantId: tenantId,
            accountDetails: AccountDetails.CreateNew(
                name: "Acme Corp",
                email: "acme@example.com",
                tier: AccountTier.Business,
                invoiceAdressId: invoiceAddress.Id
            ).Value!,
            contactInformation: ContactInformation.CreateNew(
                firstName: "John",
                lastName: "Doe",
                email: "john@example.com",
                phoneNumber: "+1234567890"
            ).Value!,
            createdDate: createdDate
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.AccountOrders.Add(order);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync("/api/v1/accounts/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.AccountOrder>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(1);

        var item = pagedResponse.Items[0];

        item.Id.Should().Be(order.Id);
        item.TenantId.Should().Be(order.TenantId);
        item.AccountId.Should().BeNull();

        // AccountDetails
        item.AccountDetails.Should().NotBeNull();
        item.AccountDetails.Name.Should().Be(order.AccountDetails.Name);
        item.AccountDetails.Email.Should().Be(order.AccountDetails.Email);
        item.AccountDetails.Tier.Should().Be(V1Contracts.AccountTier.Business);
        item.AccountDetails.InvoiceAddressId.Should().Be(order.AccountDetails.InvoiceAddressId);

        // ContactInformation
        item.ContactInformation.Should().NotBeNull();
        item.ContactInformation.FirstName.Should().Be(order.ContactInformation.FirstName);
        item.ContactInformation.LastName.Should().Be(order.ContactInformation.LastName);
        item.ContactInformation.Email.Should().Be(order.ContactInformation.Email);
        item.ContactInformation.PhoneNumber.Should().Be(order.ContactInformation.PhoneNumber);

        // Status and CreatedDate
        item.Status.Should().Be(V1Contracts.AccountOrderStatus.Pending);
        item.CreatedDate.Should().Be(createdDate);
    }

    private static AccountOrder CreateAccountOrder(InvoiceAddress invoiceAddress, Guid tenantId, string accountName)
        => AccountOrder.CreateNew(
            tenantId: tenantId,
            accountDetails: AccountDetails.CreateNew(
                name: accountName,
                email: $"{accountName.ToLower().Replace(" ", "")}@example.com",
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

    private static (Guid TenantId, InvoiceAddress InvoiceAddress) CreateInvoiceAddress(Guid tenantId)
    {
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

        return (tenantId, invoiceAddress);
    }

    public class GetAccountOrdersTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> InvoiceAddresses { get; init; }

        public required List<AccountOrder> Seed { get; init; }

        public required V1Contracts.GetAccountOrdersRequest Request { get; init; }

        public required Action<PagedResponse<V1Contracts.AccountOrder>, List<AccountOrder>> Assert { private get; init; }

        public void AssertResponse(PagedResponse<V1Contracts.AccountOrder> response, List<AccountOrder> seed)
        {
            Assert(response, seed);
        }

        public static GetAccountOrdersTestCase CreateFromFactory(Func<GetAccountOrdersTestCase> factory) => factory();
    }
}
