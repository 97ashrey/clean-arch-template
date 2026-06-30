using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Entities;

namespace Company.Service.Domain.UnitTests.Entities;

public class SubscriptionTests
{
    private readonly Guid _accountId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly DateTime _startDate = new(2026, 1, 1);
    private readonly DateTime _endDate = new(2026, 12, 31);
    private readonly Price _purchasePrice = new(99.99m, "USD");

    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedSubscription()
    {
        // Arrange
        var name = "Premium Subscription";
        var friendlyName = "Premium";
        var billCycle = BillCycle.Monthly;

        // Act
        var result = Subscription.CreateNew(_accountId, name, friendlyName, _purchasePrice, billCycle, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value.AccountId.Should().Be(_accountId);
        result.Value.Name.Should().Be(name);
        result.Value.FriendlyName.Should().Be(friendlyName);
        result.Value.PurchasePrice.Should().Be(_purchasePrice);
        result.Value.BillCycle.Should().Be(billCycle);
        result.Value.StartDate.Should().Be(_startDate);
        result.Value.EndDate.Should().Be(_endDate);
        result.Value.ProductId.Should().Be(_productId);
        result.Value.Status.Should().Be(SubscriptionStatus.Active);
        result.Value.SuspendedDate.Should().BeNull();
    }

    [Fact]
    public void CreateNew_WithEmptyAccountId_ReturnsValidationError()
    {
        // Act
        var result = Subscription.CreateNew(Guid.Empty, "Name", "Friendly", _purchasePrice, BillCycle.Monthly, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "accountId");
    }

    [Fact]
    public void CreateNew_WithEmptyName_ReturnsValidationError()
    {
        // Act
        var result = Subscription.CreateNew(_accountId, "", "Friendly", _purchasePrice, BillCycle.Monthly, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "name");
    }

    [Fact]
    public void CreateNew_WithEmptyFriendlyName_ReturnsValidationError()
    {
        // Act
        var result = Subscription.CreateNew(_accountId, "Name", "", _purchasePrice, BillCycle.Monthly, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "friendlyName");
    }

    [Fact]
    public void CreateNew_WithZeroPriceValue_ReturnsValidationError()
    {
        // Arrange
        var zeroPrice = new Price(0m, "USD");

        // Act
        var result = Subscription.CreateNew(_accountId, "Name", "Friendly", zeroPrice, BillCycle.Monthly, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "purchasePrice.Value");
    }

    [Fact]
    public void CreateNew_WithEmptyPriceCurrency_ReturnsValidationError()
    {
        // Arrange
        var invalidPrice = new Price(99.99m, "");

        // Act
        var result = Subscription.CreateNew(_accountId, "Name", "Friendly", invalidPrice, BillCycle.Monthly, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "purchasePrice.Currency");
    }

    [Fact]
    public void CreateNew_WhenStartDateAfterEndDate_ReturnsValidationError()
    {
        // Arrange
        var startDate = new DateTime(2026, 12, 31);
        var endDate = new DateTime(2026, 1, 1);

        // Act
        var result = Subscription.CreateNew(_accountId, "Name", "Friendly", _purchasePrice, BillCycle.Monthly, startDate, endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "startDate-endDate");
    }

    [Fact]
    public void CreateNew_WithStartDateEqualToEndDate_ReturnsSuccess()
    {
        // Arrange
        var sameDate = new DateTime(2026, 6, 1);

        // Act
        var result = Subscription.CreateNew(_accountId, "Name", "Friendly", _purchasePrice, BillCycle.Monthly, sameDate, sameDate, _productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(BillCycle.Monthly)]
    [InlineData(BillCycle.Yearly)]
    public void CreateNew_WithAllBillCycles_CreatesSuccessfully(BillCycle billCycle)
    {
        // Act
        var result = Subscription.CreateNew(_accountId, "Name", "Friendly", _purchasePrice, billCycle, _startDate, _endDate, _productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.BillCycle.Should().Be(billCycle);
    }

    [Fact]
    public void Update_WithValidFriendlyName_UpdatesName()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var newFriendlyName = "Updated Friendly Name";

        // Act
        var result = subscription.Update(newFriendlyName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.FriendlyName.Should().Be(newFriendlyName);
    }

    [Fact]
    public void Update_WithEmptyFriendlyName_ReturnsValidationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var originalName = subscription.FriendlyName;

        // Act
        var result = subscription.Update("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        subscription.FriendlyName.Should().Be(originalName);
    }

    [Fact]
    public void Update_WithNullFriendlyName_ReturnsValidationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var originalName = subscription.FriendlyName;

        // Act
        var result = subscription.Update(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        subscription.FriendlyName.Should().Be(originalName);
    }

    [Fact]
    public void Suspend_WhenActive_UpdatesStatusAndSetsSuspendedDate()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        var suspendedDate = new DateTime(2026, 3, 15);

        // Act
        var result = subscription.Suspend(suspendedDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
        subscription.SuspendedDate.Should().Be(suspendedDate);
    }

    [Fact]
    public void Suspend_WhenSuspended_ReturnsInvalidOperationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend(new DateTime(2026, 3, 15));

        // Act
        var result = subscription.Suspend(new DateTime(2026, 4, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't suspend the subscription it is not in {SubscriptionStatus.Active} state!");
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenCanceled_ReturnsInvalidOperationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend(new DateTime(2026, 3, 15));
        subscription.Cancel();

        // Act
        var result = subscription.Suspend(new DateTime(2026, 4, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't suspend the subscription it is not in {SubscriptionStatus.Active} state!");
    }

    [Fact]
    public void ReActivate_WhenSuspended_UpdatesStatusAndClearsSuspendedDate()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend(new DateTime(2026, 3, 15));

        // Act
        var result = subscription.ReActivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.SuspendedDate.Should().BeNull();
    }

    [Fact]
    public void ReActivate_WhenActive_ReturnsInvalidOperationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act
        var result = subscription.ReActivate();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't re-activate the subscription it is not in {SubscriptionStatus.Suspended} state!");
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void ReActivate_WhenCanceled_ReturnsInvalidOperationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend(new DateTime(2026, 3, 15));
        subscription.Cancel();

        // Act
        var result = subscription.ReActivate();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't re-activate the subscription it is not in {SubscriptionStatus.Suspended} state!");
        subscription.Status.Should().Be(SubscriptionStatus.Canceled);
    }

    [Fact]
    public void Cancel_WhenSuspended_UpdatesStatusToCanceled()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend(new DateTime(2026, 3, 15));

        // Act
        var result = subscription.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Canceled);
    }

    [Fact]
    public void Cancel_WhenActive_ReturnsInvalidOperationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act
        var result = subscription.Cancel();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't cancel the subscription it is not in {SubscriptionStatus.Suspended} state!");
        subscription.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public void Cancel_WhenCanceled_ReturnsInvalidOperationError()
    {
        // Arrange
        var subscription = CreateValidSubscription();
        subscription.Suspend(new DateTime(2026, 3, 15));
        subscription.Cancel();

        // Act
        var result = subscription.Cancel();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't cancel the subscription it is not in {SubscriptionStatus.Suspended} state!");
        subscription.Status.Should().Be(SubscriptionStatus.Canceled);
    }

    [Fact]
    public void FullLifecycle_ActiveToSuspendedToActiveToSuspendedToCanceled()
    {
        // Arrange
        var subscription = CreateValidSubscription();

        // Act & Assert - Active
        subscription.Status.Should().Be(SubscriptionStatus.Active);

        // Active -> Suspended
        subscription.Suspend(new DateTime(2026, 3, 1)).IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);

        // Suspended -> Active
        subscription.ReActivate().IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Active);
        subscription.SuspendedDate.Should().BeNull();

        // Active -> Suspended
        subscription.Suspend(new DateTime(2026, 6, 1)).IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);

