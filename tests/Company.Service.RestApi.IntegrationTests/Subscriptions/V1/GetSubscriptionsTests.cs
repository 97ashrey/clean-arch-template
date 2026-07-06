using AwesomeAssertions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Company.Service.RestApi.Api;
using Flurl;
using System.Net;
using System.Net.Http.Json;
using V1Contracts = Company.Service.RestApi.Api.Subscriptions.V1.Contracts;

namespace Company.Service.RestApi.IntegrationTests.Subscriptions.V1;

public class GetSubscriptionsTests(IntegrationTestWebAppFactory factory) : IntegrationTestBase(factory)
{
    public static TheoryData<GetSubscriptionsTestCase> Data =>
    [
        GetSubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, account) = SeedHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var s1 = CreateSubscription(account.Id, "Premium", "Premium Sub", Domain.Entities.BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(account.Id, "Basic", "Basic Sub", Domain.Entities.BillCycle.Yearly, productId2);

            return new()
            {
                Name = "Get all subscriptions",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2],
                Request = new(),
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(seed.Count);
                    pagedResponse.TotalCount.Should().Be(seed.Count);
                    pagedResponse.CurrentPage.Should().Be(1);
                }
            };
        }),
        GetSubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, account) = SeedHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(account.Id, "A", "Sub A", Domain.Entities.BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(account.Id, "B", "Sub B", Domain.Entities.BillCycle.Yearly, productId2);
            var s3 = CreateSubscription(account.Id, "C", "Sub C", Domain.Entities.BillCycle.Monthly, productId3);

            return new()
            {
                Name = "Filter by IDs",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Request = new()
                {
                    Ids = [s1.Id, s3.Id]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(i => i.Id == s1.Id);
                    pagedResponse.Items.Should().Contain(i => i.Id == s3.Id);
                    pagedResponse.Items.Should().NotContain(i => i.Id == s2.Id);
                }
            };
        }),
        GetSubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, account) = SeedHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(account.Id, "Premium Plan", "Premium Sub", Domain.Entities.BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(account.Id, "Basic Plan", "Basic Sub", Domain.Entities.BillCycle.Yearly, productId2);
            var s3 = CreateSubscription(account.Id, "Gold Plan", "Gold Sub", Domain.Entities.BillCycle.Monthly, productId3);

            return new()
            {
                Name = "Filter by search term",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Request = new()
                {
                    SearchTerm = "Premium"
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(1);
                    pagedResponse.Items.Should().Contain(i => i.Name == "Premium Plan");
                }
            };
        }),
        GetSubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, account) = SeedHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(account.Id, "Monthly A", "Monthly Sub", Domain.Entities.BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(account.Id, "Yearly B", "Yearly Sub", Domain.Entities.BillCycle.Yearly, productId2);
            var s3 = CreateSubscription(account.Id, "Monthly C", "Monthly Sub C", Domain.Entities.BillCycle.Monthly, productId3);

            return new()
            {
                Name = "Filter by bill cycles",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Request = new()
                {
                    BillCycles = [V1Contracts.BillCycle.Monthly]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(2);
                    pagedResponse.Items.Should().Contain(i => i.Id == s1.Id);
                    pagedResponse.Items.Should().Contain(i => i.Id == s3.Id);
                    pagedResponse.Items.Should().NotContain(i => i.Id == s2.Id);
                }
            };
        }),
        GetSubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, account) = SeedHierarchy();
            var productId1 = Guid.NewGuid();
            var productId2 = Guid.NewGuid();
            var productId3 = Guid.NewGuid();
            var s1 = CreateSubscription(account.Id, "Active A", "Active Sub", Domain.Entities.BillCycle.Monthly, productId1);
            var s2 = CreateSubscription(account.Id, "Suspended B", "Suspended Sub", Domain.Entities.BillCycle.Monthly, productId2);
            var s3 = CreateSubscription(account.Id, "Canceled C", "Canceled Sub", Domain.Entities.BillCycle.Monthly, productId3);

            s2.Suspend(new DateTime(2026, 6, 1));
            s3.Suspend(new DateTime(2026, 6, 1));
            s3.Cancel();

            return new()
            {
                Name = "Filter by statuses",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = [s1, s2, s3],
                Request = new()
                {
                    Statuses = [V1Contracts.SubscriptionStatus.Suspended]
                },
                Assert = (pagedResponse, seed) =>
                {
                    pagedResponse.Items.Should().HaveCount(1);
                    pagedResponse.Items.Should().Contain(i => i.Id == s2.Id);
                    pagedResponse.Items.Should().NotContain(i => i.Id == s1.Id);
                    pagedResponse.Items.Should().NotContain(i => i.Id == s3.Id);
                }
            };
        }),
        GetSubscriptionsTestCase.CreateFromFactory(() =>
        {
            var (invoiceAddress, account) = SeedHierarchy();
            List<Subscription> subscriptions = [.. Enumerable.Range(1, 25)
                .Select(i => CreateSubscription(
                    account.Id,
                    $"Subscription {i}",
                    $"Sub {i}",
                    i % 2 == 0 ? Domain.Entities.BillCycle.Monthly : Domain.Entities.BillCycle.Yearly,
                    Guid.NewGuid()))];

            return new()
            {
                Name = "Pagination - get second page with 10 items",
                InvoiceAddresses = [invoiceAddress],
                Accounts = [account],
                Seed = subscriptions,
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
            Name = "Empty result - no subscriptions",
            InvoiceAddresses = [],
            Accounts = [],
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
    public async Task GetSubscriptions_ReturnsExpectedResults(GetSubscriptionsTestCase testCase)
    {
        // Arrange — seed InvoiceAddresses first (FK constraint), then Accounts, then Subscriptions
        DbContext.InvoiceAdresses.AddRange(testCase.InvoiceAddresses);
        DbContext.Accounts.AddRange(testCase.Accounts);
        DbContext.Subscriptions.AddRange(testCase.Seed);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var accountId = testCase.Accounts.Count > 0
            ? testCase.Accounts[0].Id
            : Guid.NewGuid();
        var response = await Client.GetAsync($"/api/v1/accounts/{accountId}/subscriptions".SetQueryParams(testCase.Request));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.Subscription>>();
        pagedResponse.Should().NotBeNull();

        testCase.AssertResponse(pagedResponse!, testCase.Seed);
    }

    [Fact]
    public async Task GetSubscriptions_ReturnsFullContractWithAllPropertiesMappedCorrectly()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();

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

        DbContext.InvoiceAdresses.Add(invoiceAddress);
        await DbContext.SaveChangesAsync();

        var account = Account.CreateNew(
            tenantId: tenantId,
            name: "Test Account",
            email: "test@example.com",
            tier: AccountTier.Business,
            invoiceAddressId: invoiceAddress.Id
        ).Value!;

        DbContext.Accounts.Add(account);
        await DbContext.SaveChangesAsync();

        var subscription = Subscription.CreateNew(
            accountId: account.Id,
            name: "Premium Plan",
            friendlyName: "Premium Subscription",
            purchasePrice: Price.CreateNew(29.99m, "USD").Value!,
            billCycle: Domain.Entities.BillCycle.Monthly,
            startDate: new DateTime(2026, 1, 1),
            endDate: new DateTime(2026, 12, 31),
            productId: productId
        ).Value!;

        DbContext.Subscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        DbContext.ChangeTracker.Clear();

        // Act
        var response = await Client.GetAsync($"/api/v1/accounts/{account.Id}/subscriptions");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<V1Contracts.Subscription>>();
        pagedResponse.Should().NotBeNull();
        pagedResponse!.Items.Should().HaveCount(1);

        var item = pagedResponse.Items[0];

        item.Id.Should().Be(subscription.Id);
        item.AccountId.Should().Be(subscription.AccountId);
        item.Name.Should().Be(subscription.Name);
        item.FriendlyName.Should().Be(subscription.FriendlyName);
        item.PurchasePrice.Value.Should().Be(subscription.PurchasePrice.Value);
        item.PurchasePrice.Currency.Should().Be(subscription.PurchasePrice.Currency);
        item.BillCycle.Should().Be((V1Contracts.BillCycle)subscription.BillCycle);
        item.StartDate.Should().Be(subscription.StartDate);
        item.EndDate.Should().Be(subscription.EndDate);
        item.ProductId.Should().Be(subscription.ProductId);
        item.Status.Should().Be(V1Contracts.SubscriptionStatus.Active);
        item.SuspendedDate.Should().BeNull();
    }

    private static Subscription CreateSubscription(Guid accountId, string name, string friendlyName, Domain.Entities.BillCycle billCycle, Guid productId)
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

    private static (InvoiceAddress InvoiceAddress, Account Account) SeedHierarchy()
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

        return (invoiceAddress, account);
    }

    public class GetSubscriptionsTestCase
    {
        public required string Name { get; init; }

        public required List<InvoiceAddress> InvoiceAddresses { get; init; }

        public required List<Account> Accounts { get; init; }

        public required List<Subscription> Seed { get; init; }

        public required V1Contracts.GetSubscriptionsRequest Request { get; init; }

        public required Action<PagedResponse<V1Contracts.Subscription>, List<Subscription>> Assert { private get; init; }

        public void AssertResponse(PagedResponse<V1Contracts.Subscription> response, List<Subscription> seed)
        {
            Assert(response, seed);
        }

        public static GetSubscriptionsTestCase CreateFromFactory(Func<GetSubscriptionsTestCase> factory) => factory();
    }
}