using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById.V1;
using _116.Content.Application.Shared.DTOs;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetShortById.V1;

/// <summary>
/// Unit tests for <see cref="AdminGetShortByIdResponse"/>.
/// </summary>
public class AdminGetShortByIdEndpointV1Tests
{
    [Fact]
    public void AdminGetShortByIdResponse_ShouldConstructCorrectly()
    {
        // Arrange
        ShortVideoDto dto = CreateShortVideoDto();

        // Act
        var response = new AdminGetShortByIdResponse(ShortVideo: dto);

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
            BookmarkCount: 0,
            CreatedAt: null
        );
}
