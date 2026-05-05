using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;
using _116.Content.Application.Shared.DTOs;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminUpdateShortVideoResponse"/>.
/// </summary>
public class AdminUpdateShortVideoEndpointV1Tests
{
    [Fact]
    public void AdminUpdateShortVideoResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ShortVideoDto dto = CreateShortVideoDto();

        // Act
        var response = new AdminUpdateShortVideoResponse(ShortVideo: dto);

        // Assert
        response.Should().NotBeNull();
        response.ShortVideo.Should().Be(dto);
        response.ShortVideo.Title.Should().Be(dto.Title);
        response.ShortVideo.VideoUrl.Should().Be(dto.VideoUrl);
    }

    private static ShortVideoDto CreateShortVideoDto() =>
        new(
            Id: Guid.NewGuid(),
            Title: TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            Slug: TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            VideoUrl: "https://res.cloudinary.com/test/video/upload/v1/clip.mp4",
            ThumbnailUrl: null,
            VideoId: null,
            HasFullVideo: false,
            IsActive: true,
            ViewCount: 0,
            LikeCount: 0,
            ShareCount: 0,
            BookmarkCount: 0,
            AuthorId: Guid.NewGuid().ToString()
        );
}
