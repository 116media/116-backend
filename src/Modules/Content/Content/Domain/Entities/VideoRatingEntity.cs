using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records a user's star rating for a video (1–5 stars).
/// Uses Aggregate&lt;Guid&gt; because it can be updated (stars can change).
/// At most one rating per user per video — enforced by a unique index.
/// </summary>
public class VideoRatingEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the rater. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The video that was rated.
    /// </summary>
    public Guid VideoId { get; private set; }

    /// <summary>
    /// The star rating value, between 1 and 5 inclusive.
    /// </summary>
    public short Stars { get; private set; }

    /// <summary>
    /// Navigation property to the video.
    /// </summary>
    public VideoEntity Video { get; private set; } = null!;

    private VideoRatingEntity() { }

    /// <summary>
    /// Creates a new video rating.
    /// </summary>
    /// <param name="id">The unique identifier for the rating.</param>
    /// <param name="userId">The user who submitted the rating.</param>
    /// <param name="videoId">The video being rated.</param>
    /// <param name="stars">The star rating (1–5).</param>
    /// <returns>A new <see cref="VideoRatingEntity" />.</returns>
    public static VideoRatingEntity Create(Guid id, Guid userId, Guid videoId, short stars)
    {
        return new VideoRatingEntity
        {
            Id = id,
            UserId = userId,
            VideoId = videoId,
            Stars = stars,
        };
    }

    /// <summary>
    /// Updates the star rating value.
    /// </summary>
    /// <param name="stars">The new star rating (1–5).</param>
    public void UpdateStars(short stars) => Stars = stars;
}
