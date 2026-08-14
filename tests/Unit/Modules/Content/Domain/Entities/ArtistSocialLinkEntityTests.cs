using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArtistSocialLinkEntity"/>.
/// </summary>
public class ArtistSocialLinkEntityTests
{
    [Fact]
    public void Create_ShouldStoreArtistPlatformAndUrl()
    {
        // Arrange
        var id = Guid.NewGuid();
        var artistId = Guid.NewGuid();
        const string url = "https://instagram.com/fallyipupa01";

        // Act
        ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(id, artistId, EnumSocialPlatform.Instagram, url);

        // Assert
        link.Id.Should().Be(id);
        link.ArtistId.Should().Be(artistId);
        link.Platform.Should().Be(EnumSocialPlatform.Instagram);
        link.Url.Should().Be(url);
    }

    [Fact]
    public void UpdateUrl_ShouldReplaceTheUrlOnly()
    {
        // Arrange
        ArtistSocialLinkEntity link = ArtistSocialLinkEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnumSocialPlatform.YouTube,
            "https://youtube.com/@old"
        );

        // Act
        link.UpdateUrl("https://youtube.com/@new");

        // Assert
        link.Url.Should().Be("https://youtube.com/@new");
        link.Platform.Should().Be(EnumSocialPlatform.YouTube);
    }
}
