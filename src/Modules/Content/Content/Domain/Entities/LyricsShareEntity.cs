using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Records that a user (or anonymous visitor) shared a lyrics page.
/// UserId is nullable — anonymous social shares are tracked too.
/// Use a regular UUID primary key (not composite) because UserId can be null.
/// </summary>
public class LyricsShareEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who shared. Null for anonymous shares.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// The lyrics page that was shared.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// The channel the share was sent to. Null when the client does not report a target.
    /// </summary>
    public EnumShareChannel? ShareChannel { get; private set; }

    /// <summary>
    /// Navigation property to the lyrics page.
    /// </summary>
    public LyricsEntity Lyrics { get; private set; } = null!;

    private LyricsShareEntity() { }

    /// <summary>
    /// Creates a new lyrics share record.
    /// </summary>
    /// <param name="id">The unique identifier for this share.</param>
    /// <param name="userId">The user who shared. Null for anonymous shares.</param>
    /// <param name="lyricsId">The lyrics page that was shared.</param>
    /// <param name="shareChannel">The channel the share targeted. Null when unreported.</param>
    /// <returns>A new <see cref="LyricsShareEntity" />.</returns>
    public static LyricsShareEntity Create(Guid id, Guid? userId, Guid lyricsId, EnumShareChannel? shareChannel = null)
    {
        var share = new LyricsShareEntity
        {
            Id = id,
            UserId = userId,
            LyricsId = lyricsId,
            ShareChannel = shareChannel,
            CreatedAt = DateTime.UtcNow,
        };

        share.AddDomainEvent(new LyricsEngagedEvent(LyricsId: lyricsId, Kind: EnumEngagementKind.Share, Delta: 1));

        return share;
    }
}
