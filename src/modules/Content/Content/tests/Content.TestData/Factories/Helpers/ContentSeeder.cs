using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.TestData.Factories.Helpers;

/// <summary>
/// Adds the lookup rows Content tests need before their own subject, so each suite states the
/// arrangement once instead of repeating it.
/// </summary>
public static class ContentSeeder
{
    /// <summary>
    /// Adds a content type and a category filed under it.
    /// </summary>
    /// <param name="context">The context to add the rows to; the caller saves.</param>
    /// <returns>The new category's identifier.</returns>
    public static Guid AddCategory(ContentDbContext context)
    {
        ContentTypeEntity contentType = ContentTypeFactory.Create();
        context.ContentTypes.Add(contentType);

        CategoryEntity category = CategoryFactory.Create(contentType.Id);
        context.Categories.Add(category);

        return category.Id;
    }
}
