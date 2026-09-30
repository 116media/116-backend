using _116.Content.Domain.Enums;

namespace _116.Content.Domain.Entities;

/// <summary>
/// SocialLinks behaviour of <see cref="ArtistEntity" />. Its state lives in <c>Entities/ArtistEntity.cs</c>.
/// </summary>
public sealed partial class ArtistEntity
{
    /// <summary>
    /// Adds or replaces the link for a platform, reporting false when the stored URL already
    /// matches so an unchanged upsert writes nothing.
    /// </summary>
    /// <param name="platform">The social platform the link points to.</param>
    /// <param name="url">The outbound profile URL.</param>
    /// <returns><c>true</c> if a link was added or its URL changed; otherwise <c>false</c>.</returns>
    public bool SetSocialLink(EnumSocialPlatform platform, string url)
    {
        ArtistSocialLinkEntity? existing = FindSocialLink(platform: platform);

        if (existing is null)
        {
            SocialLinks.Add(
                ArtistSocialLinkEntity.Create(id: Guid.NewGuid(), artistId: Id, platform: platform, url: url)
            );

            return true;
        }

        if (existing.Url == url)
        {
            return false;
        }

        existing.UpdateUrl(url: url);

        return true;
    }

    /// <summary>
    /// Removes the link for a platform, reporting whether one was there.
    /// </summary>
    /// <param name="platform">The social platform whose link is removed.</param>
    /// <returns><c>true</c> if a link was removed; otherwise <c>false</c>.</returns>
    public bool RemoveSocialLink(EnumSocialPlatform platform)
    {
        ArtistSocialLinkEntity? existing = FindSocialLink(platform: platform);

        if (existing is null)
        {
            return false;
        }

        SocialLinks.Remove(existing);

        return true;
    }

    /// <summary>
    /// Returns this artist's link for a platform, or null when the slot is empty.
    /// </summary>
    /// <param name="platform">The social platform to look up.</param>
    /// <returns>The matching link, or <c>null</c>.</returns>
    public ArtistSocialLinkEntity? FindSocialLink(EnumSocialPlatform platform)
    {
        return SocialLinks.FirstOrDefault(link => link.Platform == platform);
    }
}
