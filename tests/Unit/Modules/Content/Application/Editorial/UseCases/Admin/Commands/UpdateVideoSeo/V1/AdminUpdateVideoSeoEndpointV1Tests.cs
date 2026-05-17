using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoSeo.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateVideoSeoResponse"/>.
/// </summary>
public class AdminUpdateVideoSeoEndpointV1Tests
{
    [Fact]
    public void AdminUpdateVideoSeoResponse_ShouldConstructCorrectly()
    {
        // Arrange
        VideoDetailDto dto = CreateVideoDetailDto();

        // Act
        var response = new AdminUpdateVideoSeoResponse(Video: dto);

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
            Description: "Test video description",
            ThumbnailUrl: null,
            ThumbnailStorageKey: null,
            AuthorId: "Test",
            Status: EnumContentStatus.Draft,
            RejectionReason: null,
            YoutubeVideoUrl: null,
            SocialBoost: false,
            IsPromoted: false,
            PromotedUntil: null,
            PromotionLevelId: null,
            PromotionLevelName: null,
            HasLyrics: false,
            ShootingScheduledAt: null,
            PublishedAt: null,
            MetaTitle: null,
            MetaDescription: null,
            Tags: [],
            ShareCount: 0,
            RatingAverage: 0m,
            RatingCount: 0
        );
}
