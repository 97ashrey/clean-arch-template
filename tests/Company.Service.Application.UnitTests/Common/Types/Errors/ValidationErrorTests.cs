using Company.Service.Application.Common.Types.Errors;
using FluentAssertions;

namespace Company.Service.Application.UnitTests.Common.Types.Errors;

public class ValidationErrorTests
{
    [Fact]
    public void ToString_WithSinglePropertySingleError_FormatsCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var message = "Validation failed";
        var failures = new[] { new ValidationFailure("Name", ["Name is required"]) };
        
        var error = new ValidationError
        {
            Id = id,
            Message = message,
            Failures = failures
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should()
            .Contain($"ValidationError")
            .And.Contain($"Id = {id}")
            .And.Contain($"Message = {message}")
            .And.Contain("Name: [Name is required]");
    }

    [Fact]
    public void ToString_WithMultipleErrorsOnSingleProperty_FormatsWithCommaSeparation()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Email", ["Email is required", "Email format invalid"])
        };
        
        var error = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should()
            .Contain("Email: [Email is required, Email format invalid]");
    }

    [Fact]
    public void ToString_WithMultipleProperties_FormatsAllProperties()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Name", ["Name is required"]),
            new ValidationFailure("Age", ["Age must be positive"])
        };
        
        var error = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should()
            .Contain("Name: [Name is required]")
            .And.Contain("Age: [Age must be positive]");
    }

    [Fact]
    public void ToString_WithMultiplePropertiesMultipleErrors_FormatsAll()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Name", ["Name required", "Name too long"]),
            new ValidationFailure("Email", ["Email required", "Email invalid"])
        };
        
        var error = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should()
            .Contain("Name: [Name required, Name too long]")
            .And.Contain("Email: [Email required, Email invalid]");
    }

    [Fact]
    public void ToString_FormatsWithCurlyBraces()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Field", ["Error"])
        };
        
        var error = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should()
            .Match("*{ Field: [Error] }*")
            .And.StartWith("ValidationError");
    }

    [Fact]
    public void ToString_IncludesIdMessageAndFailures()
    {
        // Arrange
        var id = Guid.NewGuid();
        var message = "Test message";
        var failures = new[]
        {
            new ValidationFailure("Property", ["Error message"])
        };
        
        var error = new ValidationError
        {
            Id = id,
            Message = message,
            Failures = failures
        };

        // Act
        var result = error.ToString();

        // Assert
        result.Should()
            .Contain($"Id = {id}")
            .And.Contain($"Message = {message}")
            .And.Contain("Failures")
            .And.Contain("Property: [Error message]");
    }
}
