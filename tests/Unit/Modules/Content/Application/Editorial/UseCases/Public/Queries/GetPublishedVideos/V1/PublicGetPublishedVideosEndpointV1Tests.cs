using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedVideos.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetPublishedVideosResponse"/>.
/// </summary>
public class PublicGetPublishedVideosEndpointV1Tests
{
    [Fact]
    public void PublicGetPublishedVideosResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<VideoSummaryDto>(1, 10, 1, [CreateVideoSummaryDto()]);

        // Act
        var response = new PublicGetPublishedVideosResponse(Videos: paginated);

        // Assert
        response.Should().NotBeNull();
        response.Videos.Should().Be(paginated);
    }

    private static VideoSummaryDto CreateVideoSummaryDto() =>
        new(
            Id: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            CategoryName: "Test",
            Title: "Test",
            Slug: "test",
            ThumbnailUrl: null,
            AuthorId: "Test",
            Status: "Published",
            YoutubeVideoId: null,
            IsFeatured: false,
            HasLyrics: false,
            PublishedAt: null,
            CreatedAt: null
        );
}
