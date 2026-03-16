using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="VideoEntity"/> instances in tests.
/// For test code, prefer using VideoFactory instead of direct Builder usage.
/// </summary>
internal class VideoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;

    private string _title = TestConstants.Content.Editorial.Video.ValidTitle;
    private string _slug = TestConstants.Content.Editorial.Video.ValidSlug;
    private Guid _authorId = Guid.NewGuid();
    private string? _description;
    private Guid? _customerId;
    private Guid? _orderItemId;
    private string? _youtubeVideoId;
    private string? _thumbnailUrl;
    private string? _thumbnailStorageKey;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;
    private string? _rejectionReason;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoBuilder"/> class with a required category ID.
    /// </summary>
    public VideoBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    /// <summary>Sets the video ID.</summary>
    public VideoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the category ID.</summary>
    public VideoBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>Sets the video title.</summary>
    public VideoBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>Sets the video slug.</summary>
    public VideoBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>Sets the author ID.</summary>
    public VideoBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    /// <summary>Sets the optional description.</summary>
    public VideoBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    /// <summary>Makes the video a paid video linked to a customer and order item.</summary>
    public VideoBuilder WithCustomer(Guid customerId, Guid orderItemId)
    {
        _customerId = customerId;
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>Attaches a YouTube video ID (required before Publishing).</summary>
    public VideoBuilder WithYoutubeId(string? youtubeVideoId = null)
    {
        _youtubeVideoId = youtubeVideoId ?? TestConstants.Content.Editorial.Video.ValidYoutubeVideoId;
        return this;
    }

    /// <summary>Sets a thumbnail URL and storage key.</summary>
    public VideoBuilder WithThumbnail(string? url = null, string? storageKey = null)
    {
        _thumbnailUrl = url ?? TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl;
        _thumbnailStorageKey = storageKey ?? TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey;
        return this;
    }

    /// <summary>Transitions the video to PendingPayment status.</summary>
    public VideoBuilder AsPendingPayment()
    {
        _targetStatus = EnumContentStatus.PendingPayment;
        return this;
    }

    /// <summary>Transitions the video to PendingReview status.</summary>
    public VideoBuilder AsPendingReview()
    {
        _targetStatus = EnumContentStatus.PendingReview;
        return this;
    }

    /// <summary>Transitions the video to Approved status.</summary>
    public VideoBuilder AsApproved()
    {
        _targetStatus = EnumContentStatus.Approved;
        return this;
    }

    /// <summary>Transitions the video to Published status (requires YouTube ID).</summary>
    public VideoBuilder AsPublished()
    {
        _targetStatus = EnumContentStatus.Published;
        if (_youtubeVideoId is null)
        {
            _youtubeVideoId = TestConstants.Content.Editorial.Video.ValidYoutubeVideoId;
        }

        return this;
    }

    /// <summary>Transitions the video to Rejected status with a reason.</summary>
    public VideoBuilder AsRejected(string? reason = null)
    {
        _targetStatus = EnumContentStatus.Rejected;
        _rejectionReason = reason ?? TestConstants.Content.Editorial.Video.ValidRejectionReason;
        return this;
    }

    /// <summary>Transitions the video to Archived status (requires YouTube ID; auto-set if not provided).</summary>
    public VideoBuilder AsArchived()
    {
        _targetStatus = EnumContentStatus.Archived;
        if (_youtubeVideoId is null)
        {
            _youtubeVideoId = TestConstants.Content.Editorial.Video.ValidYoutubeVideoId;
        }

        return this;
    }

    /// <summary>Builds the <see cref="VideoEntity"/> instance.</summary>
    public VideoEntity Build()
    {
        VideoEntity entity = _customerId.HasValue
            ? VideoEntity.CreatePaid(
                id: _id,
                customerId: _customerId.Value,
                orderItemId: _orderItemId!.Value,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                description: _description
            )
            : VideoEntity.CreateFree(
                id: _id,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                description: _description
            );

        if (_youtubeVideoId is not null)
        {
            entity.AttachYoutubeId(_youtubeVideoId);
        }

        if (_thumbnailUrl is not null && _thumbnailStorageKey is not null)
        {
            entity.UpdateThumbnail(_thumbnailUrl, _thumbnailStorageKey);
        }

        ApplyStatusTransition(entity);

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }

    private void ApplyStatusTransition(VideoEntity entity)
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
                entity.Publish();
                break;
            case EnumContentStatus.Rejected:
                entity.Reject(_rejectionReason ?? TestConstants.Content.Editorial.Video.ValidRejectionReason);
                break;
            case EnumContentStatus.Archived:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish();
                entity.Archive();
                break;
        }
    }
}
