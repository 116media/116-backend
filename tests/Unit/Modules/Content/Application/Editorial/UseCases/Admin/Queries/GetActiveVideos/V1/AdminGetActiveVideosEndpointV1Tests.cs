using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetActiveVideosResponse"/>.
/// </summary>
public class AdminGetActiveVideosEndpointV1Tests
{
    [Fact]
    public void AdminGetActiveVideosResponse_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<VideoSummaryDto> videos = [CreateVideoSummaryDto()];

        // Act
        var response = new AdminGetActiveVideosResponse(Videos: videos);

        // Assert
        response.Should().NotBeNull();
        response.Videos.Should().BeSameAs(videos);
    }

    [Fact]
    public void AdminGetActiveVideosResponse_WithEmptyList_ShouldConstructCorrectly()
    {
        // Arrange
        IReadOnlyList<VideoSummaryDto> videos = [];

        // Act
        var response = new AdminGetActiveVideosResponse(Videos: videos);

        // Assert
        response.Should().NotBeNull();
        response.Videos.Should().BeEmpty();
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
            Status: EnumContentStatus.Draft,
            YoutubeVideoUrl: null,
            IsPromoted: false,
            HasLyrics: false,
            PublishedAt: null,
            ShootingScheduledAt: null
        );
}
