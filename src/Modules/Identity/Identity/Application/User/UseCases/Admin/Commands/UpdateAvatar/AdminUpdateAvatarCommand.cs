using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Command for updating admin user avatar via file upload.
/// </summary>
/// <param name="UserId">The ID of the admin user to update (from JWT claims).</param>
/// <param name="AvatarFile">The avatar image file to upload.</param>
/// <remarks>
/// This command allows logged-in admin users to update their avatar by uploading an image file.
/// The file is uploaded to Cloudinary, and the previous avatar is automatically deleted.
/// Only active accounts can update their avatar.
/// </remarks>
public record AdminUpdateAvatarCommand(
    Guid UserId,
    IFormFile AvatarFile
) : ICommand<AdminUpdateAvatarResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateAvatarCommand" /> containing the updated admin user information.
/// </summary>
/// <param name="User">The updated admin user information with the new avatar.</param>
/// <remarks>
/// Contains the complete admin user information including the new avatar details.
/// </remarks>
public record AdminUpdateAvatarResult(
    UserResponseDto User
);
