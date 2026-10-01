namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="ArtistSocialLinkEntity" />. Its state lives in <c>Entities/ArtistSocialLinkEntity.cs</c>.
/// </summary>
public partial class ArtistSocialLinkEntity
{
    /// <summary>
    /// Replaces the URL for this platform slot.
    /// </summary>
    /// <param name="url">The new outbound profile URL.</param>
    internal void UpdateUrl(string url) => Url = url;
}
