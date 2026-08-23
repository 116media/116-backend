using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="CategoryBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class CategoryFactory
{
    /// <summary>
    /// Creates a category with a specific content type ID.
    /// </summary>
    public static CategoryEntity Create(Guid contentTypeId, bool isExclusive = false) =>
        new CategoryBuilder(contentTypeId).WithIsExclusive(isExclusive).Build();

    /// <summary>
    /// Creates a category with specific name and slug.
    /// </summary>
    public static CategoryEntity Create(Guid contentTypeId, string name, string slug, bool isExclusive = false) =>
        new CategoryBuilder(contentTypeId).WithName(name).WithSlug(slug).WithIsExclusive(isExclusive).Build();

    /// <summary>
    /// Creates a category with a content type navigation property populated.
    /// </summary>
    public static CategoryEntity Create(ContentTypeEntity contentType, bool isExclusive = false) =>
        new CategoryBuilder(contentType.Id).WithContentType(contentType).WithIsExclusive(isExclusive).Build();

    /// <summary>
    /// Creates an inactive category.
    /// </summary>
    public static CategoryEntity CreateInactive(Guid contentTypeId) =>
        new CategoryBuilder(contentTypeId).AsInactive().Build();

    /// <summary>
    /// Creates a free category (no payment required).
    /// </summary>
    public static CategoryEntity CreateFree(Guid contentTypeId) => new CategoryBuilder(contentTypeId).AsFree().Build();

    /// <summary>
    /// Creates a paid category.
    /// </summary>
    public static CategoryEntity CreatePaid(Guid contentTypeId) => new CategoryBuilder(contentTypeId).AsPaid().Build();

    /// <summary>
    /// Creates a category with default known values.
    /// </summary>
    public static CategoryEntity CreateDefault(Guid contentTypeId) =>
        new CategoryBuilder(contentTypeId)
            .WithName(TestConstants.Category.ValidName)
            .WithSlug(TestConstants.Category.ValidSlug)
            .Build();

    /// <summary>
    /// Creates a gossip category (IsGossip = true) with the given content type ID.
    /// Used to exercise the <c>GossipCategorySpecification</c> and gossip fallback logic
    /// in the article promotion feed.
    /// </summary>
    public static CategoryEntity CreateGossip(Guid contentTypeId) =>
        new CategoryBuilder(contentTypeId).AsGossip().Build();

    /// <summary>
    /// Creates a category marked as the default category for lyrics pages
    /// (IsDefaultForLyrics = true) with the given content type ID.
    /// </summary>
    public static CategoryEntity CreateDefaultForLyrics(Guid contentTypeId) =>
        new CategoryBuilder(contentTypeId).AsDefaultForLyrics().Build();

    /// <summary>
    /// Creates a list of categories with the specified count.
    /// </summary>
    public static List<CategoryEntity> CreateMany(Guid contentTypeId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(contentTypeId)).ToList();

    /// <summary>
    /// Creates a category pinned to the feed at the given time (defaults to "now"),
    /// with its content type navigation populated.
    /// </summary>
    public static CategoryEntity CreatePinned(ContentTypeEntity contentType, DateTimeOffset? pinnedAt = null) =>
        new CategoryBuilder(contentType.Id).WithContentType(contentType).PinnedToFeedAt(pinnedAt).Build();

    /// <summary>
    /// Creates several categories of a content type, each pinned at staggered times so the
    /// oldest is deterministic (index 0 = oldest).
    /// </summary>
    public static List<CategoryEntity> CreateManyPinned(
        ContentTypeEntity contentType,
        int count,
        DateTimeOffset baseTime
    ) => Enumerable.Range(0, count).Select(i => CreatePinned(contentType, baseTime.AddMinutes(i))).ToList();

    /// <summary>
    /// Creates a category pinned to the feed by content type id (no navigation populated).
    /// Suited to integration seeding where the content type is added to the context separately.
    /// </summary>
    public static CategoryEntity CreatePinned(Guid contentTypeId, DateTimeOffset? pinnedAt = null) =>
        new CategoryBuilder(contentTypeId).PinnedToFeedAt(pinnedAt).Build();

    /// <summary>
    /// Creates several categories pinned by content type id at staggered times (index 0 = oldest).
    /// </summary>
    public static List<CategoryEntity> CreateManyPinned(Guid contentTypeId, int count, DateTimeOffset baseTime) =>
        Enumerable.Range(0, count).Select(i => CreatePinned(contentTypeId, baseTime.AddMinutes(i))).ToList();

    /// <summary>
    /// Creates a category pinned to the feed with a poster file reference, by content type id.
    /// The referenced <paramref name="posterFileId" /> must point to a persisted file when used
    /// in integration tests so the poster URL can be resolved.
    /// </summary>
    public static CategoryEntity CreatePinnedWithPoster(
        Guid contentTypeId,
        Guid posterFileId,
        DateTimeOffset? pinnedAt = null
    ) => new CategoryBuilder(contentTypeId).WithPosterFileId(posterFileId).PinnedToFeedAt(pinnedAt).Build();
}
