using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user (or anonymous visitor) shared a video.
/// UserId is nullable — anonymous social shares are tracked too.
/// Uses a regular UUID primary key (not composite) because UserId can be null.
/// </summary>
public class VideoShareEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who shared. Null for anonymous shares.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The video that was shared.
    /// </summary>
    public Guid VideoId { get; private set; }

    /// <summary>
    /// The channel the share was sent to (e.g. facebook, x, whatsapp, clipboard, web-share).
    /// Null when the client does not report a target.
    /// </summary>
    public string? Platform { get; private set; }

    /// <summary>
    /// Navigation property to the video.
    /// </summary>
    public VideoEntity Video { get; private set; } = null!;

    private VideoShareEntity() { }

    /// <summary>
    /// Creates a new video share record.
    /// </summary>
    /// <param name="id">The unique identifier for this share.</param>
    /// <param name="userId">The user who shared. Null for anonymous shares.</param>
    /// <param name="videoId">The video that was shared.</param>
    /// <param name="platform">The channel the share targeted. Null when unreported.</param>
    /// <returns>A new <see cref="VideoShareEntity" />.</returns>
    public static VideoShareEntity Create(Guid id, Guid? userId, Guid videoId, string? platform = null)
    {
        return new VideoShareEntity
        {
            Id = id,
            UserId = userId,
            VideoId = videoId,
            Platform = platform,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
