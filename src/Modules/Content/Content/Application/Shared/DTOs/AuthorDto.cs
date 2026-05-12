namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an article/video author profile.
/// Resolved from the Identity module's <c>AuthorInfo</c> with
/// the avatar file ID replaced by the actual Cloudinary URL.
/// </summary>
/// <param name="UserName">The author's display name.</param>
/// <param name="Email">The author's email address, or null.</param>
/// <param name="AvatarUrl">The author's avatar URL from Cloudinary, or null.</param>
/// <param name="Role">The author's primary role name (e.g., "SuperAdmin"), or null.</param>
public record AuthorDto(string UserName, string? Email, string? AvatarUrl, string? Role);
