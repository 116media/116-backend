using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile.Contracts;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="AdminUpdateOwnProfileCommand" /> to update admin user's own profile information.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <param name="authFactory">Factory for handling admin user profile update logic.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class AdminUpdateOwnProfileHandler(IAdminUpdateProfileAuthFactory authFactory, IFileRepository fileRepository)
    : ICommandHandler<AdminUpdateOwnProfileCommand, AdminUpdateOwnProfileResult>
{
    /// <summary>
    /// Handles the profile update command by validating uniqueness and updating admin user information.
    /// </summary>
    /// <param name="command">The profile update command containing user ID and new profile data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminUpdateOwnProfileResult" /> containing updated admin user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="ConflictException">Thrown when username or phone number already exists.</exception>
    public async Task<AdminUpdateOwnProfileResult> Handle(
        AdminUpdateOwnProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        AdminUpdateProfileAuthData authData = await authFactory.UpdateProfileAsync(
            userId: command.UserId,
            sessionId: command.SessionId,
            userName: command.UserName,
            countryName: command.CountryName,
            countryIsoCode: command.CountryIsoCode,
            countryDialCode: command.CountryDialCode,
            partialPhoneNumber: command.PartialPhoneNumber,
            cancellationToken: cancellationToken
        );

        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(
            avatarFileId: authData.User.AvatarFileId,
            cancellationToken: cancellationToken
        );

        var avatarDto = avatarFile?.ToFileDto();
        var userDto = authData.User.ToUserResponseDto(
            roles: authData.Roles,
            permissions: authData.Permissions,
            avatar: avatarDto
        );
        return new AdminUpdateOwnProfileResult(User: userDto);
    }
}
