using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="ArticleEntity"/> instances in tests.
/// </summary>
public static class ArticleFactory
{
    /// <summary>Creates a free article in Draft status with the given category.</summary>
    public static ArticleEntity Create(Guid categoryId) => new ArticleBuilder(categoryId).Build();

    /// <summary>Creates a free article in Draft status.</summary>
    public static ArticleEntity CreateFree(Guid categoryId) => new ArticleBuilder(categoryId).Build();

    /// <summary>Creates a paid article in Draft status linked to a customer and order item.</summary>
    public static ArticleEntity CreatePaid(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new ArticleBuilder(categoryId).WithCustomer(customerId, orderItemId).Build();

    /// <summary>Creates a free article with a specific ID in Draft status.</summary>
    public static ArticleEntity CreateWithId(Guid id, Guid categoryId) =>
        new ArticleBuilder(categoryId).WithId(id).Build();

    /// <summary>Creates a free article with specific title and slug in Draft status.</summary>
    public static ArticleEntity Create(Guid categoryId, string title, string slug) =>
        new ArticleBuilder(categoryId).WithTitle(title).WithSlug(slug).Build();

    /// <summary>Creates a published free article.</summary>
    public static ArticleEntity CreatePublished(Guid categoryId) =>
        new ArticleBuilder(categoryId).AsPublished().Build();

    /// <summary>Creates a rejected free article.</summary>
    public static ArticleEntity CreateRejected(Guid categoryId) => new ArticleBuilder(categoryId).AsRejected().Build();

    /// <summary>Creates an approved free article.</summary>
    public static ArticleEntity CreateApproved(Guid categoryId) => new ArticleBuilder(categoryId).AsApproved().Build();

    /// <summary>Creates an article with a known valid slug (for slug-conflict tests).</summary>
    public static ArticleEntity CreateWithSlug(Guid categoryId, string slug) =>
        new ArticleBuilder(categoryId).WithSlug(slug).Build();

    /// <summary>Creates a list of free articles in Draft status.</summary>
    public static List<ArticleEntity> CreateMany(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(categoryId)).ToList();

    /// <summary>Creates a list of published articles.</summary>
    public static List<ArticleEntity> CreateManyPublished(Guid categoryId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreatePublished(categoryId)).ToList();

    /// <summary>Creates a featured article with a future expiry date.</summary>
    public static ArticleEntity CreateFeatured(Guid categoryId) =>
        new ArticleBuilder(categoryId).AsPublished().AsFeatured(DateTimeOffset.UtcNow.AddDays(7)).Build();

    /// <summary>Creates a free article in PendingReview status.</summary>
    public static ArticleEntity CreatePendingReview(Guid categoryId) =>
        new ArticleBuilder(categoryId).AsPendingReview().Build();

    /// <summary>Creates a paid article in PendingPayment status.</summary>
    public static ArticleEntity CreatePendingPayment(Guid categoryId, Guid customerId, Guid orderItemId) =>
        new ArticleBuilder(categoryId).WithCustomer(customerId, orderItemId).AsPendingPayment().Build();

    /// <summary>Creates an archived free article.</summary>
    public static ArticleEntity CreateArchived(Guid categoryId) => new ArticleBuilder(categoryId).AsArchived().Build();

    /// <summary>Creates a free article with the default known values from TestConstants.</summary>
    public static ArticleEntity CreateDefault(Guid categoryId) =>
        new ArticleBuilder(categoryId)
            .WithTitle(TestConstants.Content.Editorial.Article.ValidTitle)
            .WithSlug(TestConstants.Content.Editorial.Article.ValidSlug)
            .Build();
}
