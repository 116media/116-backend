using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Command for creating a new content tag.
/// </summary>
/// <param name="Name">The display name of the tag (e.g., "Fally Ipupa", "Kinshasa").</param>
public record AdminCreateTagCommand(string Name) : ICommand<AdminCreateTagResult>;

/// <summary>
/// Result of the <see cref="AdminCreateTagCommand" /> containing the created tag details.
/// </summary>
/// <param name="Tag">The created tag information.</param>
public record AdminCreateTagResult(TagDto Tag);
