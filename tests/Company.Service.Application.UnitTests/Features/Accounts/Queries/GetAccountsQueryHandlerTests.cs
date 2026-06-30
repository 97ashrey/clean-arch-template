using AwesomeAssertions;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Features.Accounts.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Accounts.Queries;

public class GetAccountsQueryHandlerTests : DbContextTestBase
{
    private readonly GetAccountsQueryHandler _sut;

    public GetAccountsQueryHandlerTests()
    {
        _sut = new(DbContext);
    }

    public static TheoryData<GetAccountsTestCase> Data =>
    [
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(2);

            return new()
            {
                Name = "Get all accounts",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new(),
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo(seed);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (invAddr1, account1) = CreateAccount(tenantId, "Account 1", "acc1@example.com");
            var (invAddr2, account2) = CreateAccount(tenantId, "Account 2", "acc2@example.com");
            var (invAddr3, otherAccount) = CreateAccount(Guid.NewGuid(), "Other Tenant", "other@example.com");

            return new()
            {
                Name = "Filter by tenant IDs",
                InvoiceAddresses = [invAddr1, invAddr2, invAddr3],
                Seed = [account1, account2, otherAccount],
                Query = new() { TenantIds = [tenantId] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo([account1, account2]);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(3);
            var ids = new[] { accounts[0].Id, accounts[2].Id };

            return new()
            {
                Name = "Filter by account IDs",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { Ids = ids },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo([accounts[0], accounts[2]]);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccountsWithTiers(
                AccountTier.Individual, AccountTier.Business, AccountTier.Enterprise);

            return new()
            {
                Name = "Filter by account tiers",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { AccountTiers = [AccountTier.Business, AccountTier.Enterprise] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(a => a.Tier == AccountTier.Business);
                    result.Items.Should().Contain(a => a.Tier == AccountTier.Enterprise);
                    result.Items.Should().NotContain(a => a.Tier == AccountTier.Individual);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccountsWithStatuses(
                AccountStatus.Active, AccountStatus.Suspended, AccountStatus.Removed);

            // Suspend and remove the appropriate accounts
            accounts[1].Suspend(DateTime.UtcNow);
            accounts[2].Suspend(DateTime.UtcNow);
            accounts[2].Remove();

            return new()
            {
                Name = "Filter by account statuses",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { AccountStatuses = [AccountStatus.Active, AccountStatus.Removed] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(a => a.Status == AccountStatus.Active);
                    result.Items.Should().Contain(a => a.Status == AccountStatus.Removed);
                    result.Items.Should().NotContain(a => a.Status == AccountStatus.Suspended);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(3);

            return new()
            {
                Name = "Search by name",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { SearchTerm = accounts[1].Name },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(a => a.Id == accounts[1].Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(3);

            return new()
            {
                Name = "Search by email",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { SearchTerm = accounts[2].Email },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(a => a.Id == accounts[2].Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(3);

            return new()
            {
                Name = "Search by Id (Guid string)",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { SearchTerm = accounts[0].Id.ToString() },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(a => a.Id == accounts[0].Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(3);

            return new()
            {
                Name = "Search by partial name (contains match)",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { SearchTerm = "Account" },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(3);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(3);

            return new()
            {
                Name = "Search by partial email domain",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { SearchTerm = "example.com" },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(3);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (invoiceAddress, accounts) = CreateAccounts(5, tenantId);
            var ids = new[] { accounts[0].Id, accounts[2].Id };

            return new()
            {
                Name = "Filter by tenant ID and account IDs combined",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new()
                {
                    TenantIds = [tenantId],
                    Ids = ids
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(a => a.Id == accounts[0].Id);
                    result.Items.Should().Contain(a => a.Id == accounts[2].Id);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(25);

            return new()
            {
                Name = "Pagination - get first page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { PageNumber = 1, PageSize = 10 },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(10);
                    result.CurrentPage.Should().Be(1);
                    result.PageSize.Should().Be(10);
                    result.TotalCount.Should().Be(25);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(25);

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { PageNumber = 2, PageSize = 10 },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(10);
                    result.CurrentPage.Should().Be(2);
                    result.PageSize.Should().Be(10);
                    result.TotalCount.Should().Be(25);
                }
            };
        }),
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(25);

            return new()
            {
                Name = "Pagination - get last page with remaining items",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { PageNumber = 3, PageSize = 10 },
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
            Name = "Empty result - no accounts",
            InvoiceAddresses = [],
            Seed = [],
            Query = new(),
            Assertion = (result, seed) =>
            {
                result.Items.Should().BeEmpty();
                result.TotalCount.Should().Be(0);
                result.CurrentPage.Should().Be(1);
            }
        },
        GetAccountsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, accounts) = CreateAccounts(1);

            return new()
            {
                Name = "Filter with no matching results",
                InvoiceAddresses = [invoiceAddress],
                Seed = accounts,
                Query = new() { TenantIds = [Guid.NewGuid()] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEmpty();
                    result.TotalCount.Should().Be(0);
                }
            };
        })
    ];

    [Theory]
    [MemberData(nameof(Data))]
    public async Task Handle_ReturnsAccounts(GetAccountsTestCase testCase)
    {
        // Arrange
        DbContext.InvoiceAdresses.AddRange(testCase.InvoiceAddresses);
        DbContext.Accounts.AddRange(testCase.Seed);

        testCase.AdditionalSeed?.Invoke(DbContext);

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var queryResult = await _sut.Handle(testCase.Query, default);

        // Assert
        queryResult.IsSuccess.Should().BeTrue();

        testCase.Assert(queryResult.Value!);
    }

    private static (InvoiceAddress InvoiceAddress, List<Account> Accounts) CreateAccounts(int count, Guid? tenantId = null)
    {
        var invoiceAddress = CreateInvoiceAddress(tenantId ?? Guid.NewGuid());

        var accounts = Enumerable.Range(1, count)
            .Select(i => Account.CreateNew(
                tenantId: tenantId ?? Guid.NewGuid(),
                name: $"Account {i}",
                email: $"account{i}@example.com",
                tier: AccountTier.Business,
                invoiceAddressId: invoiceAddress.Id
            ).Value!)
            .ToList();

        return (invoiceAddress, accounts);
    }

    private static (InvoiceAddress InvoiceAddress, Account Account) CreateAccount(Guid tenantId, string name, string email)
    {
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: name,
            email: email,
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        return (invoiceAddress, account);
    }

    private static (InvoiceAddress InvoiceAddress, List<Account> Accounts) CreateAccountsWithTiers(
        AccountTier tier1, AccountTier tier2, AccountTier tier3)
    {
        var tenantId = Guid.NewGuid();
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var accounts = new[]
        {
            Account.CreateNew(tenantId, $"Account {tier1}", $"{tier1}@example.com", tier1, invoiceAddress.Id).Value!,
            Account.CreateNew(tenantId, $"Account {tier2}", $"{tier2}@example.com", tier2, invoiceAddress.Id).Value!,
            Account.CreateNew(tenantId, $"Account {tier3}", $"{tier3}@example.com", tier3, invoiceAddress.Id).Value!,
        }.ToList();

        return (invoiceAddress, accounts);
    }

    private static (InvoiceAddress InvoiceAddress, List<Account> Accounts) CreateAccountsWithStatuses(
        AccountStatus status1, AccountStatus status2, AccountStatus status3)
    {
        var tenantId = Guid.NewGuid();
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var accounts = new[]
        {
            Account.CreateNew(tenantId, "Account Active", "active@example.com", AccountTier.Business, invoiceAddress.Id).Value!,
            Account.CreateNew(tenantId, "Account Suspended", "suspended@example.com", AccountTier.Business, invoiceAddress.Id).Value!,
            Account.CreateNew(tenantId, "Account Removed", "removed@example.com", AccountTier.Business, invoiceAddress.Id).Value!,
        }.ToList();

        return (invoiceAddress, accounts);
    }

    private static InvoiceAddress CreateInvoiceAddress(Guid tenantId)
    {
        return InvoiceAddress.CreateNew(
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
    }

    public class GetAccountsTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> InvoiceAddresses { get; init; }

        public required List<Account> Seed { get; init; }

        public required GetAccountsQuery Query { get; init; }

        public Action<TestDbContext>? AdditionalSeed { get; init; }

        public required Action<PagedList<Account>, List<Account>> Assertion { private get; init; }

        public void Assert(PagedList<Account> result)
        {
            Assertion(result, Seed);
        }

        public static GetAccountsTestCase CreateFromFactory(Func<GetAccountsTestCase> factory) => factory();
    }
}