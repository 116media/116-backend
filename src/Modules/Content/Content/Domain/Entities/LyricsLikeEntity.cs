using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user has liked a lyrics page.
/// Created when a user likes; removed when a user unlikes. Never updated.
/// </summary>
public class LyricsLikeEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who liked the lyrics page. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The lyrics page that was liked.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// Navigation property to the lyrics page.
    /// </summary>
    public LyricsEntity Lyrics { get; private set; } = null!;

    private LyricsLikeEntity() { }

    /// <summary>
    /// Creates a new lyrics like record.
    /// </summary>
    /// <param name="id">The unique identifier for this like.</param>
    /// <param name="userId">The user who liked the lyrics page.</param>
    /// <param name="lyricsId">The lyrics page that was liked.</param>
    /// <returns>A new <see cref="LyricsLikeEntity" />.</returns>
    public static LyricsLikeEntity Create(Guid id, Guid userId, Guid lyricsId)
    {
        return new LyricsLikeEntity
        {
            Id = id,
            UserId = userId,
            LyricsId = lyricsId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
