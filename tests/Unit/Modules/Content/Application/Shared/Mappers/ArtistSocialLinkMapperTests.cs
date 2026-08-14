using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="ArtistSocialLinkMapper"/>.
/// </summary>
public class ArtistSocialLinkMapperTests
{
    private static ArtistSocialLinkEntity Link(EnumSocialPlatform platform, string url) =>
        ArtistSocialLinkEntity.Create(Guid.NewGuid(), Guid.NewGuid(), platform, url);

    [Fact]
    public void ToArtistSocialLinkDto_ShouldCarryPlatformAndUrlOnly()
    {
        // Arrange
        ArtistSocialLinkEntity entity = Link(EnumSocialPlatform.Instagram, "https://instagram.com/fallyipupa01");

        // Act
        ArtistSocialLinkDto dto = entity.ToArtistSocialLinkDto();

        // Assert — no id, no artist id: the client renders the row and follows the URLs.
        dto.Platform.Should().Be(EnumSocialPlatform.Instagram);
        dto.Url.Should().Be("https://instagram.com/fallyipupa01");
    }

    [Fact]
    public void ToArtistSocialLinkDtoList_WithNull_ShouldCollapseToEmpty()
    {
        // A null and an empty list both mean "render nothing"; the boundary collapses them
        // so the client never handles two shapes for one state.
        IReadOnlyList<ArtistSocialLinkEntity>? entities = null;

        IReadOnlyList<ArtistSocialLinkDto> result = entities.ToArtistSocialLinkDtoList();

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToArtistSocialLinkDtoList_WithEmptyList_ShouldReturnEmpty()
    {
        IReadOnlyList<ArtistSocialLinkEntity> entities = [];

        entities.ToArtistSocialLinkDtoList().Should().BeEmpty();
    }

    [Fact]
    public void ToArtistSocialLinkDtoList_ShouldMapEveryEntryInOrder()
    {
        // Arrange
        IReadOnlyList<ArtistSocialLinkEntity> entities =
        [
            Link(EnumSocialPlatform.Instagram, "https://instagram.com/a"),
            Link(EnumSocialPlatform.Website, "https://example.com"),
        ];

        // Act
        IReadOnlyList<ArtistSocialLinkDto> result = entities.ToArtistSocialLinkDtoList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(l => l.Platform).Should().Equal(EnumSocialPlatform.Instagram, EnumSocialPlatform.Website);
        result.Select(l => l.Url).Should().Equal("https://instagram.com/a", "https://example.com");
    }
}
