using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile.Contracts;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Public.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="PublicUpdateOwnProfileCommand" /> to update user's own profile information.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
/// <param name="authFactory">Factory for handling user profile update logic.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class PublicUpdateOwnProfileHandler(IPublicUpdateProfileAuthFactory authFactory, IFileRepository fileRepository)
    : ICommandHandler<PublicUpdateOwnProfileCommand, PublicUpdateOwnProfileResult>
{
    /// <summary>
    /// Handles the profile update command by validating uniqueness and updating user information.
    /// </summary>
    /// <param name="command">The profile update command containing user ID and new profile data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicUpdateOwnProfileResult" /> containing updated user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    /// <exception cref="ConflictException">Thrown when email, username, or phone number already exists.</exception>
    public async Task<PublicUpdateOwnProfileResult> Handle(
        PublicUpdateOwnProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        PublicUpdateProfileAuthData authData = await authFactory.UpdateProfileAsync(
            userId: command.UserId,
            sessionId: command.SessionId,
            email: command.Email,
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
            avatar: avatarDto,
            roles: authData.Roles,
            permissions: authData.Permissions
        );
        return new PublicUpdateOwnProfileResult(User: userDto);
    }
}
