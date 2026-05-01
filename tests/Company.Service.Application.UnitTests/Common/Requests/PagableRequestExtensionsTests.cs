using Company.Service.Application.Common.Requests;
using FluentAssertions;

namespace Company.Service.Application.UnitTests.Common.Requests;

public class PagableRequestExtensionsTests
{
    private class FakePagableRequest : IPagableRequest
    {
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
    }

    #region GetPageNumberOrDefault

    [Fact]
    public void GetPageNumberOrDefault_WithValidPageNumber_ShouldReturnPageNumber()
    {
        // Arrange
        var request = new FakePagableRequest { PageNumber = 5 };

        // Act
        var result = request.GetPageNumberOrDefault();

        // Assert
        result.Should().Be(request.PageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void GetPageNumberOrDefault_WithInvalidPageNumber_ShouldReturnDefault(int invalidPageNumber)
    {
        // Arrange
        var request = new FakePagableRequest { PageNumber = invalidPageNumber };

        // Act
        var result = request.GetPageNumberOrDefault();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void GetPageNumberOrDefault_WithInvalidPageNumberAndCustomDefault_ShouldReturnCustomDefault()
    {
        // Arrange
        var request = new FakePagableRequest { PageNumber = 0 };
        const int customDefault = 5;

        // Act
        var result = request.GetPageNumberOrDefault(customDefault);

        // Assert
        result.Should().Be(customDefault);
    }

    [Fact]
    public void GetPageNumberOrDefault_WithValidPageNumberAndCustomDefault_ShouldReturnPageNumber()
    {
        // Arrange
        var request = new FakePagableRequest { PageNumber = 10 };
        const int customDefault = 5;

        // Act
        var result = request.GetPageNumberOrDefault(customDefault);

        // Assert
        result.Should().Be(request.PageNumber);
    }

    #endregion

    #region GetPageSizeOrDefault

    [Fact]
    public void GetPageSizeOrDefault_WithValidPageSize_ShouldReturnPageSize()
    {
        // Arrange
        var request = new FakePagableRequest { PageSize = 20 };

        // Act
        var result = request.GetPageSizeOrDefault();

        // Assert
        result.Should().Be(request.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-50)]
    public void GetPageSizeOrDefault_WithInvalidPageSize_ShouldReturnDefault(int invalidPageSize)
    {
        // Arrange
        var request = new FakePagableRequest { PageSize = invalidPageSize };

        // Act
        var result = request.GetPageSizeOrDefault();

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void GetPageSizeOrDefault_WithInvalidPageSizeAndCustomDefault_ShouldReturnCustomDefault()
    {
        // Arrange
        var request = new FakePagableRequest { PageSize = -5 };
        const int customDefault = 25;

        // Act
        var result = request.GetPageSizeOrDefault(customDefault);

        // Assert
        result.Should().Be(customDefault);
    }

    [Fact]
    public void GetPageSizeOrDefault_WithValidPageSizeAndCustomDefault_ShouldReturnPageSize()
    {
        // Arrange
        var request = new FakePagableRequest { PageSize = 50 };
        const int customDefault = 25;

        // Act
        var result = request.GetPageSizeOrDefault(customDefault);

        // Assert
        result.Should().Be(request.PageSize);
    }

    #endregion
}