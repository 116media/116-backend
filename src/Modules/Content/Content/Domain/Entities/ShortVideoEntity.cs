using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
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
public class ShortVideoEntity : Aggregate<Guid>
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
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// ID of the uploaded video file tracked in the Core module, or null while the short video
    /// is still a draft. A short video is created as a draft (no file) and the video file is
    /// attached afterwards via the dedicated upload endpoint. The video URL and storage key are
    /// resolved from the associated FileEntity.
    /// </summary>
    public Guid? VideoFileId { get; private set; }

    /// <summary>
    /// ID of the uploaded thumbnail file tracked in the Core module.
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
    /// Cached view count.
    /// </summary>
    public int ViewCount { get; private set; }

    /// <summary>
    /// Cached like count.
    /// </summary>
    public int LikeCount { get; private set; }

    /// <summary>
    /// Cached share count.
    /// </summary>
    public int ShareCount { get; private set; }

    /// <summary>
    /// Cached bookmark count.
    /// </summary>
    public int BookmarkCount { get; private set; }

    /// <summary>
    /// The identity user UUID of the admin who uploaded this short video.
    /// Distinguished from <c>CreatedBy</c> (system audit trail) — <c>AuthorId</c> is the
    /// editorial owner shown in the CMS. No FK to the identity schema by design.
    /// </summary>
    public Guid AuthorId { get; private set; }

    /// <summary>
    /// The parent full video this clip previews. <c>null</c> for standalone clips.
    /// </summary>
    public VideoEntity? ParentVideo { get; private set; }

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

    /// <summary>
    /// Updates the editable metadata fields of this short video.
    /// Slug is immutable after creation to preserve public URLs.
    /// </summary>
    /// <param name="title">The new display title.</param>
    /// <param name="videoId">Optional parent full video ID. <c>null</c> to make standalone.</param>
    public void Update(string title, Guid? videoId)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw new ContentRuleException(ContentRuleCodes.ShortVideoTitleRequired);
        }

        Title = title;
        VideoId = videoId;
        HasFullVideo = videoId.HasValue;
    }

    /// <summary>
    /// Replaces the video file reference after a successful re-upload.
    /// </summary>
    /// <param name="videoFileId">
    /// The new FileEntity ID for the re-uploaded video file.
    /// </param>
    public void ReplaceVideoFile(Guid videoFileId)
    {
        VideoFileId = videoFileId;
    }

    /// <summary>
    /// Sets or replaces the thumbnail file reference for this short video.
    /// </summary>
    /// <param name="thumbnailFileId">
    /// The FileEntity ID for the uploaded thumbnail, or null to clear it.
    /// </param>
    public void SetThumbnailFileId(Guid? thumbnailFileId)
    {
        ThumbnailFileId = thumbnailFileId;
    }

    /// <summary>
    /// Makes the short video visible on the public feed. A short video cannot be activated
    /// until its video file has been uploaded.
    /// </summary>
    /// <returns><c>true</c> if activated; <c>false</c> if already active.</returns>
    public bool Activate()
    {
        if (VideoFileId is null)
        {
            throw new ContentRuleException(ContentRuleCodes.ShortVideoFileRequired);
        }

        if (IsActive)
        {
            return false;
        }

        IsActive = true;
        return true;
    }

    /// <summary>
    /// Hides the short video from the public feed. Deactivation is reversible
    /// and does not delete any media assets.
    /// </summary>
    /// <returns><c>true</c> if deactivated; <c>false</c> if already inactive.</returns>
    public bool Deactivate()
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        return true;
    }

    /// <summary>
    /// Declares the short video's removal, capturing the video and thumbnail
    /// file ids before the row disappears so post-commit consumers can clean
    /// the remote assets without re-querying deleted rows. Called by the
    /// delete flow immediately before the repository removal.
    /// </summary>
    public void MarkDeleted()
    {
        AddDomainEvent(
            new ShortVideoDeletedEvent(ShortVideoId: Id, VideoFileId: VideoFileId, ThumbnailFileId: ThumbnailFileId)
        );
    }

    /// <summary>
    /// Increments the cached view count.
    /// </summary>
    public void IncrementViewCount() => ViewCount++;

    /// <summary>
    /// Increments the cached like count.
    /// </summary>
    public void IncrementLikeCount() => LikeCount++;

    /// <summary>
    /// Decrements the cached like count, floor at zero.
    /// </summary>
    public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);

    /// <summary>
    /// Increments the cached share count.
    /// </summary>
    public void IncrementShareCount() => ShareCount++;

    /// <summary>
    /// Increments the cached bookmark count.
    /// </summary>
    public void IncrementBookmarkCount() => BookmarkCount++;

    /// <summary>
    /// Decrements the cached bookmark count, floor at zero.
    /// </summary>
    public void DecrementBookmarkCount() => BookmarkCount = Math.Max(0, BookmarkCount - 1);
}
