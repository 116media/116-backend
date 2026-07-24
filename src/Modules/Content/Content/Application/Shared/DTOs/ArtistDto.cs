namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an artist profile.
/// </summary>
/// <param name="Id">
/// The unique identifier of the artist profile.
/// </param>
/// <param name="Name">
/// The artist's display name.
/// </param>
/// <param name="Slug">
/// The URL-safe slug used in the artist's public page URL.
/// </param>
/// <param name="Bio">
/// The artist's free-text biography, or null if not yet curated.
/// </param>
/// <param name="AvatarUrl">
/// The publicly accessible URL of the artist's avatar image, resolved from the associated
/// FileEntity. Null if no avatar has been uploaded.
/// </param>
public record ArtistDto(Guid Id, string Name, string Slug, string? Bio, string? AvatarUrl);
