namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="StreamingLinkEntity" />. Its state lives in <c>Entities/StreamingLinkEntity.cs</c>.
/// </summary>
public partial class StreamingLinkEntity
{
    /// <summary>
    /// Replaces the curated deep link URL for this platform slot.
    /// </summary>
    /// <param name="url">The new curated deep link URL.</param>
    public void UpdateUrl(string url) => Url = url;
}
