using System.ComponentModel.DataAnnotations;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Constants;
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
    /// Display title of the short video.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxShortVideoTitleLength)]
    public string Title { get; private set; } = null!;

    /// <summary>
    /// Publicly accessible CDN URL for the video file.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxShortVideoUrlLength)]
    public string VideoUrl { get; private set; } = null!;

    /// <summary>
    /// Provider-agnostic storage identifier for the video file asset.
    /// Named <c>VideoStorageKey</c> (not <c>CloudinaryPublicId</c>) to avoid coupling
    /// the entity to a specific CDN. Required — the video file must always be trackable
    /// for deletion when the short video is hard deleted.
    /// </summary>
    public string VideoStorageKey { get; private set; } = null!;

    /// <summary>
    /// Optional thumbnail image URL.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxThumbnailUrlLength)]
    public string? ThumbnailUrl { get; private set; }

    /// <summary>
    /// Provider-agnostic storage identifier for the thumbnail image.
    /// Named <c>ThumbnailStorageKey</c> for the same CDN-agnostic reason as <c>VideoStorageKey</c>.
    /// <c>null</c> until a thumbnail is uploaded.
    /// </summary>
    public string? ThumbnailStorageKey { get; private set; }

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
    /// The parent full video this clip previews. <c>null</c> for standalone clips.
    /// </summary>
    public VideoEntity? ParentVideo { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ShortVideoEntity() { }

    /// <summary>
    /// Creates a standalone short video clip with no parent video.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The display title.</param>
    /// <param name="videoUrl">The CDN URL for the video file.</param>
    /// <param name="videoStorageKey">
    /// The provider-agnostic storage key used to delete the video file from media storage.
    /// </param>
    /// <returns>A new active <see cref="ShortVideoEntity" />.</returns>
    public static ShortVideoEntity CreateStandalone(Guid id, string title, string videoUrl, string videoStorageKey)
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw ShortVideoErrors.TitleRequired();
        }

        return new ShortVideoEntity
        {
            Id = id,
            Title = title,
            VideoUrl = videoUrl,
            VideoStorageKey = videoStorageKey,
            HasFullVideo = false,
            IsActive = true,
        };
    }

    /// <summary>
    /// Creates a short video teaser linked to a parent full video.
    /// </summary>
    /// <param name="id">The unique identifier.</param>
    /// <param name="title">The display title.</param>
    /// <param name="videoUrl">The CDN URL for the video file.</param>
    /// <param name="videoStorageKey">
    /// The provider-agnostic storage key used to delete the video file from media storage.
    /// </param>
    /// <param name="videoId">The parent full video this clip previews.</param>
    /// <returns>A new active <see cref="ShortVideoEntity" /> linked to a parent video.</returns>
    public static ShortVideoEntity CreateTeaser(
        Guid id,
        string title,
        string videoUrl,
        string videoStorageKey,
        Guid videoId
    )
    {
        if (string.IsNullOrWhiteSpace(value: title))
        {
            throw ShortVideoErrors.TitleRequired();
        }

        return new ShortVideoEntity
        {
            Id = id,
            Title = title,
            VideoUrl = videoUrl,
            VideoStorageKey = videoStorageKey,
            VideoId = videoId,
            HasFullVideo = true,
            IsActive = true,
        };
    }

    /// <summary>
    /// Sets or replaces the thumbnail for this short video.
    /// Called by <c>UploadShortVideoThumbnailCommandHandler</c>.
    /// </summary>
    /// <param name="thumbnailUrl">The new publicly accessible thumbnail URL.</param>
    /// <param name="thumbnailStorageKey">
    /// The new storage key used to delete the thumbnail from media storage when replaced or hard deleted.
    /// </param>
    public void UpdateThumbnail(string thumbnailUrl, string thumbnailStorageKey)
    {
        ThumbnailUrl = thumbnailUrl;
        ThumbnailStorageKey = thumbnailStorageKey;
    }

    /// <summary>
    /// Makes the short video visible on the public feed.
    /// </summary>
    /// <returns><c>true</c> if activated; <c>false</c> if already active.</returns>
    public bool Activate()
    {
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
