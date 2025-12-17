using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.Public.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Command for updating a user's own profile information.
/// </summary>
/// <param name="UserId">The unique identifier of the user updating their profile.</param>
/// <param name="Email">The new email address (optional).</param>
/// <param name="UserName">The new username (optional).</param>
/// <param name="CountryName">The new country name (optional).</param>
/// <param name="CountryFlagUrl">The new country flag URL (optional).</param>
/// <param name="PartialPhoneNumber">The new partial phone number (optional).</param>
/// <param name="CountryIsoCode">The new country ISO code (optional).</param>
/// <param name="CountryDialCode">The new country dial code (optional).</param>
/// <remarks>
/// This command allows authenticated users to update their own profile information.
/// If the email is updated, the user's verification status will be reset, and they will be logged out.
/// All phone and country information is updated together to maintain consistency.
/// Users must be authenticated, active, and verified to update their profile.
/// </remarks>
public record PublicUpdateOwnProfileCommand(
    Guid UserId,
    string? Email,
    string? UserName,
    string? CountryName,
    string? CountryFlagUrl,
    string? PartialPhoneNumber,
    string? CountryIsoCode,
    string? CountryDialCode
) : ICommand<PublicUpdateOwnProfileResult>;
/// <summary>
/// Result of the <see cref="PublicUpdateOwnProfileCommand"/> containing updated user profile data.
/// </summary>
/// <param name="User">The updated user profile information including roles and permissions.</param>
/// <remarks>
/// Contains the complete updated user profile with all user details, roles, permissions, and avatar information.
/// This information reflects the changes made during the update operation.
/// </remarks>
public record PublicUpdateOwnProfileResult(
    UserResponseDto User
);
