namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// The admin projection of a content author, including the contact address shown in the
/// editorial dashboard.
/// </summary>
/// <param name="UserName">The author's display name.</param>
/// <param name="Email">The author's contact address.</param>
/// <param name="AvatarUrl">The resolved avatar URL, or null when none is set.</param>
/// <param name="Role">The author's role label.</param>
public record AdminAuthorDto(string UserName, string? Email, string? AvatarUrl, string? Role);
