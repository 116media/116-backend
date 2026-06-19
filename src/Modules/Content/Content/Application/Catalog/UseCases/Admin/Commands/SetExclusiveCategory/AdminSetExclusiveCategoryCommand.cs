using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Command for setting a category as the exclusive show on the homepage.
/// </summary>
/// <param name="Id">The unique identifier of the category to make exclusive.</param>
public record AdminSetExclusiveCategoryCommand(string Id) : ICommand<AdminSetExclusiveCategoryResult>;

/// <summary>
/// Result of the <see cref="AdminSetExclusiveCategoryCommand" /> containing the updated category.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminSetExclusiveCategoryResult(CategoryDto Category);
