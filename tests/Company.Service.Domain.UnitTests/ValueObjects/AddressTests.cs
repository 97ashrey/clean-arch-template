//__EXAMPLE_START__
using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.UnitTests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedAddress()
    {
        // Arrange
        var country = "USA";
        var city = "New York";
        var zipCode = "10001";
        var street = "Broadway";
        var number = "123";

        // Act
        var result = Address.CreateNew(country, city, zipCode, street, number);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Country.Should().Be(country);
        result.Value.City.Should().Be(city);
        result.Value.ZipCode.Should().Be(zipCode);
        result.Value.Street.Should().Be(street);
        result.Value.Number.Should().Be(number);
    }

    [Theory]
    [InlineData("", "City", "ZipCode", "Street", "Number", "country")]
    [InlineData("Country", "", "ZipCode", "Street", "Number", "city")]
    [InlineData("Country", "City", "", "Street", "Number", "zipCode")]
    [InlineData("Country", "City", "ZipCode", "", "Number", "street")]
    [InlineData("Country", "City", "ZipCode", "Street", "", "number")]
    public void CreateNew_WithEmptyField_ReturnsValidationError(
        string country, string city, string zipCode, string street, string number, string expectedProperty)
    {
        // Act
        var result = Address.CreateNew(country, city, zipCode, street, number);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == expectedProperty);
    }

    [Fact]
    public void CreateNew_WithAllEmptyFields_ReturnsMultipleValidationErrors()
    {
        // Act
        var result = Address.CreateNew("", "", "", "", "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Failures.Should().HaveCount(5);
        result.Error.Failures.Select(f => f.PropertyName).Should()
            .BeEquivalentTo(["country", "city", "zipCode", "street", "number"]);
    }

    [Fact]
    public void CreateNew_WithNullFields_ReturnsValidationErrors()
    {
        // Act
        var result = Address.CreateNew(null!, "City", "ZipCode", "Street", "Number");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "country");
    }
}
//__EXAMPLE_END__
