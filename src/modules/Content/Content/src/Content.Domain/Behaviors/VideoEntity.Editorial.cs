using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Editorial behaviour of <see cref="VideoEntity" />. Its state lives in <c>Entities/VideoEntity.cs</c>.
/// </summary>
public sealed partial class VideoEntity
{
    /// <summary>
    /// Renames the video and its slug. Permitted while the video is editable — <c>Draft</c>,
    /// <c>PendingPayment</c>, <c>PendingReview</c> or <c>Rejected</c>.
    /// </summary>
    /// <param name="title">The video title.</param>
    /// <param name="slug">The URL-safe slug. Uniqueness enforced by handler.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool Retitle(string title, string slug)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Video);

        if (Title == title && Slug == slug)
        {
            return false;
        }

        Title = title;
        Slug = slug;

        return true;
    }

    /// <summary>
    /// Revises the editorial description.
    /// </summary>
    /// <param name="description">The video description.</param>
    /// <returns><c>true</c> if the description changed; otherwise <c>false</c>.</returns>
    public bool ReviseDescription(string description)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Video);

        if (Description == description)
        {
            return false;
        }

        Description = description;

        return true;
    }

    /// <summary>
    /// Moves the video to another category. Existence of the category is checked by the handler.
    /// </summary>
    /// <param name="categoryId">The category this video belongs to.</param>
    /// <returns><c>true</c> if the category changed; otherwise <c>false</c>.</returns>
    public bool Recategorize(Guid categoryId)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Video);

        if (CategoryId == categoryId)
        {
            return false;
        }

        CategoryId = categoryId;

        return true;
    }

    /// <summary>
    /// Assigns — or clears — the commission this video fulfils, together with its social boost flag.
    /// </summary>
    /// <param name="customerId">The B2B customer who commissioned this video. <c>null</c> for free content.</param>
    /// <param name="orderItemId">The order item this video fulfils. <c>null</c> for free content.</param>
    /// <param name="socialBoost">Whether this video is flagged for social media promotion.</param>
    /// <returns><c>true</c> if any value changed; otherwise <c>false</c>.</returns>
    public bool AssignCommission(Guid? customerId, Guid? orderItemId, bool socialBoost)
    {
        ContentPublicationState.EnsureEditable(status: Status, contentType: EnumCoreContentType.Video);

        if (CustomerId == customerId && OrderItemId == orderItemId && SocialBoost == socialBoost)
        {
            return false;
        }

        CustomerId = customerId;
        OrderItemId = orderItemId;
        SocialBoost = socialBoost;

        return true;
    }

    /// <summary>
    /// Sets or replaces the thumbnail file reference.
    /// Called when a thumbnail is uploaded via <c>POST /admin/videos/{id}/thumbnail</c>
    /// or automatically after YouTube URL attachment (thumbnail downloaded and re-uploaded).
    /// </summary>
    /// <param name="thumbnailFileId">
    /// The FileEntity ID for the uploaded thumbnail, or null to clear it.
    /// </param>
    public void SetThumbnailFileId(Guid? thumbnailFileId)
    {
        ThumbnailFileId = thumbnailFileId;
    }

    /// <summary>
    /// Attaches the full YouTube video URL and raises
    /// <see cref="VideoYoutubeUrlAttachedEvent" /> so the YouTube thumbnail
    /// can be downloaded and attached post-commit. The attach itself only
    /// records the URL; the thumbnail is a reaction, not part of the
    /// operation's validity.
    /// </summary>
    /// <param name="youtubeVideoUrl">
    /// The full YouTube video URL (e.g., "https://www.youtube.com/watch?v=dQw4w9WgXcQ").
    /// </param>
    /// <exception cref="ContentRuleException">
    /// Thrown when a shooting is scheduled in the future, meaning the video has not yet been shot.
    /// </exception>
    public void AttachYoutubeVideoUrl(string youtubeVideoUrl, DateTimeOffset now)
    {
        if (ShootingScheduledAt.HasValue && ShootingScheduledAt.Value > now)
        {
            throw new ContentRuleException(
                ContentRuleCodes.CannotAttachYoutubeUrlBeforeShoot,
                ShootingScheduledAt.Value.ToString("O")
            );
        }

        YoutubeVideoUrl = youtubeVideoUrl;

        AddDomainEvent(new VideoYoutubeUrlAttachedEvent(VideoId: Id, YoutubeVideoUrl: youtubeVideoUrl));
    }

    /// <summary>
    /// Records or updates the scheduled shooting date and raises
    /// <see cref="VideoShootScheduledEvent" /> so the customer can be told the date.
    /// </summary>
    /// <param name="scheduledAt">The scheduled shoot date.</param>
    public void ScheduleShoot(DateTimeOffset scheduledAt)
    {
        ShootingScheduledAt = scheduledAt;

        AddDomainEvent(
            new VideoShootScheduledEvent(VideoId: Id, CustomerId: CustomerId, Title: Title, ShootDate: scheduledAt)
        );
    }

    /// <summary>
    /// Revises the SEO metadata. Unlike the editorial verbs this is allowed at any status, since
    /// search metadata is maintained after publication.
    /// </summary>
    /// <param name="metaTitle">Optional SEO meta title. Falls back to <c>Title</c> if null.</param>
    /// <param name="metaDescription">Optional SEO meta description.</param>
    /// <returns><c>true</c> if either value changed; otherwise <c>false</c>.</returns>
    public bool ReviseSeo(string? metaTitle, string? metaDescription)
    {
        if (MetaTitle == metaTitle && MetaDescription == metaDescription)
        {
            return false;
        }

        MetaTitle = metaTitle;
        MetaDescription = metaDescription;

        return true;
    }

    /// <summary>
    /// Links this video to a claimed artist profile.
    /// </summary>
    /// <param name="artistId">The <see cref="ArtistEntity" /> ID to link.</param>
    public void LinkArtist(Guid artistId) => ArtistId = artistId;

    /// <summary>
    /// Clears the artist profile link from this video.
    /// </summary>
    public void UnlinkArtist() => ArtistId = null;

    /// <summary>
    /// Replaces the tag set with the given ids, raising one <see cref="TagGraphChangedEvent" />
    /// per tag that joins or leaves. An identical set writes nothing and raises nothing.
    /// </summary>
    /// <param name="tagIds">The complete tag set this row should carry.</param>
    /// <returns><c>true</c> if the set changed; otherwise <c>false</c>.</returns>
    public bool ReplaceTags(IReadOnlyCollection<Guid> tagIds)
    {
        HashSet<Guid> desired = tagIds.ToHashSet();
        HashSet<Guid> current = Tags.Select(tag => tag.TagId).ToHashSet();

        if (desired.SetEquals(current))
        {
            return false;
        }

        foreach (VideoTagEntity removed in Tags.Where(tag => !desired.Contains(tag.TagId)).ToList())
        {
            Tags.Remove(removed);
            AddDomainEvent(new TagGraphChangedEvent(TagId: removed.TagId));
        }

        foreach (Guid tagId in desired.Where(id => !current.Contains(id)))
        {
            Tags.Add(VideoTagEntity.Create(id: Guid.NewGuid(), videoId: Id, tagId: tagId));
            AddDomainEvent(new TagGraphChangedEvent(TagId: tagId));
        }

        return true;
    }
}
