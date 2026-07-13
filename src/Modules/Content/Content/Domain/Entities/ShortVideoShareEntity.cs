using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user (or anonymous visitor) shared a short video.
/// UserId is nullable — anonymous social shares are tracked too.
/// Use a regular UUID primary key (not composite) because UserId can be null.
/// </summary>
public class ShortVideoShareEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who shared. Null for anonymous shares.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The short video that was shared.
    /// </summary>
    public Guid ShortVideoId { get; private set; }

    /// <summary>
    /// The channel the share was sent to (e.g. facebook, x, whatsapp, clipboard, web-share).
    /// Null when the client does not report a target.
    /// </summary>
    public string? Platform { get; private set; }

    /// <summary>
    /// Navigation property to the short video.
    /// </summary>
    public ShortVideoEntity ShortVideo { get; private set; } = null!;

    private ShortVideoShareEntity() { }

    /// <summary>
    /// Creates a new short video share record.
    /// </summary>
    /// <param name="id">The unique identifier for this share.</param>
    /// <param name="userId">The user who shared. Null for anonymous shares.</param>
    /// <param name="shortVideoId">The short video that was shared.</param>
    /// <param name="platform">The channel the share targeted. Null when unreported.</param>
    /// <returns>A new <see cref="ShortVideoShareEntity" />.</returns>
    public static ShortVideoShareEntity Create(Guid id, Guid? userId, Guid shortVideoId, string? platform = null)
    {
        return new ShortVideoShareEntity
        {
            Id = id,
            UserId = userId,
            ShortVideoId = shortVideoId,
            Platform = platform,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
