using _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetFeaturedVideosResponse"/>.
/// </summary>
public class PublicGetFeaturedVideosEndpointV1Tests
{
    [Fact]
    public void PublicGetFeaturedVideosResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<VideoSummaryDto> videos = [CreateVideoSummaryDto()];

        // Act
        var response = new PublicGetFeaturedVideosResponse(Videos: videos);

        // Assert
        response.Should().NotBeNull();
        response.Videos.Should().BeSameAs(videos);
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
            PublishedAt: null
        );
}
