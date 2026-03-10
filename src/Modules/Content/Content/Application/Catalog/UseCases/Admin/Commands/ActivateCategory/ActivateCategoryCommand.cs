using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Command to activate a category, making it available for content creation and orders.
/// </summary>
/// <param name="Id">The unique identifier of the category to activate.</param>
public record ActivateCategoryCommand(Guid Id) : ICommand<ActivateCategoryResult>;

/// <summary>
/// Result returned after successfully activating a category.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record ActivateCategoryResult(CategoryDto Category);
