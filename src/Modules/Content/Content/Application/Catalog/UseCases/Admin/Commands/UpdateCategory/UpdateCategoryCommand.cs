using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Command for updating an existing category's display name, slug, and description.
/// </summary>
/// <param name="Id">The unique identifier of the category to update.</param>
/// <param name="Name">The new display name for the category.</param>
/// <param name="Slug">The new URL-safe slug for the category.</param>
/// <param name="Description">The new optional description.</param>
public record UpdateCategoryCommand(string Id, string Name, string Slug, string? Description)
    : ICommand<UpdateCategoryResult>;

/// <summary>
/// Result of the <see cref="UpdateCategoryCommand" /> containing the updated category details.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record UpdateCategoryResult(CategoryDto Category);
