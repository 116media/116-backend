using _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedVideos.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetPromotedVideosResponse"/>.
/// </summary>
public class PublicGetPromotedVideosEndpointV1Tests
{
    [Fact]
    public void PublicGetPromotedVideosResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<VideoSummaryDto> videos = [CreateVideoSummaryDto()];

        // Act
        var response = new PublicGetPromotedVideosResponse(Videos: videos);

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
            Status: EnumContentStatus.Published,
            YoutubeVideoUrl: null,
            IsPromoted: false,
            HasLyrics: false,
            PublishedAt: null,
            ShootingScheduledAt: null,
            ShareCount: 0,
            RatingAverage: 0m,
            RatingCount: 0
        );
}
