using AwesomeAssertions;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Features.Accounts.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Accounts.Queries;

public class GetAccountOrdersQueryHandlerTests : DbContextTestBase
{
    private readonly GetAccountOrdersQueryHandler _sut;

    public GetAccountOrdersQueryHandlerTests()
    {
        _sut = new(DbContext);
    }

    public static TheoryData<AccountOrdersTestCase> Data =>
    [
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(2);

            return new()
            {
                Name = "Get all account orders",
                InvoiceAddresses = [invAddr],
                Seed = orders,
                Query = new(),
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo(seed);
                }
            };
        }),
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();

            var (invAddr1, order1) = CreateOrder(tenantId, "Account 1");
            var (invAddr2, order2) = CreateOrder(tenantId, "Account 2");
            var (invAddr3, otherOrder) = CreateOrder(Guid.NewGuid(), "Other Tenant Account");

            return new()
            {
                Name = "Filter by tenant IDs",
                InvoiceAddresses = [invAddr1, invAddr2, invAddr3],
                Seed = [order1, order2, otherOrder],
                Query = new() { TenantIds = [tenantId] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo([order1, order2]);
                }
            };
        }),
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(3);
            var ids = new[] { orders[0].Id, orders[2].Id };

            return new()
            {
                Name = "Filter by specific order IDs",
                InvoiceAddresses = [invAddr],
                Seed = orders,
                Query = new() { OrderIds = ids },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().BeEquivalentTo([orders[0], orders[2]]);
                    result.Items.Should().NotContain(orders[1]);
                }
            };
        }),
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, orders) = CreateOrders(3);

            // Complete the first order to get an AccountId
            var account = Account.CreateNew(
                orders[0].TenantId,
                orders[0].AccountDetails.Name,
                orders[0].AccountDetails.Email,
                orders[0].AccountDetails.Tier,
                orders[0].AccountDetails.InvoiceAddressId
            ).Value!;
            orders[0].StartProcessing();
            orders[0].Complete(account, DateTime.UtcNow);

            return new()
            {
                Name = "Filter by account IDs",
                InvoiceAddresses = [invoiceAddress],
                Seed = orders,
                // Store the account so it can be seeded too
                AdditionalSeed = (dbContext) =>
                {
                    dbContext.Accounts.Add(account);
                },
                Query = new() { AccountIds = [orders[0].AccountId!.Value] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(o => o.Id == orders[0].Id);
                }
            };
        }),
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(3);

            // Start processing the first order
            orders[0].StartProcessing();

            return new()
            {
                Name = "Filter by status",
                InvoiceAddresses = [invAddr],
                Seed = orders,
                Query = new() { Statuses = [AccountOrderStatus.Pending] },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(o => o.Id == orders[1].Id);
                    result.Items.Should().Contain(o => o.Id == orders[2].Id);
                    result.Items.Should().NotContain(o => o.Id == orders[0].Id);
                }
            };
        }),
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var tenantId = Guid.NewGuid();
            var (invAddr, orders) = CreateOrders(5, tenantId);
            var ids = new[] { orders[0].Id, orders[2].Id };

            return new()
            {
                Name = "Filter by tenant ID and order IDs combined",
                InvoiceAddresses = [invAddr],
                Seed = orders,
                Query = new()
                {
                    TenantIds = [tenantId],
                    OrderIds = ids
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(o => o.Id == orders[0].Id);
                    result.Items.Should().Contain(o => o.Id == orders[2].Id);
                }
            };
        }),
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(25);

            return new()
            {
                Name = "Pagination - get first page with 10 items",
                InvoiceAddresses = [invAddr],
                Seed = orders,
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
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(25);

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                InvoiceAddresses = [invAddr],
                Seed = orders,
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
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(25);

            return new()
            {
                Name = "Pagination - get last page with remaining items",
                InvoiceAddresses = [invAddr],
                Seed = orders,
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
            Name = "Empty result - no account orders",
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
        AccountOrdersTestCase.CreateFromFactory(() =>
        {
            var (invAddr, orders) = CreateOrders(1);

            return new()
            {
                Name = "Filter with no matching results",
                InvoiceAddresses = [invAddr],
                Seed = orders,
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
    public async Task Handle_ReturnsAccountOrders(AccountOrdersTestCase testCase)
    {
        // Arrange
        DbContext.InvoiceAdresses.AddRange(testCase.InvoiceAddresses);
        DbContext.AccountOrders.AddRange(testCase.Seed);

        testCase.AdditionalSeed?.Invoke(DbContext);

        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var queryResult = await _sut.Handle(testCase.Query, default);

        // Assert
        queryResult.IsSuccess.Should().BeTrue();

        testCase.Assert(queryResult.Value!);
    }

    private static (InvoiceAddress InvoiceAddress, List<AccountOrder> Orders) CreateOrders(int count, Guid? tenantId = null)
    {
        var invoiceAddress = CreateInvoiceAddress(tenantId ?? Guid.NewGuid());

        var orders = Enumerable.Range(1, count)
            .Select(i => AccountOrder.CreateNew(
                tenantId: tenantId ?? Guid.NewGuid(),
                accountDetails: AccountDetails.CreateNew(
                    name: $"Account {i}",
                    email: $"account{i}@example.com",
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
            ).Value!)
            .ToList();

        return (invoiceAddress, orders);
    }

    private static (InvoiceAddress InvoiceAddress, AccountOrder Order) CreateOrder(Guid tenantId, string accountName)
    {
        var invoiceAddress = CreateInvoiceAddress(tenantId);

        var accountOrder = AccountOrder.CreateNew(
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

        return (invoiceAddress, accountOrder);
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

    public class AccountOrdersTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> InvoiceAddresses { get; init; }

        public required List<AccountOrder> Seed { get; init; }

        public required GetAccountOrdersQuery Query { get; init; }

        public Action<TestDbContext>? AdditionalSeed { get; init; }

        public required Action<PagedList<AccountOrder>, List<AccountOrder>> Assertion { private get; init; }

        public void Assert(PagedList<AccountOrder> result)
        {
            Assertion(result, Seed);
        }

        public static AccountOrdersTestCase CreateFromFactory(Func<AccountOrdersTestCase> factory) => factory();
    }
}
