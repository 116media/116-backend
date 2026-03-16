namespace _116.Content.Domain.Entities;

/// <summary>
/// Junction entity linking a video to a tag (many-to-many).
/// Uses a composite primary key configured in EF Core: (VideoId, TagId).
/// </summary>
public class VideoTagEntity
{
    /// <summary>
    /// The identifier of the video.
    /// </summary>
    public Guid VideoId { get; set; }

    /// <summary>
    /// The identifier of the tag.
    /// </summary>
    public Guid TagId { get; set; }

    /// <summary>
    /// The video associated with this tag relationship.
    /// </summary>
    public VideoEntity Video { get; set; } = null!;

    /// <summary>
    /// The tag associated with this video relationship.
    /// </summary>
    public TagEntity Tag { get; set; } = null!;
}
