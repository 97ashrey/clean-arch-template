using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.UnitTests.ValueObjects;

public class ContactInformationTests
{
    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedContactInformation()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var phoneNumber = "+1234567890";

        // Act
        var result = ContactInformation.CreateNew(firstName, lastName, email, phoneNumber);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.Email.Should().Be(email);
        result.Value.PhoneNumber.Should().Be(phoneNumber);
    }

    [Theory]
    [InlineData("", "LastName", "Email", "Phone", "firstName")]
    [InlineData("FirstName", "", "Email", "Phone", "lastName")]
    [InlineData("FirstName", "LastName", "", "Phone", "email")]
    [InlineData("FirstName", "LastName", "Email", "", "phoneNumber")]
    public void CreateNew_WithEmptyField_ReturnsValidationError(
        string firstName, string lastName, string email, string phoneNumber, string expectedProperty)
    {
        // Act
        var result = ContactInformation.CreateNew(firstName, lastName, email, phoneNumber);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == expectedProperty);
    }

    [Fact]
    public void CreateNew_WithAllEmptyFields_ReturnsMultipleValidationErrors()
    {
        // Act
        var result = ContactInformation.CreateNew("", "", "", "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Failures.Should().HaveCount(4);
        result.Error.Failures.Select(f => f.PropertyName).Should()
            .BeEquivalentTo(["firstName", "lastName", "email", "phoneNumber"]);
    }

    [Fact]
    public void CreateNew_WithNullFields_ReturnsValidationErrors()
    {
        // Act
        var result = ContactInformation.CreateNew(null!, "Doe", "email@test.com", "123");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "firstName");
    }
}