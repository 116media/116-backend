using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Persistence;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Errors;
using _116.Auth.Application.Shared.Mappers;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Domain.Entities;
using _116.Auth.Domain.ValueObjects;

namespace _116.Auth.Application.Public.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="PublicUpdateOwnProfileCommand"/> to update user's own profile information.
/// This endpoint requires user authentication - only logged-in users can update their own profile.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicUpdateOwnProfileHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<PublicUpdateOwnProfileCommand, PublicUpdateOwnProfileResult>
{
    /// <summary>
    /// Handles the profile update command by validating uniqueness and updating user information.
    /// </summary>
    /// <param name="command">The profile update command containing user ID and new profile data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicUpdateOwnProfileResult"/> containing updated user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    /// <exception cref="ConflictException">Thrown when email, username, or phone number already exists.</exception>
    public async Task<PublicUpdateOwnProfileResult> Handle(
        PublicUpdateOwnProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            command.UserId,
            cancellationToken
        );

        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);

        bool isPhoneUpdated = !string.IsNullOrEmpty(command.PartialPhoneNumber);
        bool isUsernameUpdated = !string.IsNullOrEmpty(command.UserName) && user!.UserName != command.UserName;
        bool isEmailUpdated = !string.IsNullOrEmpty(command.Email) && user!.Email != command.Email?.ToLowerInvariant();

        // Validate uniqueness for email if being updated - check against other users
        if (isEmailUpdated)
        {
            await EnsureEmailUnique(command.Email!, cancellationToken);
            user!.UpdateEmail(command.Email!);
        }

        // Validate uniqueness for username if being updated - check against other users
        if (isUsernameUpdated)
        {
            await EnsureUsernameUnique(command.UserName!, cancellationToken);
            user!.UpdateUserName(command.UserName!);
        }

        // Check phone number uniqueness if being updated - check against other users
        if (isPhoneUpdated)
        {
            await EnsurePhoneUnique(command, cancellationToken);

            user!.UpdatePhoneNumber(
                countryName: command.CountryName,
                countryFlagUrl: command.CountryFlagUrl,
                countryIsoCode: command.CountryIsoCode,
                countryDialCode: command.CountryDialCode,
                partialPhoneNumber: command.PartialPhoneNumber,
                fullPhoneNumber: $"{command.CountryDialCode}{command.PartialPhoneNumber}"
            );
        }

        await unitOfWork.CommitAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user!.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new PublicUpdateOwnProfileResult(userDto);
    }

    private async Task EnsureEmailUnique(string email, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByEmailAsync(new Email(email), cancellationToken))
        {
            throw UserErrors.EmailAlreadyExists(email);
        }
    }

    private async Task EnsureUsernameUnique(string username, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUserNameAsync(username, cancellationToken))
        {
            throw UserErrors.UsernameAlreadyExists(username);
        }
    }

    private async Task EnsurePhoneUnique(PublicUpdateOwnProfileCommand command, CancellationToken cancellationToken)
    {
        string fullPhone = $"{command.CountryDialCode}{command.PartialPhoneNumber}";
        UserEntity? existing = await userRepository.GetUserByPhoneNumberAsync(fullPhone, cancellationToken);

        if (existing is not null && existing.Id != command.UserId)
        {
            throw UserErrors.PhoneNumberAlreadyExists(fullPhone);
        }
    }
}
