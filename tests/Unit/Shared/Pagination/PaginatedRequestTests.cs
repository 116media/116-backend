using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Pagination;

/// <summary>
/// Unit tests for <see cref="PaginatedRequest"/>.
/// </summary>
public class PaginatedRequestTests
{
    #region Default Values Tests

    [Fact]
    public void Constructor_WithNoParameters_ShouldUseDefaultValues()
    {
        // Arrange & Act
        PaginatedRequest request = new();

        // Assert
        request.PageIndex.Should().Be(0);
        request.PageSize.Should().Be(10);
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldHaveZeroBasedPageIndex()
    {
        // Arrange & Act
        PaginatedRequest request = new();

        // Assert
        request.PageIndex.Should().Be(0, "page index should be zero-based by default");
    }

    [Fact]
    public void Constructor_WithDefaultParameters_ShouldHavePageSizeOfTen()
    {
        // Arrange & Act
        PaginatedRequest request = new();

        // Assert
        request.PageSize.Should().Be(10, "default page size should be 10");
    }

    #endregion

    #region Custom Values Tests

    [Fact]
    public void Constructor_WithCustomPageIndex_ShouldSetPageIndex()
    {
        // Arrange
        int customPageIndex = 5;

        // Act
        PaginatedRequest request = new(PageIndex: customPageIndex);

        // Assert
        request.PageIndex.Should().Be(customPageIndex);
    }

    [Fact]
    public void Constructor_WithCustomPageSize_ShouldSetPageSize()
    {
        // Arrange
        int customPageSize = 25;

        // Act
        PaginatedRequest request = new(PageSize: customPageSize);

        // Assert
        request.PageSize.Should().Be(customPageSize);
    }

    [Fact]
    public void Constructor_WithCustomPageIndexAndPageSize_ShouldSetBothValues()
    {
        // Arrange
        int customPageIndex = 3;
        int customPageSize = 50;

        // Act
        PaginatedRequest request = new(PageIndex: customPageIndex, PageSize: customPageSize);

        // Assert
        request.PageIndex.Should().Be(customPageIndex);
        request.PageSize.Should().Be(customPageSize);
    }

    #endregion

    #region Boundary Values Tests

    [Fact]
    public void Constructor_WithZeroPageIndex_ShouldAllowZero()
    {
        // Arrange & Act
        PaginatedRequest request = new(PageIndex: 0);

        // Assert
        request.PageIndex.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithLargePageIndex_ShouldAcceptValue()
    {
        // Arrange
        int largePageIndex = int.MaxValue;

        // Act
        PaginatedRequest request = new(PageIndex: largePageIndex);

        // Assert
        request.PageIndex.Should().Be(largePageIndex);
    }

    [Fact]
    public void Constructor_WithNegativePageIndex_ShouldAcceptValue()
    {
        // Arrange
        int negativePageIndex = -1;

        // Act
        PaginatedRequest request = new(PageIndex: negativePageIndex);

        // Assert
        request.PageIndex.Should().Be(negativePageIndex);
    }

    [Fact]
    public void Constructor_WithOnePageSize_ShouldAcceptValue()
    {
        // Arrange & Act
        PaginatedRequest request = new(PageSize: 1);

        // Assert
        request.PageSize.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithLargePageSize_ShouldAcceptValue()
    {
        // Arrange
        int largePageSize = 1000;

        // Act
        PaginatedRequest request = new(PageSize: largePageSize);

        // Assert
        request.PageSize.Should().Be(largePageSize);
    }

    [Fact]
    public void Constructor_WithZeroPageSize_ShouldAcceptValue()
    {
        // Arrange & Act
        PaginatedRequest request = new(PageSize: 0);

        // Assert
        request.PageSize.Should().Be(0);
    }

    #endregion

    #region Record Behavior Tests

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        PaginatedRequest request1 = new(PageIndex: 2, PageSize: 20);
        PaginatedRequest request2 = new(PageIndex: 2, PageSize: 20);

        // Act
        bool areEqual = request1.Equals(request2);

        // Assert
        areEqual.Should().BeTrue("records with same values should be equal");
    }

    [Fact]
    public void Equals_WithDifferentPageIndex_ShouldReturnFalse()
    {
        // Arrange
        PaginatedRequest request1 = new(PageIndex: 1, PageSize: 20);
        PaginatedRequest request2 = new(PageIndex: 2, PageSize: 20);

        // Act
        bool areEqual = request1.Equals(request2);

        // Assert
        areEqual.Should().BeFalse("records with different page index should not be equal");
    }

    [Fact]
    public void Equals_WithDifferentPageSize_ShouldReturnFalse()
    {
        // Arrange
        PaginatedRequest request1 = new(PageIndex: 2, PageSize: 10);
        PaginatedRequest request2 = new(PageIndex: 2, PageSize: 20);

        // Act
        bool areEqual = request1.Equals(request2);

        // Assert
        areEqual.Should().BeFalse("records with different page size should not be equal");
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        PaginatedRequest request1 = new(PageIndex: 3, PageSize: 15);
        PaginatedRequest request2 = new(PageIndex: 3, PageSize: 15);

        // Act
        int hashCode1 = request1.GetHashCode();
        int hashCode2 = request2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2, "same values should produce same hash code");
    }

    #endregion
}
