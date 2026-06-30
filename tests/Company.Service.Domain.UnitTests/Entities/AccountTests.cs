using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.Entities;

namespace Company.Service.Domain.UnitTests.Entities;

public class AccountTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _invoiceAddressId = Guid.NewGuid();

    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedAccount()
    {
        // Arrange
        var name = "Test Account";
        var email = "test@example.com";
        var tier = AccountTier.Business;

        // Act
        var result = Account.CreateNew(_tenantId, name, email, tier, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().NotBeEmpty();
        result.Value.TenantId.Should().Be(_tenantId);
        result.Value.Name.Should().Be(name);
        result.Value.Email.Should().Be(email);
        result.Value.Tier.Should().Be(tier);
        result.Value.InvoiceAddressId.Should().Be(_invoiceAddressId);
        result.Value.Status.Should().Be(AccountStatus.Active);
        result.Value.SuspendedDate.Should().BeNull();
    }

    [Fact]
    public void CreateNew_WithEmptyTenantId_ReturnsValidationError()
    {
        // Act
        var result = Account.CreateNew(Guid.Empty, "Name", "email@test.com", AccountTier.Individual, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "tenantId");
    }

    [Fact]
    public void CreateNew_WithEmptyName_ReturnsValidationError()
    {
        // Act
        var result = Account.CreateNew(_tenantId, "", "email@test.com", AccountTier.Individual, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "name");
    }

    [Fact]
    public void CreateNew_WithEmptyEmail_ReturnsValidationError()
    {
        // Act
        var result = Account.CreateNew(_tenantId, "Name", "", AccountTier.Individual, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "email");
    }

    [Fact]
    public void CreateNew_WithEmptyInvoiceAddressId_ReturnsValidationError()
    {
        // Act
        var result = Account.CreateNew(_tenantId, "Name", "email@test.com", AccountTier.Individual, Guid.Empty);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "invoiceAddressId");
    }

    [Theory]
    [InlineData(AccountTier.Individual)]
    [InlineData(AccountTier.Business)]
    [InlineData(AccountTier.Enterprise)]
    public void CreateNew_WithAllTiers_CreatesSuccessfully(AccountTier tier)
    {
        // Act
        var result = Account.CreateNew(_tenantId, "Name", "email@test.com", tier, _invoiceAddressId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Tier.Should().Be(tier);
    }

    [Fact]
    public void ChangeName_WithValidName_UpdatesName()
    {
        // Arrange
        var account = CreateValidAccount();
        var newName = "Updated Name";

        // Act
        var result = account.ChangeName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Name.Should().Be(newName);
    }

    [Fact]
    public void ChangeName_WithEmptyName_ReturnsValidationError()
    {
        // Arrange
        var account = CreateValidAccount();
        var originalName = account.Name;

        // Act
        var result = account.ChangeName("");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        account.Name.Should().Be(originalName);
    }

    [Fact]
    public void ChangeName_WithNullName_ReturnsValidationError()
    {
        // Arrange
        var account = CreateValidAccount();
        var originalName = account.Name;

        // Act
        var result = account.ChangeName(null!);

        // Assert
        result.IsSuccess.Should().BeFalse();
        account.Name.Should().Be(originalName);
    }

    [Fact]
    public void Suspend_WhenActive_UpdatesStatusAndSetsSuspendedDate()
    {
        // Arrange
        var account = CreateValidAccount();
        var suspendedDate = new DateTime(2026, 6, 15);

        // Act
        var result = account.Suspend(suspendedDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Suspended);
        account.SuspendedDate.Should().Be(suspendedDate);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ReturnsInvalidOperationError()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Suspend(new DateTime(2026, 6, 15));

        // Act
        var result = account.Suspend(new DateTime(2026, 7, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't suspend the account it is not in {AccountStatus.Active} state!");
        account.Status.Should().Be(AccountStatus.Suspended);
    }

    [Fact]
    public void Suspend_WhenRemoved_ReturnsInvalidOperationError()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Suspend(new DateTime(2026, 6, 15));
        account.Remove();

        // Act
        var result = account.Suspend(new DateTime(2026, 7, 1));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't suspend the account it is not in {AccountStatus.Active} state!");
    }

    [Fact]
    public void ReActivate_WhenSuspended_UpdatesStatusAndClearsSuspendedDate()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Suspend(new DateTime(2026, 6, 15));

        // Act
        var result = account.ReActivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Active);
        account.SuspendedDate.Should().BeNull();
    }

    [Fact]
    public void ReActivate_WhenActive_ReturnsInvalidOperationError()
    {
        // Arrange
        var account = CreateValidAccount();

        // Act
        var result = account.ReActivate();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't re-activate the account it is not in {AccountStatus.Suspended} state!");
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void ReActivate_WhenRemoved_ReturnsInvalidOperationError()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Suspend(new DateTime(2026, 6, 15));
        account.Remove();

        // Act
        var result = account.ReActivate();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't re-activate the account it is not in {AccountStatus.Suspended} state!");
        account.Status.Should().Be(AccountStatus.Removed);
    }

    [Fact]
    public void Remove_WhenSuspended_UpdatesStatusToRemoved()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Suspend(new DateTime(2026, 6, 15));

        // Act
        var result = account.Remove();

        // Assert
        result.IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Removed);
    }

    [Fact]
    public void Remove_WhenActive_ReturnsInvalidOperationError()
    {
        // Arrange
        var account = CreateValidAccount();

        // Act
        var result = account.Remove();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't remove the account it is not in {AccountStatus.Suspended} state!");
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Remove_WhenRemoved_ReturnsInvalidOperationError()
    {
        // Arrange
        var account = CreateValidAccount();
        account.Suspend(new DateTime(2026, 6, 15));
        account.Remove();

        // Act
        var result = account.Remove();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationError>();
        result.Error!.Message.Should().Be($"Can't remove the account it is not in {AccountStatus.Suspended} state!");
        account.Status.Should().Be(AccountStatus.Removed);
    }

    [Fact]
    public void FullLifecycle_ActiveToSuspendedToActiveToSuspendedToRemoved()
    {
        // Arrange
        var account = CreateValidAccount();
        var date1 = new DateTime(2026, 1, 1);
        var date2 = new DateTime(2026, 3, 1);
        var date3 = new DateTime(2026, 6, 1);

        // Act & Assert - Active -> Suspended
        account.Suspend(date1).IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Suspended);
        account.SuspendedDate.Should().Be(date1);

        // Suspended -> Active
        account.ReActivate().IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Active);
        account.SuspendedDate.Should().BeNull();

        // Active -> Suspended
        account.Suspend(date2).IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Suspended);
        account.SuspendedDate.Should().Be(date2);

        // Suspended -> Removed
        account.Remove().IsSuccess.Should().BeTrue();
        account.Status.Should().Be(AccountStatus.Removed);

        // Removed -> cannot do anything
        account.ReActivate().IsSuccess.Should().BeFalse();
        account.Suspend(date3).IsSuccess.Should().BeFalse();
        account.Remove().IsSuccess.Should().BeFalse();
        account.Status.Should().Be(AccountStatus.Removed);
    }

    private Account CreateValidAccount()
    {
        return Account.CreateNew(_tenantId, "Test Account", "test@example.com", AccountTier.Individual, _invoiceAddressId).Value!;
    }
}