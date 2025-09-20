using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.DTOs;

namespace _116.User.Application.Admin.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Command for updating admin user avatar.
/// </summary>
/// <param name="UserId">The ID of the admin user to update (from JWT claims).</param>
/// <param name="AvatarUrl">The new avatar URL to set for the admin user.</param>
/// <remarks>
/// This command allows logged-in admin users to update their avatar by providing a new avatar URL.
/// The previous avatar file will be deleted if it exists. Only active accounts can update their avatar.
/// </remarks>
public record AdminUpdateAvatarCommand(
    Guid UserId,
    string AvatarUrl
) : ICommand<AdminUpdateAvatarResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateAvatarCommand"/> containing the updated admin user information.
/// </summary>
/// <param name="User">The updated admin user information with the new avatar.</param>
/// <remarks>
/// Contains the complete admin user information including the new avatar details.
/// </remarks>
public record AdminUpdateAvatarResult(
    UserResponseDto User
);
