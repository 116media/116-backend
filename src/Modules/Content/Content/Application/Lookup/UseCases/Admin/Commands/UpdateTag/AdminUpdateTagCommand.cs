using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateTag;

/// <summary>
/// Command for updating an existing content tag's name and slug.
/// </summary>
/// <param name="Id">The unique identifier of the tag to update.</param>
/// <param name="Name">The new display name for the tag.</param>
/// <param name="Slug">The new URL-safe slug for the tag.</param>
public record AdminUpdateTagCommand(string Id, string Name, string Slug) : ICommand<AdminUpdateTagResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateTagCommand" /> containing the updated tag details.
/// </summary>
/// <param name="Tag">The updated tag information.</param>
public record AdminUpdateTagResult(TagDto Tag);
