using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.PinCategoryToFeed;

/// <summary>
/// Command for pinning a category to the content feed so it appears as a feed section.
/// </summary>
/// <param name="Id">The unique identifier of the category to pin.</param>
public record AdminPinCategoryToFeedCommand(string Id) : ICommand<AdminPinCategoryToFeedResult>;

/// <summary>
/// Result of the <see cref="AdminPinCategoryToFeedCommand" /> containing the updated category.
/// </summary>
/// <param name="Category">The updated category information.</param>
public record AdminPinCategoryToFeedResult(CategoryDto Category);
