using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user has bookmarked a short video.
/// Created when a user bookmarks; removed when a user un-bookmarks. Never updated.
/// </summary>
public class ShortVideoBookmarkEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who bookmarked the short video. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The short video that was bookmarked.
    /// </summary>
    public Guid ShortVideoId { get; private set; }

    /// <summary>
    /// Navigation property to the short video.
    /// </summary>
    public ShortVideoEntity ShortVideo { get; private set; } = null!;

    private ShortVideoBookmarkEntity() { }

    /// <summary>
    /// Creates a new short video bookmark record.
    /// </summary>
    /// <param name="id">The unique identifier for this bookmark.</param>
    /// <param name="userId">The user who bookmarked the short video.</param>
    /// <param name="shortVideoId">The short video that was bookmarked.</param>
    /// <returns>A new <see cref="ShortVideoBookmarkEntity" />.</returns>
    public static ShortVideoBookmarkEntity Create(Guid id, Guid userId, Guid shortVideoId)
    {
        return new ShortVideoBookmarkEntity
        {
            Id = id,
            UserId = userId,
            ShortVideoId = shortVideoId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
