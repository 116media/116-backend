using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateTag;

/// <summary>
/// Command for creating a new content tag.
/// </summary>
/// <param name="Name">The display name of the tag (e.g., "Fally Ipupa", "Kinshasa").</param>
/// <param name="Slug">The URL-safe slug for the tag (e.g., "fally-ipupa", "kinshasa").</param>
public record CreateTagCommand(string Name, string Slug) : ICommand<CreateTagResult>;

/// <summary>
/// Result of the <see cref="CreateTagCommand" /> containing the created tag details.
/// </summary>
/// <param name="Tag">The created tag information.</param>
public record CreateTagResult(TagDto Tag);
