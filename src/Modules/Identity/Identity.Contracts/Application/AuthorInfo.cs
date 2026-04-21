namespace _116.Identity.Contracts.Application;

/// <summary>
/// Lightweight author profile resolved from the Identity module.
/// Used by Content module to embed author details in article/video DTOs
/// without a direct dependency on the Identity domain.
/// </summary>
/// <param name="UserName">The author's display name.</param>
/// <param name="Email">The author's email address.</param>
/// <param name="AvatarFileId">The file ID of the author's avatar, or null. Resolved to a URL by the consuming module.</param>
/// <param name="Role">The author's primary role name (e.g., "SuperAdmin", "Admin").</param>
public record AuthorInfo(string UserName, string? Email, Guid? AvatarFileId, string? Role);
