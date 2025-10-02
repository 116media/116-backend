using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Domain.DTOs;

namespace _116.Auth.Application.Admin.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Command for updating an admin user's own profile information.
/// </summary>
/// <param name="UserId">The unique identifier of the admin user updating their profile.</param>
/// <param name="UserName">The new username (optional).</param>
/// <param name="CountryName">The new country name (optional).</param>
/// <param name="CountryFlagUrl">The new country flag URL (optional).</param>
/// <param name="PartialPhoneNumber">The new partial phone number (optional).</param>
/// <param name="CountryIsoCode">The new country ISO code (optional).</param>
/// <param name="CountryDialCode">The new country dial code (optional).</param>
/// <remarks>
/// This command allows authenticated admin users to update their own profile information.
/// Email updates are not allowed for admin users to maintain security.
/// All phone and country information is updated together to maintain consistency.
/// Admin users must be authenticated and active to update their profile.
/// </remarks>
public record AdminUpdateOwnProfileCommand(
    Guid UserId,
    string? UserName,
    string? CountryName,
    string? CountryFlagUrl,
    string? PartialPhoneNumber,
    string? CountryIsoCode,
    string? CountryDialCode
) : ICommand<AdminUpdateOwnProfileResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateOwnProfileCommand"/> containing updated admin user profile data.
/// </summary>
/// <param name="User">The updated admin user profile information including roles and permissions.</param>
/// <remarks>
/// Contains the complete updated admin user profile with all user details, roles, permissions, and avatar information.
/// This information reflects the changes made during the update operation.
/// </remarks>
public record AdminUpdateOwnProfileResult(
    UserResponseDto User
);
