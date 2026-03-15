using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Pagination;

/// <summary>
/// Unit tests for <see cref="PaginatedResult{TEntity}"/>.
/// </summary>
public class PaginatedResultTests
{
    #region Test Models

    private class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidParameters_ShouldSetAllProperties()
    {
        // Arrange
        int pageIndex = 2;
        int pageSize = 20;
        long count = 100;
        List<TestEntity> items = [new() { Id = 1, Name = "Item 1" }, new() { Id = 2, Name = "Item 2" }];

        // Act
        PaginatedResult<TestEntity> result = new(pageIndex, pageSize, count, items);

        // Assert
        result.PageIndex.Should().Be(pageIndex);
        result.PageSize.Should().Be(pageSize);
        result.Count.Should().Be(count);
        result.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void Constructor_WithZeroPageIndex_ShouldAcceptValue()
    {
        // Arrange
        List<TestEntity> items = [new() { Id = 1, Name = "Item 1" }];

        // Act
        PaginatedResult<TestEntity> result = new(0, 10, 1, items);

        // Assert
        result.PageIndex.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithEmptyItems_ShouldAcceptEmptyList()
    {
        // Arrange
        List<TestEntity> emptyItems = [];

        // Act
        PaginatedResult<TestEntity> result = new(0, 10, 0, emptyItems);

        // Assert
        result.Items.Should().BeEmpty();
        result.Count.Should().Be(0);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void PageIndex_ShouldReturnConstructorValue()
    {
        // Arrange
        int expectedPageIndex = 5;
        PaginatedResult<TestEntity> result = new(expectedPageIndex, 10, 100, []);

        // Act
        int actualPageIndex = result.PageIndex;

        // Assert
        actualPageIndex.Should().Be(expectedPageIndex);
    }

    [Fact]
    public void PageSize_ShouldReturnConstructorValue()
    {
        // Arrange
        int expectedPageSize = 25;
        PaginatedResult<TestEntity> result = new(0, expectedPageSize, 100, []);

        // Act
        int actualPageSize = result.PageSize;

        // Assert
        actualPageSize.Should().Be(expectedPageSize);
    }

    [Fact]
    public void Count_ShouldReturnConstructorValue()
    {
        // Arrange
        long expectedCount = 500;
        PaginatedResult<TestEntity> result = new(0, 10, expectedCount, []);

        // Act
        long actualCount = result.Count;

        // Assert
        actualCount.Should().Be(expectedCount);
    }

    [Fact]
    public void Items_ShouldReturnConstructorCollection()
    {
        // Arrange
        List<TestEntity> expectedItems =
        [
            new() { Id = 1, Name = "Item 1" },
            new() { Id = 2, Name = "Item 2" },
            new() { Id = 3, Name = "Item 3" },
        ];
        PaginatedResult<TestEntity> result = new(0, 10, 3, expectedItems);

        // Act
        IEnumerable<TestEntity> actualItems = result.Items;

        // Assert
        actualItems.Should().BeEquivalentTo(expectedItems);
    }

    #endregion

    #region Pagination Calculation Tests

    [Fact]
    public void Constructor_FirstPage_ShouldHavePageIndexZero()
    {
        // Arrange
        List<TestEntity> items = [new() { Id = 1, Name = "Item 1" }];

        // Act
        PaginatedResult<TestEntity> result = new(0, 10, 100, items);

        // Assert
        result.PageIndex.Should().Be(0, "first page should have index 0");
    }

    [Fact]
    public void Constructor_WithCountGreaterThanPageSize_ShouldIndicateMorePages()
    {
        // Arrange
        int pageSize = 10;
        long totalCount = 25;
        List<TestEntity> items = Enumerable
            .Range(1, pageSize)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        // Act
        PaginatedResult<TestEntity> result = new(0, pageSize, totalCount, items);

        // Assert
        result.Count.Should().BeGreaterThan(result.PageSize, "should indicate more pages exist");
    }

    [Fact]
    public void Constructor_WithCountEqualToPageSize_ShouldIndicateSinglePage()
    {
        // Arrange
        int pageSize = 10;
        long totalCount = 10;
        List<TestEntity> items = Enumerable
            .Range(1, pageSize)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        // Act
        PaginatedResult<TestEntity> result = new(0, pageSize, totalCount, items);

        // Assert
        result.Count.Should().Be(result.PageSize, "should indicate single page");
    }

    [Fact]
    public void Constructor_WithCountLessThanPageSize_ShouldAcceptValue()
    {
        // Arrange
        int pageSize = 10;
        long totalCount = 5;
        List<TestEntity> items = Enumerable
            .Range(1, 5)
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        // Act
        PaginatedResult<TestEntity> result = new(0, pageSize, totalCount, items);

        // Assert
        result.Count.Should().BeLessThan(result.PageSize);
    }

    #endregion

    #region Boundary Tests

    [Fact]
    public void Constructor_WithLargeCount_ShouldAcceptValue()
    {
        // Arrange
        long largeCount = long.MaxValue;

        // Act
        PaginatedResult<TestEntity> result = new(0, 10, largeCount, []);

        // Assert
        result.Count.Should().Be(largeCount);
    }

    [Fact]
    public void Constructor_WithNegativePageIndex_ShouldAcceptValue()
    {
        // Arrange & Act
        PaginatedResult<TestEntity> result = new(-1, 10, 100, []);

        // Assert
        result.PageIndex.Should().Be(-1);
    }

    [Fact]
    public void Constructor_WithZeroPageSize_ShouldAcceptValue()
    {
        // Arrange & Act
        PaginatedResult<TestEntity> result = new(0, 0, 0, []);

        // Assert
        result.PageSize.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithMaxPageSize_ShouldAcceptValue()
    {
        // Arrange
        int maxPageSize = int.MaxValue;

        // Act
        PaginatedResult<TestEntity> result = new(0, maxPageSize, 100, []);

        // Assert
        result.PageSize.Should().Be(maxPageSize);
    }

    #endregion

    #region Different Entity Types Tests

    [Fact]
    public void Constructor_WithStringEntity_ShouldWork()
    {
        // Arrange
        List<string> items = ["item1", "item2", "item3"];

        // Act
        PaginatedResult<string> result = new(0, 10, 3, items);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items.Should().Contain("item1");
    }

    private record TestRecord(int Id, string Name);

    [Fact]
    public void Constructor_WithRecordEntity_ShouldWork()
    {
        // Arrange
        List<TestRecord> items = [new(1, "Record 1"), new(2, "Record 2")];

        // Act
        PaginatedResult<TestRecord> result = new(0, 10, 2, items);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.First().Name.Should().Be("Record 1");
    }

    #endregion

    #region Real-World Scenario Tests

    [Fact]
    public void Constructor_SecondPageOfThreePages_ShouldHaveCorrectValues()
    {
        // Arrange - Simulating page 2 of 3 pages (total 25 items, 10 per page)
        int pageIndex = 1; // Zero-based, so page 2
        int pageSize = 10;
        long totalCount = 25;
        List<TestEntity> pageItems = Enumerable
            .Range(11, 10) // Items 11-20
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        // Act
        PaginatedResult<TestEntity> result = new(pageIndex, pageSize, totalCount, pageItems);

        // Assert
        result.PageIndex.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Count.Should().Be(25);
        result.Items.Should().HaveCount(10);
        result.Items.First().Id.Should().Be(11);
        result.Items.Last().Id.Should().Be(20);
    }

    [Fact]
    public void Constructor_LastPageWithPartialItems_ShouldHaveCorrectValues()
    {
        // Arrange - Simulating last page with only 5 items (total 25 items, 10 per page)
        int pageIndex = 2; // Page 3 (zero-based)
        int pageSize = 10;
        long totalCount = 25;
        List<TestEntity> pageItems = Enumerable
            .Range(21, 5) // Items 21-25
            .Select(i => new TestEntity { Id = i, Name = $"Item {i}" })
            .ToList();

        // Act
        PaginatedResult<TestEntity> result = new(pageIndex, pageSize, totalCount, pageItems);

        // Assert
        result.PageIndex.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Count.Should().Be(25);
        result.Items.Should().HaveCount(5, "last page should have only remaining items");
        result.Items.First().Id.Should().Be(21);
        result.Items.Last().Id.Should().Be(25);
    }

    #endregion
}
