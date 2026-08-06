using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="ArticleImageBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class ArticleImageFactory
{
    /// <summary>
    /// Creates a body image for the specified article.
    /// </summary>
    public static ArticleImageEntity Create(Guid articleId) => new ArticleImageBuilder(articleId).AsBody().Build();

    /// <summary>
    /// Creates a cover image for the specified article.
    /// </summary>
    public static ArticleImageEntity CreateCover(Guid articleId) =>
        new ArticleImageBuilder(articleId)
            .AsCover()
            .WithStorageKey(TestConstants.ArticleImage.ValidStorageKey)
            .WithUrl(TestConstants.ArticleImage.ValidUrl)
            .Build();

    /// <summary>
    /// Creates a body image for the specified article.
    /// </summary>
    public static ArticleImageEntity CreateBody(Guid articleId) =>
        new ArticleImageBuilder(articleId)
            .AsBody()
            .WithStorageKey(TestConstants.ArticleImage.AnotherStorageKey)
            .WithUrl(TestConstants.ArticleImage.AnotherUrl)
            .Build();

    /// <summary>
    /// Creates a cover image with specific storage key and URL.
    /// </summary>
    public static ArticleImageEntity CreateCover(Guid articleId, string storageKey, string url) =>
        new ArticleImageBuilder(articleId).AsCover().WithStorageKey(storageKey).WithUrl(url).Build();

    /// <summary>
    /// Creates a body image with specific storage key and URL.
    /// </summary>
    public static ArticleImageEntity CreateBody(Guid articleId, string storageKey, string url) =>
        new ArticleImageBuilder(articleId).AsBody().WithStorageKey(storageKey).WithUrl(url).Build();

    /// <summary>
    /// Creates a list of body images for the specified article.
    /// </summary>
    public static List<ArticleImageEntity> CreateMany(Guid articleId, int count) =>
        Enumerable
            .Range(0, count)
            .Select(i =>
                new ArticleImageBuilder(articleId)
                    .AsBody()
                    .WithStorageKey($"content/articles/image-{i}")
                    .WithUrl($"https://res.cloudinary.com/test/image/upload/v1/image-{i}.jpg")
                    .Build()
            )
            .ToList();

    /// <summary>
    /// Creates a list of article images from raw storage keys (for delete tests).
    /// </summary>
    public static List<ArticleImageEntity> CreateFromStorageKeys(Guid articleId, IEnumerable<string> storageKeys) =>
        storageKeys
            .Select(key =>
                new ArticleImageBuilder(articleId)
                    .AsBody()
                    .WithStorageKey(key)
                    .WithUrl($"https://res.cloudinary.com/test/{key}.jpg")
                    .Build()
            )
            .ToList();
}
