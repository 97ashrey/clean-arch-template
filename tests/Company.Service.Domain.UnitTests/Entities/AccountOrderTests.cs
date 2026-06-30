using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Entities;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.UnitTests.Entities;

public class AccountOrderTests
{
    private static readonly AccountDetails ValidAccountDetails = AccountDetails
        .CreateNew("Test Account", "test@example.com", AccountTier.Business, Guid.NewGuid()).Value!;

    private static readonly ContactInformation ValidContactInformation = ContactInformation
        .CreateNew("John", "Doe", "john@example.com", "+1234567890").Value!;

    private readonly DateTime _createdDate = new(2026, 6, 1);
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedAccountOrder()
    {
        // Act
        var result = AccountOrder.CreateNew(_tenantId, ValidAccountDetails, ValidContactInformation, _createdDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value.TenantId.Should().Be(_tenantId);
        result.Value.AccountDetails.Should().Be(ValidAccountDetails);
        result.Value.ContactInformation.Should().Be(ValidContactInformation);
        result.Value.CreatedDate.Should().Be(_createdDate);
        result.Value.Status.Should().Be(AccountOrderStatus.Pending);
        result.Value.AccountId.Should().BeNull();
        result.Value.CompletedDate.Should().BeNull();
    }

    [Fact]
    public void CreateNew_WithEmptyTenantId_ReturnsValidationError()
    {
        // Act
        var result = AccountOrder.CreateNew(Guid.Empty, ValidAccountDetails, ValidContactInformation, _createdDate);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "tenantId");
    }

    [Fact]
    public void StartProcessing_WhenPending_SetsStatusToProcessing()
    {
        // Arrange
        var order = CreateValidOrder();

        // Act
        var result = order.StartProcessing();

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(AccountOrderStatus.Processing);
    }

    [Fact]
    public void StartProcessing_WhenProcessing_ReturnsInvalidOperationError()
    {
        // Arrange
        var order = CreateValidOrder();
        order.StartProcessing();

        // Act
        var result = order.StartProcessing();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't start processing order. It is not in {AccountOrderStatus.Pending} state!");
        order.Status.Should().Be(AccountOrderStatus.Processing);
    }

    [Fact]
    public void StartProcessing_WhenCompleted_ReturnsInvalidOperationError()
    {
        // Arrange
        var order = CreateValidOrder();
        order.StartProcessing();
        var account = CreateValidAccount();
        order.Complete(account, new DateTime(2026, 6, 10));

        // Act
        var result = order.StartProcessing();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't start processing order. It is not in {AccountOrderStatus.Pending} state!");
    }

    [Fact]
    public void Complete_WhenProcessing_UpdatesStatusAndSetsCompletedDateAndAccountId()
    {
        // Arrange
        var order = CreateValidOrder();
        order.StartProcessing();
        var account = CreateValidAccount();
        var completedDate = new DateTime(2026, 6, 10);

        // Act
        var result = order.Complete(account, completedDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(AccountOrderStatus.Completed);
        order.CompletedDate.Should().Be(completedDate);
        order.AccountId.Should().Be(account.Id);
    }

    [Fact]
    public void Complete_WhenPending_ReturnsInvalidOperationError()
    {
        // Arrange
        var order = CreateValidOrder();
        var account = CreateValidAccount();

        // Act
        var result = order.Complete(account, new DateTime(2026, 6, 10));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't complete order. It is not in {AccountOrderStatus.Processing} state!.");
        order.Status.Should().Be(AccountOrderStatus.Pending);
    }

    [Fact]
    public void Complete_WhenCompleted_ReturnsInvalidOperationError()
    {
        // Arrange
        var order = CreateValidOrder();
        order.StartProcessing();
        var account = CreateValidAccount();
        order.Complete(account, new DateTime(2026, 6, 10));

        // Act
        var result = order.Complete(account, new DateTime(2026, 6, 15));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't complete order. It is not in {AccountOrderStatus.Processing} state!.");
        order.Status.Should().Be(AccountOrderStatus.Completed);
    }

    [Fact]
    public void Complete_WithCompletedDateBeforeCreatedDate_ReturnsInvalidOperationError()
    {
        // Arrange
        var order = CreateValidOrder();
        order.StartProcessing();
        var account = CreateValidAccount();

        // Act
        var result = order.Complete(account, _createdDate.AddDays(-1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't complete order. Completed Date {_createdDate.AddDays(-1)} is before Created Date {_createdDate}.");
        order.Status.Should().Be(AccountOrderStatus.Processing);
        order.CompletedDate.Should().BeNull();
        order.AccountId.Should().BeNull();
    }

    [Fact]
    public void Complete_WithCompletedDateEqualToCreatedDate_CompletesSuccessfully()
    {
        // Arrange
        var order = CreateValidOrder();
        order.StartProcessing();
        var account = CreateValidAccount();

        // Act
        var result = order.Complete(account, _createdDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(AccountOrderStatus.Completed);
        order.CompletedDate.Should().Be(_createdDate);
    }

    [Fact]
    public void FullLifecycle_PendingToProcessingToCompleted()
    {
        // Arrange
        var order = CreateValidOrder();
        var account = CreateValidAccount();
        var completedDate = new DateTime(2026, 6, 15);

        // Act & Assert
        order.Status.Should().Be(AccountOrderStatus.Pending);

        order.StartProcessing().IsSuccess.Should().BeTrue();
        order.Status.Should().Be(AccountOrderStatus.Processing);

        order.Complete(account, completedDate).IsSuccess.Should().BeTrue();
        order.Status.Should().Be(AccountOrderStatus.Completed);
        order.CompletedDate.Should().Be(completedDate);
        order.AccountId.Should().Be(account.Id);
    }

    private AccountOrder CreateValidOrder()
    {
        return AccountOrder.CreateNew(_tenantId, ValidAccountDetails, ValidContactInformation, _createdDate).Value!;
    }

    private static Account CreateValidAccount()
    {
        return Account.CreateNew(Guid.NewGuid(), "Test Account", "test@example.com", AccountTier.Individual, Guid.NewGuid()).Value!;
    }
}