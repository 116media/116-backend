using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Command for updating user avatar via file upload.
/// </summary>
/// <param name="UserId">The ID of the user to update (from JWT claims).</param>
/// <param name="AvatarFile">The avatar image file to upload.</param>
/// <remarks>
/// This command allows logged-in users to update their avatar by uploading an image file.
/// The file is uploaded to Cloudinary, and the previous avatar is automatically deleted.
/// Only verified accounts can update their avatar.
/// </remarks>
public record PublicUpdateAvatarCommand(Guid UserId, IFormFile AvatarFile) : ICommand<PublicUpdateAvatarResult>;

/// <summary>
/// Result of the <see cref="PublicUpdateAvatarCommand" /> containing the updated user information.
/// </summary>
/// <param name="User">The updated user information with the new avatar.</param>
/// <remarks>
/// Contains the complete user information including the new avatar details.
/// </remarks>
public record PublicUpdateAvatarResult(UserResponseDto User);
