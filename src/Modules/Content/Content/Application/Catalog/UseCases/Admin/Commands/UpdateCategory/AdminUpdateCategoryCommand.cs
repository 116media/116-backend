using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Command for updating an existing category's display name, slug, and description.
/// </summary>
/// <param name="Id">The unique identifier of the category to update.</param>
/// <param name="Name">The new display name for the category.</param>
/// <param name="Slug">The new URL-safe slug for the category.</param>
/// <param name="Description">The new description.</param>
public record AdminUpdateCategoryCommand(string Id, string Name, string Slug, string Description)
    : ICommand<AdminUpdateCategoryResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateCategoryCommand" /> containing the updated category details.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminUpdateCategoryResult(CategoryDto Category);
