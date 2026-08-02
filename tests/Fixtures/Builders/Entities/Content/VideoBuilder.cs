using System.Reflection;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// For test code, prefer using VideoFactory instead of direct Builder usage.
/// </summary>
internal class VideoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;

    private string _title = $"{TestConstants.Content.Editorial.Video.ValidTitle} {Guid.NewGuid():N}";
    private string _slug = $"{TestConstants.Content.Editorial.Video.ValidSlug}-{Guid.NewGuid():N}";
    private Guid _authorId = Guid.NewGuid();
    private string _description = TestConstants.Content.Editorial.Video.ValidDescription;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private string? _youtubeVideoUrl;
    private Guid? _thumbnailFileId;
    private DateTimeOffset? _shootingScheduledAt;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;
    private string? _rejectionReason;
    private DateTimeOffset? _promotedUntil;
    private Guid _promotionLevelId = Guid.NewGuid();
    private DateTimeOffset? _publishedAtOverride;
    private Guid? _artistId;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoBuilder"/> class with a required category ID.
    /// </summary>
    public VideoBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    /// <summary>
    /// Sets the video ID.
    /// </summary>
    public VideoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the category ID.
    /// </summary>
    public VideoBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>
    /// Sets the video title.
    /// </summary>
    public VideoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the video slug.
    /// </summary>
    public VideoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Sets the author ID.
    /// </summary>
    public VideoBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    /// <summary>
    /// Sets the description.
    /// </summary>
    public VideoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <summary>
    /// Makes the video a paid video linked to a customer and order item.
    /// </summary>
    public VideoBuilder WithCustomer(Guid customerId, Guid orderItemId)
    {
        _customerId = customerId;
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Attaches a YouTube video URL (required before Publishing).
    /// </summary>
    public VideoBuilder WithYoutubeUrl(string? youtubeVideoUrl = null)
    {
        _youtubeVideoUrl = youtubeVideoUrl ?? TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;
        return this;
    }

    /// <summary>
    /// Sets a shooting scheduled date on the video.
    /// </summary>
    public VideoBuilder WithShootingScheduledAt(DateTimeOffset scheduledAt)
    {
        _shootingScheduledAt = scheduledAt;
        return this;
    }

    /// <summary>
    /// Sets the thumbnail file ID (FileEntity reference).
    /// </summary>
    public VideoBuilder WithThumbnail(Guid? fileId = null)
    {
        _thumbnailFileId = fileId ?? Guid.NewGuid();
        return this;
    }

    /// <summary>
    /// Sets the thumbnail file ID (FileEntity reference).
    /// </summary>
    public VideoBuilder WithThumbnailFileId(Guid fileId)
    {
        _thumbnailFileId = fileId;
        return this;
    }

    /// <summary>
    /// Links this video to a real, addressable artist profile.
    /// </summary>
    public VideoBuilder WithArtistId(Guid artistId)
    {
        _artistId = artistId;
        return this;
    }

    /// <summary>
    /// Transitions the video to PendingPayment status.
    /// </summary>
    public VideoBuilder AsPendingPayment()
    {
        _targetStatus = EnumContentStatus.PendingPayment;
        return this;
    }

    /// <summary>
    /// Transitions the video to PendingReview status.
    /// </summary>
    public VideoBuilder AsPendingReview()
    {
        _targetStatus = EnumContentStatus.PendingReview;
        return this;
    }

    /// <summary>
    /// Transitions the video to Approved status.
    /// </summary>
    public VideoBuilder AsApproved()
    {
        _targetStatus = EnumContentStatus.Approved;
        return this;
    }

    /// <summary>
    /// Transitions the video to Published status (requires YouTube URL).
    /// </summary>
    public VideoBuilder AsPublished()
    {
        _targetStatus = EnumContentStatus.Published;
        if (_youtubeVideoUrl is not null)
        {
            return this;
        }
        _youtubeVideoUrl = TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;

        return this;
    }

    /// <summary>
    /// Publishes the video with an explicit PublishedAt, for deterministic "latest first" ordering.
    /// </summary>
    public VideoBuilder AsPublishedAt(DateTimeOffset publishedAt)
    {
        AsPublished();
        _publishedAtOverride = publishedAt;
        return this;
    }

    /// <summary>
    /// Transitions the video to Rejected status with a reason.
    /// </summary>
    public VideoBuilder AsRejected(string? reason = null)
    {
        _targetStatus = EnumContentStatus.Rejected;
        _rejectionReason = reason ?? TestConstants.Content.Editorial.Video.ValidRejectionReason;
        return this;
    }

    /// <summary>
    /// Stamps the video as promoted until the specified date.
    /// </summary>
    public VideoBuilder AsPromoted(DateTimeOffset until, Guid? promotionLevelId = null)
    {
        _promotedUntil = until;
        _promotionLevelId = promotionLevelId ?? Guid.NewGuid();
        return this;
    }

    /// <summary>
    /// Transitions the video to Archived status (requires YouTube URL; auto-set if not provided).
    /// </summary>
    public VideoBuilder AsArchived()
    {
        _targetStatus = EnumContentStatus.Archived;
        if (_youtubeVideoUrl is not null)
        {
            return this;
        }
        _youtubeVideoUrl = TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl;

        return this;
    }

    /// <summary>
    /// Builds the <see cref="VideoEntity"/> instance.
    /// </summary>
    public VideoEntity Build()
    {
        VideoErrors errors = TestErrorsFactory.CreateVideoErrors();
        VideoEntity entity = _customerId.HasValue
            ? VideoEntity.CreatePaid(
                id: _id,
                customerId: _customerId.Value,
                orderItemId: _orderItemId!.Value,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                description: _description,
                errors: errors
            )
            : VideoEntity.CreateFree(
                id: _id,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                description: _description,
                errors: errors
            );

        if (_shootingScheduledAt.HasValue)
        {
            entity.ScheduleShoot(_shootingScheduledAt.Value);
        }

        if (_youtubeVideoUrl is not null)
        {
            entity.AttachYoutubeVideoUrl(_youtubeVideoUrl, errors);
        }

        if (_thumbnailFileId.HasValue)
        {
            entity.SetThumbnailFileId(_thumbnailFileId);
        }

        ApplyStatusTransition(entity, errors);

        if (_promotedUntil.HasValue)
        {
            entity.StampPromotion(_promotionLevelId, _promotedUntil.Value);
        }

        if (_publishedAtOverride.HasValue)
        {
            PropertyInfo publishedProp = typeof(VideoEntity).GetProperty(
                nameof(VideoEntity.PublishedAt),
                BindingFlags.Public | BindingFlags.Instance
            )!;

            publishedProp.SetValue(entity, _publishedAtOverride);
        }

        if (_artistId.HasValue)
        {
            entity.LinkArtist(_artistId.Value);
        }

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }

    private void ApplyStatusTransition(VideoEntity entity, VideoErrors errors)
    {
        switch (_targetStatus)
        {
            case EnumContentStatus.PendingPayment:
                entity.Submit();
                break;
            case EnumContentStatus.PendingReview:
                entity.MarkPendingReview();
                break;
            case EnumContentStatus.Approved:
                entity.MarkPendingReview();
                entity.Approve();
                break;
            case EnumContentStatus.Published:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish(errors);
                break;
            case EnumContentStatus.Rejected:
                entity.Reject(_rejectionReason ?? TestConstants.Content.Editorial.Video.ValidRejectionReason);
                break;
            case EnumContentStatus.Archived:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish(errors);
                entity.Archive();
                break;
        }
    }
}
