using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Company.Service.RestApi.Api;
using Flurl;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Accounts.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Accounts.V1;

public class GetAccountsTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<GetAccountsTestCase> Data =>
    [
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (tenantId, invoiceAddress) = CreateInvoiceAddress(Guid.NewGuid());
            var account1 = CreateAccount(invoiceAddress, tenantId, "Account 1", "acc1@example.com");
            var account2 = CreateAccount(invoiceAddress, tenantId, "Account 2", "acc2@example.com");

            return new()
            {
                Name = "Get all accounts",
                InvoiceAddresses = [invoiceAddress],
                Seed = [account1, account2],
                Request = new(),
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(seed.Count);
                    pagedResponse.TotalCount.Should().Be(seed.Count);
                    pagedResponse.CurrentPage.Should().Be(1);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var (_, otherInvoiceAddress) = CreateInvoiceAddress(Guid.NewGuid());
            var account1 = CreateAccount(invoiceAddress, tenantId, "Account 1", "acc1@example.com");
            var account2 = CreateAccount(invoiceAddress, tenantId, "Account 2", "acc2@example.com");
            var otherTenantAccount = CreateAccount(otherInvoiceAddress, Guid.NewGuid(), "Other Tenant", "other@example.com");

            return new()
            {
                Name = "Filter by tenant IDs",
                InvoiceAddresses = [invoiceAddress, otherInvoiceAddress],
                Seed = [account1, account2, otherTenantAccount],
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
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var account1 = CreateAccount(invoiceAddress, tenantId, "Account 1", "acc1@example.com");
            var account2 = CreateAccount(invoiceAddress, tenantId, "Account 2", "acc2@example.com");
            var account3 = CreateAccount(invoiceAddress, tenantId, "Account 3", "acc3@example.com");

            return new()
            {
                Name = "Filter by account IDs",
                InvoiceAddresses = [invoiceAddress],
                Seed = [account1, account2, account3],
                Request = new()
                {
                    Ids = [account1.Id, account3.Id]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(item => item.Id == account1.Id);
                    pagedResponse.Items.Should().Contain(item => item.Id == account3.Id);
                    pagedResponse.Items.Should().NotContain(item => item.Id == account2.Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var account1 = CreateAccount(invoiceAddress, tenantId, "Individual Account", "individual@example.com", AccountTier.Individual);
            var account2 = CreateAccount(invoiceAddress, tenantId, "Business Account", "business@example.com", AccountTier.Business);
            var account3 = CreateAccount(invoiceAddress, tenantId, "Enterprise Account", "enterprise@example.com", AccountTier.Enterprise);

            return new()
            {
                Name = "Filter by account tiers",
                InvoiceAddresses = [invoiceAddress],
                Seed = [account1, account2, account3],
                Request = new()
                {
                    AccountTiers = [V1Contracts.AccountTier.Business, V1Contracts.AccountTier.Enterprise]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(item => item.Tier == V1Contracts.AccountTier.Business);
                    pagedResponse.Items.Should().Contain(item => item.Tier == V1Contracts.AccountTier.Enterprise);
                    pagedResponse.Items.Should().NotContain(item => item.Tier == V1Contracts.AccountTier.Individual);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var active = CreateAccount(invoiceAddress, tenantId, "Active Account", "active@example.com");
            var suspended = CreateAccount(invoiceAddress, tenantId, "Suspended Account", "suspended@example.com");
            var removed = CreateAccount(invoiceAddress, tenantId, "Removed Account", "removed@example.com");

            suspended.Suspend(DateTime.UtcNow);
            removed.Suspend(DateTime.UtcNow);
            removed.Remove();

            return new()
            {
                Name = "Filter by account statuses",
                InvoiceAddresses = [invoiceAddress],
                Seed = [active, suspended, removed],
                Request = new()
                {
                    AccountStatuses = [V1Contracts.AccountStatus.Active, V1Contracts.AccountStatus.Removed]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(item => item.Status == V1Contracts.AccountStatus.Active);
                    pagedResponse.Items.Should().Contain(item => item.Status == V1Contracts.AccountStatus.Removed);
                    pagedResponse.Items.Should().NotContain(item => item.Status == V1Contracts.AccountStatus.Suspended);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var account1 = CreateAccount(invoiceAddress, tenantId, "Alpha Corp", "alpha@example.com");
            var account2 = CreateAccount(invoiceAddress, tenantId, "Beta Inc", "beta@example.com");
            var account3 = CreateAccount(invoiceAddress, tenantId, "Gamma Ltd", "gamma@example.com");

            return new()
            {
                Name = "Search by name",
                InvoiceAddresses = [invoiceAddress],
                Seed = [account1, account2, account3],
                Request = new()
                {
                    SearchTerm = "Beta"
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(1);
                    pagedResponse.Items.Should().Contain(item => item.Id == account2.Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var account1 = CreateAccount(invoiceAddress, tenantId, "Alpha Corp", "alpha@example.com");
            var account2 = CreateAccount(invoiceAddress, tenantId, "Beta Inc", "beta@test.com");

            return new()
            {
                Name = "Search by email",
                InvoiceAddresses = [invoiceAddress],
                Seed = [account1, account2],
                Request = new()
                {
                    SearchTerm = "beta@test.com"
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(1);
                    pagedResponse.Items.Should().Contain(item => item.Id == account2.Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            var account = CreateAccount(invoiceAddress, tenantId, "Target Account", "target@example.com");
            var otherAccount = CreateAccount(invoiceAddress, tenantId, "Other Account", "other@example.com");

            return new()
            {
                Name = "Search by Id (Guid string)",
                InvoiceAddresses = [invoiceAddress],
                Seed = [account, otherAccount],
                Request = new()
                {
                    SearchTerm = account.Id.ToString()
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(1);
                    pagedResponse.Items.Should().Contain(item => item.Id == account.Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (_, invoiceAddress) = CreateInvoiceAddress(tenantId);
            List<Account> accounts = [.. Enumerable.Range(1, 25)
                .Select(i => CreateAccount(invoiceAddress, tenantId, $"Account {i}", $"account{i}@example.com"))];

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
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
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            return new()
            {
                Name = "Empty result - no accounts",
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
    public async Task GetAccounts_ReturnsExpectedResults(GetAccountsTestCase testCase)
    {
        // Arrange — seed InvoiceAddresses first (FK constraint), then Accounts
        DbContext.InvoiceAdresses.AddRange(testCase.InvoiceAddresses);
        DbContext.Accounts.AddRange(testCase.Seed);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync("/api/v1/accounts".SetQueryParams(testCase.Request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.Account>>();
        pagedResponse.Should().NotBeNull();

        testCase.AssertResponse(pagedResponse, testCase.Seed);
    }

    [Fact]
    public async Task GetAccounts_ReturnsFullContractWithAllPropertiesMappedCorrectly()
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

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Acme Corp",
            email: "acme@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync("/api/v1/accounts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.Account>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(1);

        var item = pagedResponse.Items[0];

        item.Id.Should().Be(account.Id);
        item.TenantId.Should().Be(account.TenantId);
        item.Name.Should().Be(account.Name);
        item.Email.Should().Be(account.Email);
        item.Tier.Should().Be((V1Contracts.AccountTier)account.Tier);
        item.Status.Should().Be((V1Contracts.AccountStatus)account.Status);
        item.SuspendedDate.Should().BeNull();
        item.InvoiceAddressId.Should().Be(account.InvoiceAddressId);
    }

    private static Account CreateAccount(InvoiceAddress invoiceAddress, Guid tenantId, string name, string email, AccountTier? tier = null)
        => Account.CreateNew(
            tenantId: tenantId,
            name: name,
            email: email,
            tier: tier ?? AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
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

    public class GetAccountsTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> InvoiceAddresses { get; init; }

        public required List<Account> Seed { get; init; }

        public required V1Contracts.GetAccountsRequest Request { get; init; }

        public required Action<PagedResponse<V1Contracts.Account>, List<Account>> Assert { private get; init; }

        public void AssertResponse(PagedResponse<V1Contracts.Account> response, List<Account> seed)
        {
            Assert(response, seed);
        }

        public static GetAccountsTestCase CreateFromFactory(Func<GetAccountsTestCase> factory) => factory();
    }
}