using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideos.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetAllVideos.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetAllVideosResponse"/>.
/// </summary>
public class AdminGetAllVideosEndpointV1Tests
{
    [Fact]
    public void AdminGetAllVideosResponse_ShouldConstructCorrectly()
    {
        // Arrange
        var paginated = new PaginatedResult<VideoSummaryDto>(1, 10, 1, [CreateVideoSummaryDto()]);

        // Act
        var response = new AdminGetAllVideosResponse(Videos: paginated);

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
            Status: "Draft",
            YoutubeVideoId: null,
            IsFeatured: false,
            HasLyrics: false,
            PublishedAt: null,
            CreatedAt: null
        );
}
