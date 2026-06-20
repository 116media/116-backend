using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UnpinCategoryFromFeed;

/// <summary>
/// Command for unpinning a category from the content feed.
/// </summary>
/// <param name="Id">The unique identifier of the category to unpin.</param>
public record AdminUnpinCategoryFromFeedCommand(string Id) : ICommand<AdminUnpinCategoryFromFeedResult>;

/// <summary>
/// Result of the <see cref="AdminUnpinCategoryFromFeedCommand" /> containing the updated category.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminUnpinCategoryFromFeedResult(CategoryDto Category);
