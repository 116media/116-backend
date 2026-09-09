namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The public projection of a content author: display identity only.
/// </summary>
/// <param name="UserName">The author's display name.</param>
/// <param name="AvatarUrl">The resolved avatar URL, or null when none is set.</param>
public record PublicAuthorDto(string UserName, string? AvatarUrl);
