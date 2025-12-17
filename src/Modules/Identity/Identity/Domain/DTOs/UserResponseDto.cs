using _116.Core.Domain.DTOs;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Domain.DTOs;

/// <summary>
/// Data transfer object representing comprehensive user information in API responses.
/// Includes roles, permissions, and avatar file details for complete UI display.
/// </summary>
/// <param name="Id">The unique identifier of the user</param>
/// <param name="Email">The user's email address (maybe null for external auth providers)</param>
/// <param name="UserName">The user's unique username</param>
/// <param name="Roles">Collection of roles assigned to the user with their permissions</param>
/// <param name="Permissions">Collection of permissions associated with this role</param>
/// <param name="AuthProvider">The authentication provider used by the user</param>
/// <param name="IsVerified">Whether the user's account is verified</param>
/// <param name="IsActive">Whether the user's account is active</param>
/// <param name="IsLoggedIn">Whether the user is currently logged in</param>
/// <param name="LastLoginAt">Date and time of the user's last login in UTC</param>
/// <param name="Avatar">Avatar file information for UI display, if uploaded</param>
/// <param name="CountryName">Full country name associated with the user</param>
/// <param name="CountryFlagUrl">URL link to the flag of the user's country</param>
/// <param name="CountryIsoCode">ISO country code (e.g., "US", "RW")</param>
/// <param name="CountryDialCode">Country dialing code (e.g., "+1", "+250")</param>
/// <param name="PartialPhoneNumber">Partial (masked) phone number for privacy display</param>
/// <param name="FullPhoneNumber">Full phone number including country code (sensitive data)</param>
/// <param name="CreatedAt">Date and time when the user account was created</param>
/// <param name="UpdatedAt">Date and time when the user account was last updated</param>
public record UserResponseDto(
    Guid Id,
    string? Email,
    string UserName,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions,
    string AuthProvider,
    bool IsVerified,
    bool IsActive,
    bool IsLoggedIn,
    DateTime? LastLoginAt,
    FileDto? Avatar,
    string? CountryName,
    string? CountryFlagUrl,
    string? CountryIsoCode,
    string? CountryDialCode,
    string? PartialPhoneNumber,
    string? FullPhoneNumber,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);
