using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllShorts.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllShortsResponse"/>.
/// </summary>
public class AdminGetAllShortsEndpointV1Tests
{
    [Fact]
    public void AdminGetAllShortsResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<ShortVideoDto>(1, 10, 1, [CreateShortVideoDto()]);

        // Act
        var response = new AdminGetAllShortsResponse(ShortVideos: paginated);

        // Assert
        response.Should().NotBeNull();
        response.ShortVideos.Should().Be(paginated);
    }

    private static ShortVideoDto CreateShortVideoDto() =>
        new(
            Id: Guid.NewGuid(),
            Title: "Test",
            Slug: "test",
            VideoUrl: "Test",
            ThumbnailUrl: null,
            HasFullVideo: false,
            IsActive: false,
            ViewCount: 0,
            LikeCount: 0,
            ShareCount: 0,
            BookmarkCount: 0
        );
}
