using System.Reflection;
using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="VideoEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; VideoFactory only names chains three or more tests share.
/// </summary>
public class VideoBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;

    private string _title = $"{TestConstants.Video.ValidTitle} {Guid.NewGuid():N}";
    private string _slug = $"{TestConstants.Video.ValidSlug}-{Guid.NewGuid():N}";
    private Guid _authorId = Guid.NewGuid();
    private string _description = TestConstants.Video.ValidDescription;
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
    private CategoryEntity? _category;
    private CustomerEntity? _customerNavigation;

    /// <summary>
    /// Initializes a new instance of the <see cref="VideoBuilder"/> class with a required category ID.
    /// </summary>
    public VideoBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
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
    /// <summary>
    /// Sets the description, which the search specification matches alongside the title.
    /// </summary>
    /// <param name="description">The video description.</param>
    /// <returns>The builder instance for chaining.</returns>
    public VideoBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public VideoBuilder WithSlug(string slug)
    {
        _slug = slug;
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
        _youtubeVideoUrl = youtubeVideoUrl ?? TestConstants.Video.ValidYoutubeVideoUrl;
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
        _youtubeVideoUrl = TestConstants.Video.ValidYoutubeVideoUrl;

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
        _rejectionReason = reason ?? TestConstants.Video.ValidRejectionReason;
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
        _youtubeVideoUrl = TestConstants.Video.ValidYoutubeVideoUrl;

        return this;
    }

    /// <summary>
    /// Attaches the Category navigation EF Core populates through <c>.Include(v =&gt; v.Category)</c>,
    /// and points the foreign key at the same category.
    /// </summary>
    public VideoBuilder WithCategory(CategoryEntity category)
    {
        _category = category;
        _categoryId = category.Id;
        return this;
    }

    /// <summary>
    /// Attaches the Customer navigation EF Core populates through <c>.Include(v =&gt; v.Customer)</c>.
    /// Combine with <see cref="WithCustomer" /> to set the matching foreign key.
    /// </summary>
    public VideoBuilder WithCustomerNavigation(CustomerEntity customer)
    {
        _customerNavigation = customer;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="VideoEntity"/> instance.
    /// </summary>
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

        if (_shootingScheduledAt.HasValue)
        {
            entity.ScheduleShoot(_shootingScheduledAt.Value);
        }

        if (_youtubeVideoUrl is not null)
        {
            entity.AttachYoutubeVideoUrl(_youtubeVideoUrl);
        }

        if (_thumbnailFileId.HasValue)
        {
            entity.SetThumbnailFileId(_thumbnailFileId);
        }

        ApplyStatusTransition(entity);

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

        if (_category is not null)
        {
            typeof(VideoEntity)
                .GetProperty(nameof(VideoEntity.Category), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(entity, _category);
        }

        if (_customerNavigation is not null)
        {
            typeof(VideoEntity)
                .GetProperty(nameof(VideoEntity.Customer), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(entity, _customerNavigation);
        }

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
                entity.MarkPendingReview();
                entity.Reject(_rejectionReason ?? TestConstants.Video.ValidRejectionReason);
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
