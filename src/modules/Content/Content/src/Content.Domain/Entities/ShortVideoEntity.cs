using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a short-form loopable video clip on the platform — teasers, reels, gossip clips,
/// and quick previews. Short videos are distinct from full <see cref="VideoEntity" /> productions:
/// they are uploaded directly to Cloudinary (not YouTube) and do not go through the editorial
/// approval workflow.
/// <para>
/// Short videos can be standalone (gossip, scandal clips) or linked to a full video
/// as a teaser (e.g., a 30-second preview of a 116 Le Focus episode).
/// </para>
/// </summary>
public partial class ShortVideoEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display the title of the short video.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxShortVideoTitleLength)]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug uniquely identifying this short video (e.g., "fally-ipupa-teaser-1").
    /// Used as the public permalink on the short video page.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public Slug Slug { get; private set; } = null!;

    /// <summary>
    /// ID of the uploaded video file tracked in the Storage module, or null while the short video
    /// is still a draft. A short video is created as a draft (no file) and the video file is
    /// attached afterwards via the dedicated upload endpoint. The video URL and storage key are
    /// resolved from the associated FileEntity.
    /// </summary>
    public Guid? VideoFileId { get; private set; }

    /// <summary>
    /// ID of the uploaded thumbnail file tracked in the Storage module.
    /// Null until a thumbnail is manually uploaded.
    /// </summary>
    public Guid? ThumbnailFileId { get; private set; }

    /// <summary>
    /// Optional link to the parent full video (e.g., a 116 Le Focus episode this clip previews).
    /// <c>null</c> for standalone short videos.
    /// </summary>
    public Guid? VideoId { get; private set; }

    /// <summary>
    /// Whether this short video is a teaser for a full <see cref="VideoEntity" /> production.
    /// <c>true</c> when <c>VideoId</c> is set.
    /// </summary>
    public bool HasFullVideo { get; private set; }

    /// <summary>
    /// Whether this short video is visible on the public feed.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Stable, uniformly-random 64-bit rank used to shuffle the public feed. Combined with a
    /// per-session seed via bitwise XOR, it yields a fresh random ordering each session while
    /// keeping keyset pagination stable. Assigned once at creation and never changed.
    /// </summary>
    public long FeedRank { get; private set; }

    /// <summary>
    /// Cached view count, maintained by <c>ShortVideoRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int ViewCount { get; private init; }

    /// <summary>
    /// Cached like count, maintained by <c>ShortVideoRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int LikeCount { get; private init; }

    /// <summary>
    /// Cached share count, maintained by <c>ShortVideoRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int ShareCount { get; private init; }

    /// <summary>
    /// Cached bookmark count, maintained by <c>ShortVideoRepository.ApplyEngagementDeltaAsync</c>.
    /// </summary>
    public int BookmarkCount { get; private init; }

    /// <summary>
    /// The identity user UUID of the admin who uploaded this short video.
    /// Distinguished from <c>CreatedBy</c> (system audit trail) — <c>AuthorId</c> is the
    /// editorial owner shown in the CMS. No FK to the identity schema by design.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ShortVideoEntity() { }

    /// <summary>
    /// Draws a new uniformly-random 64-bit feed rank. Collisions are astronomically unlikely
    /// and guarded by a unique index, matching how <c>Guid</c> identities are treated.
    /// </summary>
    private static long NewFeedRank() => Random.Shared.NextInt64(long.MinValue, long.MaxValue);

    /// <summary>
    /// Creates a standalone short video draft with no parent video and no video file yet.
    /// The video file is attached afterwards via the dedicated upload endpoint; the draft stays
    /// inactive (hidden from the feed) until a file is uploaded and it is activated.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The display title.</param>
    /// <param name="slug">The URL-safe slug for the short video permalink.</param>
    /// <param name="authorId">The identity user UUID of the admin uploading this short video.</param>
    /// <returns>A new inactive draft <see cref="ShortVideoEntity" />.</returns>
    public static ShortVideoEntity CreateStandalone(Guid id, string title, string slug, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw new ContentRuleException(ContentRuleCodes.ShortVideoTitleRequired);
        }

        return new ShortVideoEntity
        {
            Id = id,
            Title = title,
            Slug = slug,
            AuthorId = authorId,
            HasFullVideo = false,
            IsActive = false,
            FeedRank = NewFeedRank(),
        };
    }

    /// <summary>
    /// Creates a short video teaser linked to a parent full video.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The display title.</param>
    /// <param name="slug">The URL-safe slug for the short video permalink.</param>
    /// <param name="videoId">The parent full video this clip previews.</param>
    /// <param name="authorId">The identity user UUID of the admin uploading this short video.</param>
    /// <returns>A new inactive draft <see cref="ShortVideoEntity" /> linked to a parent video.</returns>
    public static ShortVideoEntity CreateTeaser(Guid id, string title, string slug, Guid videoId, Guid authorId)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw new ContentRuleException(ContentRuleCodes.ShortVideoTitleRequired);
        }

        return new ShortVideoEntity
        {
            Id = id,
            Title = title,
            Slug = slug,
            VideoId = videoId,
            AuthorId = authorId,
            HasFullVideo = true,
            IsActive = false,
            FeedRank = NewFeedRank(),
        };
    }
}
