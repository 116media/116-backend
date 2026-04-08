using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Command for creating a new content category.
/// </summary>
/// <param name="ContentTypeId">The identifier of the content type this category belongs to.</param>
/// <param name="Name">The display name of the category.</param>
/// <param name="Slug">The URL-safe slug for the category.</param>
/// <param name="Description">The description of the category.</param>
/// <param name="IsFree">Whether content in this category requires no payment.</param>
public record AdminCreateCategoryCommand(
    string ContentTypeId,
    string Name,
    string Slug,
    string Description,
    bool IsFree
) : ICommand<AdminCreateCategoryResult>;

/// <summary>
/// Result of the <see cref="AdminCreateCategoryCommand" /> containing the created category details.
/// </summary>
/// <param name="Category">The created category information.</param>
public record AdminCreateCategoryResult(CategoryDto Category);
