using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking a lyrics page to a tag (many-to-many).
/// </summary>
public class LyricsTagEntity : Entity<Guid>
{
    /// <summary>
    /// The identifier of the lyrics page.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; private set; }

    private LyricsTagEntity() { }

    /// <summary>
    /// Creates a new lyrics-tag association.
    /// </summary>
    /// <param name="id">The unique identifier for this association.</param>
    /// <param name="lyricsId">The lyrics page being tagged.</param>
    /// <param name="tagId">The tag being applied.</param>
    /// <returns>A new <see cref="LyricsTagEntity" />.</returns>
    internal static LyricsTagEntity Create(Guid id, Guid lyricsId, Guid tagId)
    {
        var association = new LyricsTagEntity
        {
            Id = id,
            LyricsId = lyricsId,
            TagId = tagId,
        };

        return association;
    }
}
