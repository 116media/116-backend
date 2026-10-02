namespace _116.Identity.Contracts.Application.DTOs;

/// <summary>
/// The display identity of one user account, resolved from the Identity module. Consumers name it
/// for the role it plays in their own use case, be that an author, a submitter or an owner.
/// </summary>
/// <param name="UserName">The display name.</param>
/// <param name="Email">The email address, or null when the account has none.</param>
/// <param name="AvatarFileId">The avatar's file ID, or null. Resolved to a URL by the consuming module.</param>
/// <param name="Role">The primary role name, for example "SuperAdmin" or "Admin".</param>
/// <param name="PreferredLocale">The locale this account's mail and notifications render in.</param>
public record UserProfileDto(string UserName, string? Email, Guid? AvatarFileId, string? Role, string PreferredLocale);
