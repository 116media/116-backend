using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking a video to a tag (many-to-many).
/// </summary>
public class VideoTagEntity : Entity<Guid>
{
    /// <summary>
    /// The identifier of the video.
    /// </summary>
    public Guid VideoId { get; private set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; private set; }

    private VideoTagEntity() { }

    /// <summary>
    /// Creates a new video-tag association.
    /// </summary>
    /// <param name="id">The unique identifier for this association.</param>
    /// <param name="videoId">The video being tagged.</param>
    /// <param name="tagId">The tag being applied.</param>
    /// <returns>A new <see cref="VideoTagEntity" />.</returns>
    internal static VideoTagEntity Create(Guid id, Guid videoId, Guid tagId)
    {
        var association = new VideoTagEntity
        {
            Id = id,
            VideoId = videoId,
            TagId = tagId,
        };

        return association;
    }
}
