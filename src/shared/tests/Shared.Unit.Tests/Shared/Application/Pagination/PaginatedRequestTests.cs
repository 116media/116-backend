using _116.BuildingBlocks.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Shared.Unit.Tests.Shared.Application.Pagination;

/// <summary>
/// Unit tests for <see cref="PaginatedRequest"/>, including the constructor-enforced bounds:
/// page index is clamped to [0, <see cref="PaginatedRequest.MaxPageIndex"/>] and page size to
/// [1, <see cref="PaginatedRequest.MaxPageSize"/>], so <see cref="PaginatedRequest.Skip"/> never overflows.
/// </summary>
public class PaginatedRequestTests
{
    #region Default Values Tests

    [Fact]
    public void Constructor_WithNoParameters_ShouldUseDefaultValues()
    {
        PaginatedRequest request = new();

        request.PageIndex.Should().Be(0);
        request.PageSize.Should().Be(10);
    }

    #endregion

    #region Custom Values Within Bounds

    [Fact]
    public void Constructor_WithCustomPageIndex_ShouldSetPageIndex()
    {
        PaginatedRequest request = new(pageIndex: 5);

        request.PageIndex.Should().Be(5);
    }

    [Fact]
    public void Constructor_WithPageSizeWithinBounds_ShouldSetPageSize()
    {
        PaginatedRequest request = new(pageSize: 25);

        request.PageSize.Should().Be(25);
    }

    [Fact]
    public void Constructor_WithBothValuesWithinBounds_ShouldSetBoth()
    {
        PaginatedRequest request = new(3, 50);

        request.PageIndex.Should().Be(3);
        request.PageSize.Should().Be(50);
    }

    #endregion

    #region Bounds Enforcement

    [Fact]
    public void MaxPageSize_ShouldBe100()
    {
        PaginatedRequest.MaxPageSize.Should().Be(100);
    }

    [Fact]
    public void MaxPageIndex_ShouldBe10000()
    {
        PaginatedRequest.MaxPageIndex.Should().Be(10_000);
    }

    [Fact]
    public void Constructor_WithPageIndexAboveMax_ShouldClampToMax()
    {
        PaginatedRequest request = new(pageIndex: int.MaxValue);

        request.PageIndex.Should().Be(PaginatedRequest.MaxPageIndex, "an over-deep page index is clamped");
    }

    [Fact]
    public void Constructor_WithPageIndexAtMax_ShouldKeepMax()
    {
        PaginatedRequest request = new(pageIndex: PaginatedRequest.MaxPageIndex);

        request.PageIndex.Should().Be(PaginatedRequest.MaxPageIndex);
    }

    [Theory]
    [InlineData(0, 20, 0)]
    [InlineData(3, 20, 60)]
    [InlineData(PaginatedRequest.MaxPageIndex, PaginatedRequest.MaxPageSize, 1_000_000)]
    public void Skip_ShouldBeTheRowsPrecedingThePage(int pageIndex, int pageSize, int expected)
    {
        PaginatedRequest request = new(pageIndex, pageSize);

        request.Skip.Should().Be(expected);
    }

    [Theory]
    [InlineData(30_000_000)]
    [InlineData(100_000_000)]
    [InlineData(int.MaxValue)]
    public void Skip_WithAnOverflowingPageIndex_ShouldStayPositive(int pageIndex)
    {
        PaginatedRequest request = new(pageIndex, PaginatedRequest.MaxPageSize);

        request.Skip.Should().BePositive("an unclamped index would wrap the multiplication negative");
    }

    [Fact]
    public void Constructor_WithNegativePageIndex_ShouldFloorToZero()
    {
        PaginatedRequest request = new(pageIndex: -1);

        request.PageIndex.Should().Be(0, "a negative page index is floored to 0");
    }

    [Fact]
    public void Constructor_WithZeroPageSize_ShouldClampToOne()
    {
        PaginatedRequest request = new(pageSize: 0);

        request.PageSize.Should().Be(1, "page size is clamped to a minimum of 1");
    }

    [Fact]
    public void Constructor_WithNegativePageSize_ShouldClampToOne()
    {
        PaginatedRequest request = new(pageSize: -5);

        request.PageSize.Should().Be(1);
    }

    [Fact]
    public void Constructor_WithPageSizeAboveMax_ShouldClampToMax()
    {
        PaginatedRequest request = new(pageSize: 1_000_000);

        request.PageSize.Should().Be(PaginatedRequest.MaxPageSize, "an over-large page size is clamped to the max");
    }

    [Fact]
    public void Constructor_WithPageSizeAtMax_ShouldKeepMax()
    {
        PaginatedRequest request = new(pageSize: PaginatedRequest.MaxPageSize);

        request.PageSize.Should().Be(PaginatedRequest.MaxPageSize);
    }

    #endregion

    #region Record Behavior Tests

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        PaginatedRequest request1 = new(2, 20);
        PaginatedRequest request2 = new(2, 20);

        request1.Equals(request2).Should().BeTrue("records with same values should be equal");
    }

    [Fact]
    public void Equals_WithDifferentPageIndex_ShouldReturnFalse()
    {
        PaginatedRequest request1 = new(1, 20);
        PaginatedRequest request2 = new(2, 20);

        request1.Equals(request2).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithDifferentPageSize_ShouldReturnFalse()
    {
        PaginatedRequest request1 = new(2, 10);
        PaginatedRequest request2 = new(2, 20);

        request1.Equals(request2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        PaginatedRequest request1 = new(3, 15);
        PaginatedRequest request2 = new(3, 15);

        request1.GetHashCode().Should().Be(request2.GetHashCode());
    }

    #endregion
}
