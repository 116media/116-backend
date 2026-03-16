using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Command to deactivate a category, preventing it from being used in new content or orders.
/// </summary>
/// <param name="Id">The unique identifier of the category to deactivate.</param>
public record AdminDeactivateCategoryCommand(string Id) : ICommand<AdminDeactivateCategoryResult>;

/// <summary>
/// Result returned after successfully deactivating a category.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminDeactivateCategoryResult(CategoryDto Category);
