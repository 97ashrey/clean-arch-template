using FluentAssertions;
using Company.Service.Application.Common.Types;

namespace Company.Service.Application.UnitTests.Common.Types;

public class PagedListTests
{
    [Fact]
    public void Constructor_WithValidData_SetsPropertiesCorrectly()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        const int currentPage = 1;
        const int pageSize = 10;
        const int totalCount = 25;

        // Act
        var pagedList = new PagedList<int>(items, currentPage, pageSize, totalCount);

        // Assert
        pagedList.CurrentPage.Should().Be(currentPage);
        pagedList.PageSize.Should().Be(pageSize);
        pagedList.TotalCount.Should().Be(totalCount);
    }

    [Fact]
    public void TotalPages_CalculatedCorrectly_WhenTotalCountDivisibleByPageSize()
    {
        // Arrange
        var items = new[] { 1, 2, 3, 4, 5 };
        const int pageSize = 5;
        const int totalCount = 20;

        // Act
        var pagedList = new PagedList<int>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.TotalPages.Should().Be(4);
    }

    [Fact]
    public void TotalPages_CalculatedCorrectly_WhenTotalCountNotDivisibleByPageSize()
    {
        // Arrange
        var items = new[] { 1, 2 };
        const int pageSize = 5;
        const int totalCount = 22;

        // Act
        var pagedList = new PagedList<int>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.TotalPages.Should().Be(5);
    }

    [Fact]
    public void TotalPages_EqualsOne_WhenTotalCountLessThanPageSize()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        const int pageSize = 10;
        const int totalCount = 3;

        // Act
        var pagedList = new PagedList<int>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.TotalPages.Should().Be(1);
    }

    [Fact]
    public void CurrentPageCount_ReturnsItemsLength()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        const int pageSize = 10;
        const int totalCount = 25;

        // Act
        var pagedList = new PagedList<int>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.CurrentPageCount.Should().Be(3);
    }

    [Fact]
    public void CurrentPageCount_ReturnsZero_WhenItemsEmpty()
    {
        // Arrange
        var items = Array.Empty<int>();
        const int pageSize = 10;
        const int totalCount = 0;

        // Act
        var pagedList = new PagedList<int>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.CurrentPageCount.Should().Be(0);
    }

    [Fact]
    public void Items_ReturnsReadOnlyCollection()
    {
        // Arrange
        var items = new[] { 1, 2, 3 };
        const int pageSize = 10;
        const int totalCount = 3;

        // Act
        var pagedList = new PagedList<int>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.Items.Should().BeOfType<System.Collections.ObjectModel.ReadOnlyCollection<int>>();
        pagedList.Items.Should().HaveCount(3);
        pagedList.Items.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void Items_IsEmpty_WhenConstructedWithEmptyArray()
    {
        // Arrange
        var items = Array.Empty<string>();
        const int pageSize = 10;
        const int totalCount = 0;

        // Act
        var pagedList = new PagedList<string>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.Items.Should().BeEmpty();
    }

    [Fact]
    public void PagedList_WorksWithComplexTypes()
    {
        // Arrange
        var items = new[]
        {
            new { Id = 1, Name = "Item1" },
            new { Id = 2, Name = "Item2" }
        };
        const int pageSize = 10;
        const int totalCount = 2;

        // Act
        var pagedList = new PagedList<dynamic>(items, 1, pageSize, totalCount);

        // Assert
        pagedList.Items.Should().HaveCount(2);
        pagedList.CurrentPageCount.Should().Be(2);
    }

    [Fact]
    public void MultiplePages_PropertiesSetCorrectly()
    {
        // Arrange
        var itemsPage2 = new[] { 11, 12, 13, 14, 15 };
        const int currentPage = 2;
        const int pageSize = 5;
        const int totalCount = 27;

        // Act
        var pagedList = new PagedList<int>(itemsPage2, currentPage, pageSize, totalCount);

        // Assert
        pagedList.CurrentPage.Should().Be(2);
        pagedList.CurrentPageCount.Should().Be(5);
        pagedList.PageSize.Should().Be(5);
        pagedList.TotalCount.Should().Be(27);
        pagedList.TotalPages.Should().Be(6);
    }

    [Fact]
    public void LastPage_WithPartialData_PropertiesSetCorrectly()
    {
        // Arrange
        var itemsLastPage = new[] { 26, 27 };
        const int currentPage = 6;
        const int pageSize = 5;
        const int totalCount = 27;

        // Act
        var pagedList = new PagedList<int>(itemsLastPage, currentPage, pageSize, totalCount);

        // Assert
        pagedList.CurrentPage.Should().Be(6);
        pagedList.CurrentPageCount.Should().Be(2);
        pagedList.TotalPages.Should().Be(6);
    }
}
