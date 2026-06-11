using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetExclusiveCategoryResponse"/>.
/// </summary>
public class PublicGetExclusiveCategoryEndpointV1Tests
{
    [Fact]
    public void PublicGetExclusiveCategoryResponse_ShouldConstructCorrectly()
    {
        // Arrange
        CategoryDto category = CreateCategoryDto();
        var videos = new PaginatedResult<VideoSummaryDto>(
            pageIndex: 0,
            pageSize: 10,
            count: 1,
            items: [CreateVideoSummaryDto()]
        );

        // Act
        var response = new PublicGetExclusiveCategoryResponse(Category: category, Videos: videos);

        // Assert
        response.Category.Should().NotBeNull();
        response.Category.IsExclusive.Should().BeTrue();
        response.Videos.Should().NotBeNull();
        response.Videos.Items.Should().ContainSingle();
    }

    private static CategoryDto CreateCategoryDto() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Video",
            "Exclusive Show",
            "exclusive-show",
            "The exclusive video category",
            false,
            true,
            false,
            true,
            "https://cloudinary.com/poster.jpg",
            []
        );

    private static VideoSummaryDto CreateVideoSummaryDto() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Exclusive Show",
            "Test Video",
            "test-video",
            "https://cloudinary.com/thumbnail.jpg",
            Guid.NewGuid().ToString(),
            EnumContentStatus.Published,
            "https://youtube.com/watch?v=test",
            false,
            false,
            DateTimeOffset.UtcNow,
            null,
            0,
            0m,
            0
        );
}
