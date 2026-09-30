using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="VideoRatingEntity" />. Its state lives in <c>Entities/VideoRatingEntity.cs</c>.
/// </summary>
public partial class VideoRatingEntity
{
    /// <summary>
    /// Updates the star rating value.
    /// </summary>
    /// <param name="stars">The new star rating (1–5).</param>
    public void UpdateStars(short stars)
    {
        Stars = stars;

        AddDomainEvent(new VideoEngagedEvent(VideoId: VideoId, Kind: EnumEngagementKind.Rating, Delta: 0));
    }
}
