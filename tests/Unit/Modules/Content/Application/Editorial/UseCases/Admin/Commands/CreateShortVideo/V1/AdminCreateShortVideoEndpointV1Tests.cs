using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateShortVideo.V1;

/// <summary>
/// Unit tests for <see cref="AdminCreateShortVideoResponse"/>.
/// </summary>
public class AdminCreateShortVideoEndpointV1Tests
{
    [Fact]
    public void AdminCreateShortVideoResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ShortVideoDto dto = CreateShortVideoDto();

        // Act
        var response = new AdminCreateShortVideoResponse(ShortVideo: dto);

        // Assert
        response.Should().NotBeNull();
        response.ShortVideo.Should().Be(dto);
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
