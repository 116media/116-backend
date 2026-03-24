using _116.Core.Application.Shared.DTOs;
using _116.Shared.Application.DTOs;

namespace _116.Identity.Application.Shared.DTOs;

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
/// <param name="Avatar">Avatar file information for UI display, if uploaded</param>
/// <param name="CountryName">Full country name associated with the user</param>
/// <param name="CountryIsoCode">ISO country code (e.g., "US", "RW")</param>
/// <param name="CountryDialCode">Country dialing code (e.g., "+1", "+250")</param>
/// <param name="PartialPhoneNumber">Partial (masked) phone number for privacy display</param>
/// <param name="FullPhoneNumber">Full phone number including country code (sensitive data)</param>
public record UserResponseDto(
    Guid Id,
    string? Email,
    string UserName,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions,
    string AuthProvider,
    bool IsVerified,
    bool IsActive,
    FileDto? Avatar,
    string? CountryName,
    string? CountryIsoCode,
    string? CountryDialCode,
    string? PartialPhoneNumber,
    string? FullPhoneNumber
) : AuditableDto;
