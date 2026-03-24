using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetVideoByIdResponse"/>.
/// </summary>
public class AdminGetVideoByIdEndpointV1Tests
{
    [Fact]
    public void AdminGetVideoByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        VideoDetailDto dto = CreateVideoDetailDto();

        // Act
        var response = new AdminGetVideoByIdResponse(Video: dto);

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
            Status: EnumContentStatus.Draft,
            RejectionReason: null,
            YoutubeVideoId: null,
            IsFeatured: false,
            FeaturedUntil: null,
            HasLyrics: false,
            ShootingScheduledAt: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Tags: []
        );
}
