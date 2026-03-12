using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="ArticleEntity"/> instances in tests.
/// For test code, prefer using ArticleFactory instead of direct Builder usage.
/// </summary>
internal class ArticleBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _categoryId;
    private string _title = TestConstants.Content.Editorial.Article.ValidTitle;
    private string _slug = TestConstants.Content.Editorial.Article.ValidSlug;
    private Guid _authorId = Guid.NewGuid();
    private Guid? _customerId;
    private Guid? _orderItemId;
    private EnumContentStatus _targetStatus = EnumContentStatus.Draft;
    private string? _rejectionReason;
    private bool _stampSocialBoost;
    private DateTimeOffset? _featuredUntil;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArticleBuilder"/> class with a required category ID.
    /// </summary>
    public ArticleBuilder(Guid categoryId)
    {
        _categoryId = categoryId;
    }

    /// <summary>Sets the article ID.</summary>
    public ArticleBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>Sets the category ID.</summary>
    public ArticleBuilder WithCategoryId(Guid categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <summary>Sets the article title.</summary>
    public ArticleBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    /// <summary>Sets the article slug.</summary>
    public ArticleBuilder WithSlug(string slug)
    {
        _slug = slug;
        return this;
    }

    /// <summary>Sets the author ID.</summary>
    public ArticleBuilder WithAuthorId(Guid authorId)
    {
        _authorId = authorId;
        return this;
    }

    /// <summary>Makes the article a paid article linked to a customer and order item.</summary>
    public ArticleBuilder WithCustomer(Guid customerId, Guid orderItemId)
    {
        _customerId = customerId;
        _orderItemId = orderItemId;
        return this;
    }

    /// <summary>Transitions the article to PendingPayment status.</summary>
    public ArticleBuilder AsPendingPayment()
    {
        _targetStatus = EnumContentStatus.PendingPayment;
        return this;
    }

    /// <summary>Transitions the article to PendingReview status.</summary>
    public ArticleBuilder AsPendingReview()
    {
        _targetStatus = EnumContentStatus.PendingReview;
        return this;
    }

    /// <summary>Transitions the article to Approved status.</summary>
    public ArticleBuilder AsApproved()
    {
        _targetStatus = EnumContentStatus.Approved;
        return this;
    }

    /// <summary>Transitions the article to Published status.</summary>
    public ArticleBuilder AsPublished()
    {
        _targetStatus = EnumContentStatus.Published;
        return this;
    }

    /// <summary>Transitions the article to Rejected status with a reason.</summary>
    public ArticleBuilder AsRejected(string? reason = null)
    {
        _targetStatus = EnumContentStatus.Rejected;
        _rejectionReason = reason ?? TestConstants.Content.Editorial.Article.ValidRejectionReason;
        return this;
    }

    /// <summary>Transitions the article to Archived status.</summary>
    public ArticleBuilder AsArchived()
    {
        _targetStatus = EnumContentStatus.Archived;
        return this;
    }

    /// <summary>Stamps the article as having social boost.</summary>
    public ArticleBuilder WithSocialBoost()
    {
        _stampSocialBoost = true;
        return this;
    }

    /// <summary>Stamps the article as featured until the specified date.</summary>
    public ArticleBuilder AsFeatured(DateTimeOffset until)
    {
        _featuredUntil = until;
        return this;
    }

    /// <summary>Builds the <see cref="ArticleEntity"/> instance.</summary>
    public ArticleEntity Build()
    {
        ArticleEntity entity = _customerId.HasValue
            ? ArticleEntity.CreatePaid(
                id: _id,
                customerId: _customerId.Value,
                orderItemId: _orderItemId!.Value,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId
            )
            : ArticleEntity.CreateFree(
                id: _id,
                categoryId: _categoryId,
                title: _title,
                slug: _slug,
                authorId: _authorId
            );

        ApplyStatusTransition(entity);

        if (_stampSocialBoost)
        {
            entity.StampSocialBoost();
        }

        if (_featuredUntil.HasValue)
        {
            entity.StampFeatured(_featuredUntil.Value);
        }

        entity.CreatedAt = DateTime.UtcNow;

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
                entity.Reject(_rejectionReason ?? TestConstants.Content.Editorial.Article.ValidRejectionReason);
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
