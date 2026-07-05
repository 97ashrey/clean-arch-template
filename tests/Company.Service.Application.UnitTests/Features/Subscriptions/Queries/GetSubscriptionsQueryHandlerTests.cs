using AwesomeAssertions;
using Company.Service.Application.Common.Types;
using Company.Service.Application.Features.Subscriptions.Queries;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Queries;

public class GetSubscriptionsQueryHandlerTests : DbContextTestBase
{
    private readonly GetSubscriptionsQueryHandler _sut;

    public GetSubscriptionsQueryHandlerTests()
    {
        _sut = new GetSubscriptionsQueryHandler(DbContext);
    }

    public static TheoryData<SubscriptionsTestCase> Data =>
    [
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "Premium", "Premium Sub", BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(accountId, "Basic", "Basic Sub", BillCycle.Yearly, productId2);

            return new()
            {
                Name = "Get all subscriptions",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2],
                Query = new() { AccountId = accountId },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(i => i.Id == s1.Id);
                    result.Items.Should().Contain(i => i.Id == s2.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "A", "Sub A", BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(accountId, "B", "Sub B", BillCycle.Yearly, productId2);
            var s3 = CreateSubscription(accountId, "C", "Sub C", BillCycle.Monthly, productId3);

            return new()
            {
                Name = "Filter by IDs",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Query = new()
                {
                    AccountId = accountId,
                    Ids = [s1.Id, s3.Id]
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(i => i.Id == s1.Id);
                    result.Items.Should().Contain(i => i.Id == s3.Id);
                    result.Items.Should().NotContain(i => i.Id == s2.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "Premium Plan", "Premium Sub", BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(accountId, "Basic Plan", "Basic Sub", BillCycle.Yearly, productId2);
            var s3 = CreateSubscription(accountId, "Gold Plan", "Gold Sub", BillCycle.Monthly, productId3);

            return new()
            {
                Name = "Filter by search term - name",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Query = new()
                {
                    AccountId = accountId,
                    SearchTerm = "Premium"
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(i => i.Id == s1.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "Alpha", "Premium Sub", BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(accountId, "Beta", "Basic Sub", BillCycle.Yearly, productId2);

            return new()
            {
                Name = "Filter by search term - friendly name",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2],
                Query = new()
                {
                    AccountId = accountId,
                    SearchTerm = "Premium"
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(i => i.Id == s1.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productIdTarget = Guid.NewGuid();
            var productIdOther = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "A", "Sub A", BillCycle.Monthly, productIdTarget);
            var s2 = CreateSubscription(accountId, "B", "Sub B", BillCycle.Yearly, productIdOther);

            return new()
            {
                Name = "Filter by search term - product ID",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2],
                Query = new()
                {
                    AccountId = accountId,
                    SearchTerm = productIdTarget.ToString()
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(i => i.Id == s1.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "Monthly A", "Monthly Sub", BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(accountId, "Yearly B", "Yearly Sub", BillCycle.Yearly, productId2);
            var s3 = CreateSubscription(accountId, "Monthly C", "Monthly Sub C", BillCycle.Monthly, productId3);

            return new()
            {
                Name = "Filter by bill cycles",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Query = new()
                {
                    AccountId = accountId,
                    BillCycles = [BillCycle.Monthly]
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(2);
                    result.Items.Should().Contain(i => i.Id == s1.Id);
                    result.Items.Should().Contain(i => i.Id == s3.Id);
                    result.Items.Should().NotContain(i => i.Id == s2.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "Active A", "Active Sub", BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(accountId, "Suspended B", "Suspended Sub", BillCycle.Monthly, productId2);
            var s3 = CreateSubscription(accountId, "Canceled C", "Canceled Sub", BillCycle.Monthly, productId3);

            s2.Suspend(new DateTime(2026, 6, 1));
            s3.Suspend(new DateTime(2026, 6, 1));
            s3.Cancel();

            return new()
            {
                Name = "Filter by statuses",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Query = new()
                {
                    AccountId = accountId,
                    Statuses = [Domain.Entities.SubscriptionStatus.Suspended]
                },
                Assertion = (result, seed) =>
                {
                    result.Items.Should().HaveCount(1);
                    result.Items.Should().Contain(i => i.Id == s2.Id);
                    result.Items.Should().NotContain(i => i.Id == s1.Id);
                    result.Items.Should().NotContain(i => i.Id == s3.Id);
                }
            };
        }),
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var subscriptions = Enumerable.Range(1, 25)
                .Select(i => CreateSubscription(
                    accountId,
                    $"Subscription {i}",
                    $"Sub {i}",
                    i % 2 == 0 ? BillCycle.Monthly : BillCycle.Yearly,
                    Guid.NewGuid()))
                .ToList();

            return new()
            {
                Name = "Pagination - get first page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = subscriptions,
                Query = new()
                {
                    AccountId = accountId,
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
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var subscriptions = Enumerable.Range(1, 25)
                .Select(i => CreateSubscription(
                    accountId,
                    $"Subscription {i}",
                    $"Sub {i}",
                    i % 2 == 0 ? BillCycle.Monthly : BillCycle.Yearly,
                    Guid.NewGuid()))
                .ToList();

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = subscriptions,
                Query = new()
                {
                    AccountId = accountId,
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
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var subscriptions = Enumerable.Range(1, 25)
                .Select(i => CreateSubscription(
                    accountId,
                    $"Subscription {i}",
                    $"Sub {i}",
                    i % 2 == 0 ? BillCycle.Monthly : BillCycle.Yearly,
                    Guid.NewGuid()))
                .ToList();

            return new()
            {
                Name = "Pagination - get last page with remaining items",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = subscriptions,
                Query = new()
                {
                    AccountId = accountId,
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
            Name = "Empty result - no subscriptions",
            InvoiceAddresses = [],
            Accounts = [],
            Seed = [],
            Query = new() { AccountId = Guid.NewGuid() },
            Assertion = (result, seed) =>
            {
                result.Items.Should().BeEmpty();
                result.TotalCount.Should().Be(0);
                result.CurrentPage.Should().Be(1);
            }
        },
        SubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (accountId, invoiceAddress, account) = CreateHierarchy();
            var productId = Guid.NewGuid();
            var s1 = CreateSubscription(accountId, "Only One", "Only Sub", BillCycle.Monthly, productId);

            return new()
            {
                Name = "Filter with no matching results",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1],
                Query = new()
                {
                    AccountId = accountId,
                    Ids = [Guid.NewGuid()]
                },
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
    public async Task Handle_ReturnsSubscriptions(SubscriptionsTestCase testCase)
    {
        // Arrange — seed InvoiceAddresses first (FK constraint), then Accounts, then Subscriptions
        DbContext.InvoiceAdresses.AddRange(testCase.InvoiceAddresses);
        DbContext.Accounts.AddRange(testCase.Accounts);
        DbContext.Subscriptions.AddRange(testCase.Seed);
        await DbContext.SaveChangesAsync();
        DbContext.ChangeTracker.Clear();

        // Act
        var queryResult = await _sut.Handle(testCase.Query, default);

        // Assert
        queryResult.IsSuccess.Should().BeTrue();

        testCase.Assert(queryResult.Value!);
    }

    private static Subscription CreateSubscription(Guid accountId, string name, string friendlyName, BillCycle billCycle, Guid productId)
    {
        return Subscription.CreateNew(
            accountId: accountId,
            name: name,
            friendlyName: friendlyName,
            purchasePrice: Price.CreateNew(19.99m, "USD").Value!,
            billCycle: billCycle,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;
    }

    private static (Guid AccountId, InvoiceAddress InvoiceAddress, Account Account) CreateHierarchy()
    {
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
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        return (account.Id, invoiceAddress, account);
    }

    public class SubscriptionsTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> InvoiceAddresses { get; init; }

        public required List<Account> Accounts { get; init; }

        public required List<Subscription> Seed { get; init; }

        public required GetSubscriptionsQuery Query { get; init; }

        public required Action<PagedList<Subscription>, List<Subscription>> Assertion { private get; init; }

        public void Assert(PagedList<Subscription> result)
        {
            Assertion(result, Seed);
        }

        public static SubscriptionsTestCase CreateFromFactory(Func<SubscriptionsTestCase> factory) => factory();
    }
}