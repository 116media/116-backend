namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a content tag.
/// </summary>
/// <param name="Id">The unique identifier of the tag.</param>
/// <param name="Name">The display name of the tag.</param>
/// <param name="Slug">The URL-safe slug for the tag.</param>
public record TagDto(Guid Id, string Name, string Slug);
