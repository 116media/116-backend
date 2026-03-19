using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user has liked a short video.
/// Created when a user likes; removed when a user unlikes. Never updated.
/// </summary>
public class ShortVideoLikeEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who liked the short video. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The short video that was liked.
    /// </summary>
    public Guid ShortVideoId { get; private set; }

    /// <summary>
    /// Navigation property to the short video.
    /// </summary>
    public ShortVideoEntity ShortVideo { get; private set; } = null!;

    private ShortVideoLikeEntity() { }

    /// <summary>
    /// Creates a new short video like record.
    /// </summary>
    /// <param name="id">The unique identifier for this like.</param>
    /// <param name="userId">The user who liked the short video.</param>
    /// <param name="shortVideoId">The short video that was liked.</param>
    /// <returns>A new <see cref="ShortVideoLikeEntity" />.</returns>
    public static ShortVideoLikeEntity Create(Guid id, Guid userId, Guid shortVideoId)
    {
        return new ShortVideoLikeEntity
        {
            Id = id,
            UserId = userId,
            ShortVideoId = shortVideoId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
