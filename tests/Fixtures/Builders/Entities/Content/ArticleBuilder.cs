using System.Reflection;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ArticleEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; ArticleFactory only names chains three or more tests share.
/// </summary>
public class ArticleBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;
    private string _title = $"{TestConstants.Article.ValidTitle} {Guid.NewGuid():N}";
    private string _slug = $"{TestConstants.Article.ValidSlug}-{Guid.NewGuid():N}";
    private Guid _authorId = Guid.NewGuid();
    private Guid? _customerId;
    private Guid? _orderItemId;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;
    private string? _rejectionReason;
    private DateTimeOffset? _promotedUntil;
    private Guid _promotionLevelId = Guid.NewGuid();
    private DateTimeOffset? _publishedAtOverride;
    private DateTime? _createdAt;
    private CategoryEntity? _category;
    private CustomerEntity? _customerNavigation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleBuilder"/> class with a required category ID.
    /// </summary>
    public ArticleBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    /// <summary>
    /// Sets the article title.
    /// </summary>
    public ArticleBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>
    /// Sets the article slug.
    /// </summary>
    public ArticleBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>
    /// Makes the article a paid article linked to a customer and order item.
    /// </summary>
    public ArticleBuilder WithCustomer(Guid customerId, Guid orderItemId)
    {
        _customerId = customerId;
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>
    /// Transitions the article to PendingPayment status.
    /// </summary>
    public ArticleBuilder AsPendingPayment()
    {
        _targetStatus = EnumContentStatus.PendingPayment;
        return this;
    }

    /// <summary>
    /// Transitions the article to PendingReview status.
    /// </summary>
    public ArticleBuilder AsPendingReview()
    {
        _targetStatus = EnumContentStatus.PendingReview;
        return this;
    }

    /// <summary>
    /// Transitions the article to Approved status.
    /// </summary>
    public ArticleBuilder AsApproved()
    {
        _targetStatus = EnumContentStatus.Approved;
        return this;
    }

    /// <summary>
    /// Transitions the article to Published status.
    /// </summary>
    public ArticleBuilder AsPublished()
    {
        _targetStatus = EnumContentStatus.Published;
        return this;
    }

    /// <summary>
    /// Publishes the article with an explicit PublishedAt, for deterministic "latest first" ordering.
    /// </summary>
    public ArticleBuilder AsPublishedAt(DateTimeOffset publishedAt)
    {
        AsPublished();
        _publishedAtOverride = publishedAt;
        return this;
    }

    /// <summary>
    /// Transitions the article to Rejected status with a reason.
    /// </summary>
    public ArticleBuilder AsRejected(string? reason = null)
    {
        _targetStatus = EnumContentStatus.Rejected;
        _rejectionReason = reason ?? TestConstants.Article.ValidRejectionReason;
        return this;
    }

    /// <summary>
    /// Transitions the article to Archived status.
    /// </summary>
    public ArticleBuilder AsArchived()
    {
        _targetStatus = EnumContentStatus.Archived;
        return this;
    }

    /// <summary>
    /// Stamps the article as promoted until the specified date.
    /// </summary>
    public ArticleBuilder AsPromoted(DateTimeOffset until, Guid? promotionLevelId = null)
    {
        _promotedUntil = until;
        _promotionLevelId = promotionLevelId ?? Guid.NewGuid();
        return this;
    }

    /// <summary>
    /// Overrides the <c>CreatedAt</c> timestamp, for tests that exercise recency-based ordering.
    /// </summary>
    public ArticleBuilder WithCreatedAt(DateTime createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    /// <summary>
    /// Attaches the Category navigation EF Core populates through <c>.Include(a =&gt; a.Category)</c>,
    /// and points the foreign key at the same category.
    /// </summary>
    public ArticleBuilder WithCategory(CategoryEntity category)
    {
        _category = category;
        _categoryId = category.Id;
        return this;
    }

    /// <summary>
    /// Attaches the Customer navigation EF Core populates through <c>.Include(a =&gt; a.Customer)</c>.
    /// Combine with <see cref="WithCustomer" /> to set the matching foreign key.
    /// </summary>
    public ArticleBuilder WithCustomerNavigation(CustomerEntity customer)
    {
        _customerNavigation = customer;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="ArticleEntity"/> instance.
    /// </summary>
    public ArticleEntity Build()
    {
        var errors = TestErrorsFactory.CreateArticleErrors();
        ArticleEntity entity = _customerId.HasValue
            ? ArticleEntity.CreatePaid(
                id: _id,
                customerId: _customerId.Value,
                orderItemId: _orderItemId!.Value,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                errors: errors
            )
            : ArticleEntity.CreateFree(
                id: _id,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId,
                errors: errors
            );

        ApplyStatusTransition(entity);

        if (_promotedUntil.HasValue)
        {
            entity.StampPromotion(_promotionLevelId, _promotedUntil.Value);
        }

        if (_publishedAtOverride.HasValue)
        {
            PropertyInfo publishedProp = typeof(ArticleEntity).GetProperty(
                nameof(ArticleEntity.PublishedAt),
                BindingFlags.Public | BindingFlags.Instance
            )!;

            publishedProp.SetValue(entity, _publishedAtOverride);
        }

        if (_category is not null)
        {
            typeof(ArticleEntity)
                .GetProperty(nameof(ArticleEntity.Category), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(entity, _category);
        }

        if (_customerNavigation is not null)
        {
            typeof(ArticleEntity)
                .GetProperty(nameof(ArticleEntity.Customer), BindingFlags.Public | BindingFlags.Instance)!
                .SetValue(entity, _customerNavigation);
        }

        entity.CreatedAt = _createdAt ?? DateTime.UtcNow;

        return entity;
    }

    private void ApplyStatusTransition(ArticleEntity entity)
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
                entity.Reject(_rejectionReason ?? TestConstants.Article.ValidRejectionReason);
                break;
            case EnumContentStatus.Archived:
                entity.MarkPendingReview();
                entity.Approve();
                entity.Publish();
                entity.Archive();
                break;
            case EnumContentStatus.Draft:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
