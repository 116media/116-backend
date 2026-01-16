using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;

/// <summary>
/// Contains updated admin user data and associated roles/permissions.
/// </summary>
public record AdminUpdateProfileAuthData(
    UserEntity User,
    IReadOnlyCollection<RoleDto> Roles,
    IReadOnlyCollection<PermissionDto> Permissions
);

/// <summary>
/// Factory for handling admin user profile update logic.
/// </summary>
public interface IAdminUpdateProfileAuthFactory
{
    /// <summary>
    /// Updates an admin user's profile with new information.
    /// </summary>
    /// <param name="userId">The ID of the admin user to update.</param>
    /// <param name="sessionId">The ID of the current session.</param>
    /// <param name="userName">The new username (optional).</param>
    /// <param name="countryName">The country name for phone number (optional).</param>
    /// <param name="countryIsoCode">The country ISO code for phone number (optional).</param>
    /// <param name="countryDialCode">The country dial code for phone number (optional).</param>
    /// <param name="partialPhoneNumber">The partial phone number without country code (optional).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Update data containing user, roles, and permissions.</returns>
    Task<AdminUpdateProfileAuthData> UpdateProfileAsync(
        Guid userId,
        Guid sessionId,
        string? userName,
        string? countryName,
        string? countryIsoCode,
        string? countryDialCode,
        string? partialPhoneNumber,
        CancellationToken cancellationToken
    );
}
