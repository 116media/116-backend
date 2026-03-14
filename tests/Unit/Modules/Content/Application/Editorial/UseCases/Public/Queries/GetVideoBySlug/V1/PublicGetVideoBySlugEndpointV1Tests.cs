using _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug.V1;

/// <summary>
/// Unit tests for <see cref="PublicGetVideoBySlugResponse"/>.
/// </summary>
public class PublicGetVideoBySlugEndpointV1Tests
{
    [Fact]
    public void PublicGetVideoBySlugResponse_ShouldConstructCorrectly()
    {
        // Arrange
        VideoDetailDto dto = CreateVideoDetailDto();

        // Act
        var response = new PublicGetVideoBySlugResponse(Video: dto);

        // Assert
        response.Should().NotBeNull();
        response.Video.Should().Be(dto);
    }

    private static VideoDetailDto CreateVideoDetailDto() =>
        new(
            Id: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            CategoryName: "Test",
            Title: "Test",
            Slug: "test",
            Description: null,
            ThumbnailUrl: null,
            ThumbnailStorageKey: null,
            AuthorId: "Test",
            Status: "Published",
            RejectionReason: null,
            YoutubeVideoId: null,
            IsFeatured: false,
            FeaturedUntil: null,
            HasLyrics: false,
            ShootingScheduledAt: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Tags: [],
            CreatedAt: null,
            UpdatedAt: null
        );
}
