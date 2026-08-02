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
/// <param name="IsGossip">Whether this is the gossip category used for homepage feed fallbacks and the gossip strip.</param>
/// <param name="IsExclusive">Whether this category is the exclusive show featured on the homepage.</param>
/// <param name="IsDefaultForLyrics">
/// Whether this is the default category community-originated lyrics pages are filed under.
/// At most one category holds this flag; setting it clears the previous holder.
/// </param>
public record AdminUpdateCategoryCommand(
    string Id,
    string Name,
    string Slug,
    string Description,
    bool IsGossip,
    bool IsExclusive,
    bool IsDefaultForLyrics
) : ICommand<AdminUpdateCategoryResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateCategoryCommand" /> containing the updated category details.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminUpdateCategoryResult(CategoryDto Category);