        // Suspended -> Canceled
        subscription.Cancel().IsSuccess.Should().BeTrue();
        subscription.Status.Should().Be(SubscriptionStatus.Canceled);

        // Canceled -> cannot do anything
        subscription.ReActivate().IsSuccess.Should().BeFalse();
        subscription.Suspend(new DateTime(2026, 7, 1)).IsSuccess.Should().BeFalse();
        subscription.Cancel().IsSuccess.Should().BeFalse();
        subscription.Status.Should().Be(SubscriptionStatus.Canceled);
    }

    [Fact]
    public void InternalConstructor_RecreatesSubscriptionWithGivenValues()
    {
        // Arrange
        var id = Guid.NewGuid();
        var suspendedDate = new DateTime(2026, 3, 15);
        var price = new Price(49.99m, "EUR");

        // Act
        var subscription = new Subscription(id, _accountId, "Name", "Friendly", price, BillCycle.Yearly,
            _startDate, _endDate, SubscriptionStatus.Suspended, suspendedDate, _productId);

        // Assert
        subscription.Id.Should().Be(id);
        subscription.AccountId.Should().Be(_accountId);
        subscription.Name.Should().Be("Name");
        subscription.FriendlyName.Should().Be("Friendly");
        subscription.PurchasePrice.Should().Be(price);
        subscription.BillCycle.Should().Be(BillCycle.Yearly);
        subscription.StartDate.Should().Be(_startDate);
        subscription.EndDate.Should().Be(_endDate);
        subscription.Status.Should().Be(SubscriptionStatus.Suspended);
        subscription.SuspendedDate.Should().Be(suspendedDate);
        subscription.ProductId.Should().Be(_productId);
    }

    private Subscription CreateValidSubscription()
    {
        return Subscription.CreateNew(_accountId, "Premium Subscription", "Premium",
            _purchasePrice, BillCycle.Monthly, _startDate, _endDate, _productId).Value!;
    }
}