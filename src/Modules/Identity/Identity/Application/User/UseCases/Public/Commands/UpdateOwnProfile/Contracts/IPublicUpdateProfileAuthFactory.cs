using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;

/// <summary>
/// Contains updated user data and associated roles/permissions.
/// </summary>
public record PublicUpdateProfileAuthData(
    UserEntity User,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling user profile update logic.
/// </summary>
public interface IPublicUpdateProfileAuthFactory
{
    /// <summary>
    /// Updates a user's profile with new information.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="email">The new email address (optional).</param>
    /// <param name="userName">The new username (optional).</param>
    /// <param name="countryName">The country name for phone number (optional).</param>
    /// <param name="countryIsoCode">The country ISO code for phone number (optional).</param>
    /// <param name="countryDialCode">The country dial code for phone number (optional).</param>
    /// <param name="partialPhoneNumber">The partial phone number without country code (optional).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Update data containing user, roles, and permissions.</returns>
    Task<PublicUpdateProfileAuthData> UpdateProfileAsync(
        Guid userId,
        string? email,
        string? userName,
        string? countryName,
        string? countryIsoCode,
        string? countryDialCode,
        string? partialPhoneNumber,
        CancellationToken cancellationToken
    );
}
