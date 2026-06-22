using AwesomeAssertions;
using Company.Service.Domain.Common.Utils;

namespace Company.Service.Domain.UnitTests.Common.Utils;

public class ValidateTests
{
    #region ExecuteRules Tests

    [Fact]
    public void ExecuteRules_WithNoFailures_ReturnsSuccessResult()
    {
        // Arrange & Act
        var result = Validate.ExecuteRules();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ExecuteRules_WithSingleFailure_ReturnsFailureResult()
    {
        // Arrange
        var failure = new SingleFailure("PropertyA", "Error message");

        // Act
        var result = Validate.ExecuteRules(failure);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Be("Validation failed!");
        result.Error.Failures.Should().HaveCount(1);
        result.Error.Failures[0].PropertyName.Should().Be("PropertyA");
        result.Error.Failures[0].Errors.Should().Contain("Error message");
    }

    [Fact]
    public void ExecuteRules_WithMultipleFailuresDifferentProperties_ReturnsSeparateFailures()
    {
        // Arrange
        var failure1 = new SingleFailure("PropertyA", "Error 1");
        var failure2 = new SingleFailure("PropertyB", "Error 2");

        // Act
        var result = Validate.ExecuteRules(failure1, failure2);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Failures.Should().HaveCount(2);
        result.Error.Failures.Should().Contain(f => f.PropertyName == "PropertyA");
        result.Error.Failures.Should().Contain(f => f.PropertyName == "PropertyB");
    }

    [Fact]
    public void ExecuteRules_WithMultipleFailuresSameProperty_GroupsErrorsForProperty()
    {
        // Arrange
        var failure1 = new SingleFailure("PropertyA", "Error 1");
        var failure2 = new SingleFailure("PropertyA", "Error 2");

        // Act
        var result = Validate.ExecuteRules(failure1, failure2);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Failures.Should().HaveCount(1);
        result.Error.Failures[0].PropertyName.Should().Be("PropertyA");
        result.Error.Failures[0].Errors.Should().HaveCount(2);
        result.Error.Failures[0].Errors.Should().Contain("Error 1");
        result.Error.Failures[0].Errors.Should().Contain("Error 2");
    }

    [Fact]
    public void ExecuteRules_WithNullFailures_FiltersOutNulls()
    {
        // Arrange
        var failure = new SingleFailure("PropertyA", "Error");

        // Act
        var result = Validate.ExecuteRules(null, failure, null);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Failures.Should().HaveCount(1);
        result.Error.Failures[0].PropertyName.Should().Be("PropertyA");
    }

    #endregion

    #region Must Tests

    [Fact]
    public void Must_WithNullReturningRule_ReturnsNull()
    {
        // Arrange
        SingleFailure? TestRule() => null;

        // Act
        var result = Validate.Must(TestRule);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Must_WithFailureReturningRule_ReturnsFailure()
    {
        // Arrange
        SingleFailure? TestRule() => new("PropertyA", "Error message");

        // Act
        var result = Validate.Must(TestRule);

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyA");
        result.Error.Should().Be("Error message");
    }

    #endregion

    #region NotEmpty String Tests

    [Fact]
    public void NotEmpty_WithValidNonEmptyString_ReturnsNull()
    {
        // Act
        var result = Validate.NotEmpty("ValidValue", "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NotEmpty_WithEmptyString_ReturnsFailure()
    {
        // Act
        var result = Validate.NotEmpty("", "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be empty!");
    }

    [Fact]
    public void NotEmpty_WithNullString_ReturnsFailure()
    {
        // Act
        var result = Validate.NotEmpty(null!, "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be empty!");
    }

    #endregion

    #region NotEmpty Guid Tests

    [Fact]
    public void NotEmpty_WithValidGuid_ReturnsNull()
    {
        // Arrange
        var validGuid = Guid.NewGuid();

        // Act
        var result = Validate.NotEmpty(validGuid, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NotEmpty_WithEmptyGuid_ReturnsFailure()
    {
        // Act
        var result = Validate.NotEmpty(Guid.Empty, "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be empty!");
    }

    #endregion

    #region NotZero Int Tests

    [Fact]
    public void NotZero_WithValidInt_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(42, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NotZero_WithZeroInt_ReturnsFailure()
    {
        // Act
        var result = Validate.NotZero(0, "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be zero!");
    }

    [Fact]
    public void NotZero_WithNegativeInt_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(-42, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region NotZero Decimal Tests

    [Fact]
    public void NotZero_WithValidDecimal_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(42.5m, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NotZero_WithZeroDecimal_ReturnsFailure()
    {
        // Act
        var result = Validate.NotZero(0m, "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be zero!");
    }

    [Fact]
    public void NotZero_WithNegativeDecimal_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(-42.5m, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region NotZero Float Tests

    [Fact]
    public void NotZero_WithValidFloat_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(42.5f, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NotZero_WithZeroFloat_ReturnsFailure()
    {
        // Act
        var result = Validate.NotZero(0f, "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be zero!");
    }

    [Fact]
    public void NotZero_WithNegativeFloat_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(-42.5f, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region NotZero Double Tests

    [Fact]
    public void NotZero_WithValidDouble_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(42.5d, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void NotZero_WithZeroDouble_ReturnsFailure()
    {
        // Act
        var result = Validate.NotZero(0d, "PropertyName");

        // Assert
        result.Should().NotBeNull();
        result!.PropertyName.Should().Be("PropertyName");
        result.Error.Should().Be("Must not be zero!");
    }

    [Fact]
    public void NotZero_WithNegativeDouble_ReturnsNull()
    {
        // Act
        var result = Validate.NotZero(-42.5d, "PropertyName");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ExecuteRules_WithComplexScenario_HandlesMultiplePropertiesAndErrors()
    {
        // Arrange & Act
        var result = Validate.ExecuteRules(
            Validate.NotEmpty("", "Name"),
            Validate.NotEmpty(Guid.Empty, "Id"),
            Validate.NotZero(0, "Amount"),
            Validate.NotEmpty("ValidValue", "Address")
        );

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Failures.Should().HaveCount(3);
        result.Error.Failures.Should().Contain(f => f.PropertyName == "Name");
        result.Error.Failures.Should().Contain(f => f.PropertyName == "Id");
        result.Error.Failures.Should().Contain(f => f.PropertyName == "Amount");
        result.Error.Failures.Should().NotContain(f => f.PropertyName == "Address");
    }

    #endregion
}