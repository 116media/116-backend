using _116.Identity.Domain.Enums;
using _116.Storage.Contracts.Application.DTOs;

namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// The public projection of the authenticated user's own profile. Carries no audit trail —
/// that stays on <see cref="UserResponseDto" /> for admin.
/// </summary>
public record PublicUserResponseDto(
    Guid Id,
    string? Email,
    string UserName,
    IReadOnlyCollection<PublicRoleDto> Roles,
    IReadOnlyCollection<PublicPermissionDto> Permissions,
    EnumAuthProvider AuthProvider,
    bool IsVerified,
    bool IsActive,
    PublicFileDto? Avatar,
    string? CountryName,
    string? CountryIsoCode,
    string? CountryDialCode,
    string? PartialPhoneNumber,
    string? FullPhoneNumber
);
