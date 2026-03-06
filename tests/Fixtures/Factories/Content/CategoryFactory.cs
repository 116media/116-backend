using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="CategoryEntity"/> instances in tests.
/// </summary>
public static class CategoryFactory
{
    /// <summary>Creates a category with a specific content type ID.</summary>
    public static CategoryEntity Create(Guid contentTypeId) => new CategoryBuilder(contentTypeId).Build();

    /// <summary>Creates a category with specific name and slug.</summary>
    public static CategoryEntity Create(Guid contentTypeId, string name, string slug) =>
        new CategoryBuilder(contentTypeId).WithName(name).WithSlug(slug).Build();

    /// <summary>Creates an inactive category.</summary>
    public static CategoryEntity CreateInactive(Guid contentTypeId) =>
        new CategoryBuilder(contentTypeId).AsInactive().Build();

    /// <summary>Creates a free category (no payment required).</summary>
    public static CategoryEntity CreateFree(Guid contentTypeId) => new CategoryBuilder(contentTypeId).AsFree().Build();

    /// <summary>Creates a paid category.</summary>
    public static CategoryEntity CreatePaid(Guid contentTypeId) => new CategoryBuilder(contentTypeId).AsPaid().Build();

    /// <summary>Creates a category with default known values.</summary>
    public static CategoryEntity CreateDefault(Guid contentTypeId) =>
        new CategoryBuilder(contentTypeId)
            .WithName(TestConstants.Content.Category.ValidName)
            .WithSlug(TestConstants.Content.Category.ValidSlug)
            .Build();

    /// <summary>Creates a list of categories with the specified count.</summary>
    public static List<CategoryEntity> CreateMany(Guid contentTypeId, int count) =>
        Enumerable.Range(0, count).Select(_ => Create(contentTypeId)).ToList();
}
