using _116.Shared.Contracts.Application.CQRS;
using _116.User.Domain.DTOs;

namespace _116.User.Application.Public.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Command for updating user avatar.
/// </summary>
/// <param name="UserId">The ID of the user to update (from JWT claims).</param>
/// <param name="AvatarUrl">The new avatar URL to set for the user.</param>
/// <remarks>
/// This command allows logged-in users to update their avatar by providing a new avatar URL.
/// The previous avatar file will be deleted if it exists. Only verified accounts can update their avatar.
/// </remarks>
public record PublicUpdateAvatarCommand(
    Guid UserId,
    string AvatarUrl
) : ICommand<PublicUpdateAvatarResult>;

/// <summary>
/// Result of the <see cref="PublicUpdateAvatarCommand"/> containing the updated user information.
/// </summary>
/// <param name="User">The updated user information with the new avatar.</param>
/// <remarks>
/// Contains the complete user information including the new avatar details.
/// </remarks>
public record PublicUpdateAvatarResult(
    UserResponseDto User
);
