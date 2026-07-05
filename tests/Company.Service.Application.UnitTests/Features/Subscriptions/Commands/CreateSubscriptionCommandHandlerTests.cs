using AwesomeAssertions;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Application.Features.Subscriptions.Commands;
using Company.Service.Application.IntegrationEvents.V1.Subscriptions;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace Company.Service.Application.UnitTests.Features.Subscriptions.Commands;

public class CreateSubscriptionCommandHandlerTests : DbContextTestBase
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly MassTransit.IPublishEndpoint _publishEndpoint;
    private readonly CreateSubscriptionCommandHandler _sut;

    public CreateSubscriptionCommandHandlerTests()
    {
        _fakeTimeProvider = new();
        _publishEndpoint = Substitute.For<MassTransit.IPublishEndpoint>();
        _sut = new(DbContext, _publishEndpoint, _fakeTimeProvider);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesAndReturnsSubscription()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
        var tenantId = Guid.NewGuid();

        // Seed an InvoiceAddress and Account — required by FK constraints
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

        DbContext.ChangeTracker.Clear();

        var command = new CreateSubscriptionCommand
        {
            AccountId = account.Id,
            Name = "Premium",
            FriendlyName = "Premium Subscription",
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand { Value = 29.99m, Currency = "USD" },
            BillCycle = Domain.Entities.BillCycle.Monthly,
            StartDate = _fakeTimeProvider.GetUtcNow().DateTime,
            EndDate = _fakeTimeProvider.GetUtcNow().DateTime.AddYears(1),
            ProductId = Guid.NewGuid()
        };

        // Capture published event
        SubscriptionCreatedEvent? capturedEvent = null;
        _publishEndpoint.Publish(
                Arg.Do<SubscriptionCreatedEvent>(e => capturedEvent = e),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccountId.Should().Be(command.AccountId);
        result.Value!.Name.Should().Be(command.Name);
        result.Value!.FriendlyName.Should().Be(command.FriendlyName);
        result.Value!.PurchasePrice.Value.Should().Be(command.PurchasePrice.Value);
        result.Value!.PurchasePrice.Currency.Should().Be(command.PurchasePrice.Currency);
        result.Value!.BillCycle.Should().Be((Domain.Entities.BillCycle)command.BillCycle);
        result.Value!.StartDate.Should().Be(command.StartDate);
        result.Value!.EndDate.Should().Be(command.EndDate);
        result.Value!.ProductId.Should().Be(command.ProductId);
        result.Value!.Status.Should().Be(SubscriptionStatus.Active);
        result.Value!.SuspendedDate.Should().BeNull();

        // Verify it was persisted to the database — all fields
        DbContext.ChangeTracker.Clear();
        var persisted = DbContext.Subscriptions.FirstOrDefault(s => s.Id == result.Value!.Id);
        persisted.Should().NotBeNull();
        persisted!.AccountId.Should().Be(command.AccountId);
        persisted.Name.Should().Be(command.Name);
        persisted.FriendlyName.Should().Be(command.FriendlyName);
        persisted.PurchasePrice.Value.Should().Be(command.PurchasePrice.Value);
        persisted.PurchasePrice.Currency.Should().Be(command.PurchasePrice.Currency);
        persisted.BillCycle.Should().Be((Domain.Entities.BillCycle)command.BillCycle);
        persisted.StartDate.Should().Be(command.StartDate);
        persisted.EndDate.Should().Be(command.EndDate);
        persisted.ProductId.Should().Be(command.ProductId);
        persisted.Status.Should().Be(SubscriptionStatus.Active);
        persisted.SuspendedDate.Should().BeNull();

        // Verify integration event was published with all fields correctly mapped
        capturedEvent.Should().NotBeNull();
        capturedEvent!.SubscriptionId.Should().Be(persisted.Id);
        capturedEvent.AccountId.Should().Be(persisted.AccountId);
        capturedEvent.Name.Should().Be(persisted.Name);
        capturedEvent.FriendlyName.Should().Be(persisted.FriendlyName);
        capturedEvent.PurchasePrice.Value.Should().Be(persisted.PurchasePrice.Value);
        capturedEvent.PurchasePrice.Currency.Should().Be(persisted.PurchasePrice.Currency);
        capturedEvent.BillCycle.Should().Be((IntegrationEvents.V1.Shared.BillCycle)persisted.BillCycle);
        capturedEvent.StartDate.Should().Be(persisted.StartDate);
        capturedEvent.EndDate.Should().Be(persisted.EndDate);
        capturedEvent.ProductId.Should().Be(persisted.ProductId);
        capturedEvent.CreatedDate.Should().Be(_fakeTimeProvider.GetUtcNow().DateTime);
    }

    [Fact]
    public async Task Handle_WithInvalidDataInCommand_ReturnsValidationError()
    {
        // Arrange
        var command = new CreateSubscriptionCommand
        {
            AccountId = Guid.Empty,
            Name = "Premium",
            FriendlyName = "Premium Subscription",
            PurchasePrice = new CreateSubscriptionCommand.PriceCommand { Value = 29.99m, Currency = "USD" },
            BillCycle = Domain.Entities.BillCycle.Monthly,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(1),
            ProductId = Guid.NewGuid()
        };

        // Act
        var result = await _sut.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().BeOfType<ValidationError>();
    }
}