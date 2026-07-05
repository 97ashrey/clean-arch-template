using AwesomeAssertions;
using Company.Service.Domain.Common.Types.Errors;
using Company.Service.Domain.ValueObjects;

namespace Company.Service.Domain.UnitTests.ValueObjects;

public class PriceTests
{
    [Theory]
    [InlineData(99.99, "USD")]
    [InlineData(50.00, "EUR")]
    [InlineData(75.25, "CAD")]
    [InlineData(10.00, "GBP")]
    [InlineData(20.00, "CHF")]
    [InlineData(30.00, "NOK")]
    [InlineData(40.00, "SEK")]
    [InlineData(50.00, "DKK")]
    [InlineData(60.00, "PLN")]
    [InlineData(70.00, "CZK")]
    [InlineData(80.00, "HUF")]
    [InlineData(90.00, "RON")]
    [InlineData(11.00, "BGN")]
    [InlineData(22.00, "ISK")]
    public void CreateNew_WithValidInputs_ReturnsSuccessWithPopulatedPrice(decimal value, string currency)
    {
        // Act
        var result = Price.CreateNew(value, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(value);
        result.Value.Currency.Should().Be(currency.ToUpperInvariant());
    }

    [Theory]
    [InlineData("usd")]
    [InlineData("eur")]
    [InlineData("cad")]
    [InlineData("Usd")]
    [InlineData("EUR")]
    public void CreateNew_WithCurrencyCaseInsensitivity_ConvertsToUppercase(string currency)
    {
        // Arrange
        var value = 29.99m;

        // Act
        var result = Price.CreateNew(value, currency);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Currency.Should().Be(currency.ToUpperInvariant());
    }

    [Fact]
    public void CreateNew_WithZeroValue_ReturnsValidationError()
    {
        // Act
        var result = Price.CreateNew(0m, "USD");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "value");
    }

    [Fact]
    public void CreateNew_WithEmptyCurrency_ReturnsValidationError()
    {
        // Act
        var result = Price.CreateNew(99.99m, "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "currency");
    }

    [Theory]
    [InlineData("JPY")]
    [InlineData("BRL")]
    [InlineData("XYZ")]
    [InlineData("AUD")]
    [InlineData("MXN")]
    public void CreateNew_WithInvalidCurrency_ReturnsValidationError(string currency)
    {
        // Act
        var result = Price.CreateNew(99.99m, currency);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<ValidationError>();
        result.Error!.Failures.Should().Contain(f => f.PropertyName == "currency");
    }
}