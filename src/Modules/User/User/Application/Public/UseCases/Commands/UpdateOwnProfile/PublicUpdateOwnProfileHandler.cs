using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;
using _116.User.Domain.ValueObjects;

namespace _116.User.Application.Public.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="PublicUpdateOwnProfileCommand"/> to update user's own profile information.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class PublicUpdateOwnProfileHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository
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
        // Get user by ID
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdAsync(command.UserId, cancellationToken);

        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);

        bool isEmailUpdated = !string.IsNullOrEmpty(command.Email) && user.Email != command.Email.ToLowerInvariant();
        bool isUsernameUpdated = !string.IsNullOrEmpty(command.UserName) && user.UserName != command.UserName;
        bool isPhoneUpdated = !string.IsNullOrEmpty(command.PartialPhoneNumber);

        // Validate uniqueness for email if being updated
        if (isEmailUpdated)
        {
            bool emailExists = await userRepository.ExistsByEmailAsync(new Email(command.Email!), cancellationToken);
            if (emailExists)
            {
                throw UserErrors.EmailAlreadyExists(command.Email!);
            }
        }

        // Validate uniqueness for username if being updated
        if (isUsernameUpdated)
        {
            bool usernameExists = await userRepository.ExistsByUserNameAsync(command.UserName!, cancellationToken);
            if (usernameExists)
            {
                throw UserErrors.UsernameAlreadyExists(command.UserName!);
            }
        }

        // Check phone number uniqueness if being updated
        if (isPhoneUpdated && !string.IsNullOrEmpty(command.CountryDialCode))
        {
            string fullPhoneNumber = $"{command.CountryDialCode}{command.PartialPhoneNumber}";

            // Check if phone number already exists for another user
            var existingUserWithPhone = await userRepository.GetUserByPhoneNumberAsync(fullPhoneNumber, cancellationToken);
            if (existingUserWithPhone != null && existingUserWithPhone.Id != command.UserId)
            {
                throw UserErrors.PhoneNumberAlreadyExists(fullPhoneNumber);
            }
        }

        // Update email if provided
        if (isEmailUpdated)
        {
            user.UpdateEmail(command.Email!);
            // Note: UpdateEmail method automatically resets IsVerified to false and sets IsLoggedIn to false
        }

        // Update username if provided
        if (isUsernameUpdated)
        {
            user.UpdateUserName(command.UserName!);
        }

        // Update phone and country information if provided
        if (isPhoneUpdated)
        {
            string? fullPhoneNumber = !string.IsNullOrEmpty(command.CountryDialCode)
                ? $"{command.CountryDialCode}{command.PartialPhoneNumber}"
                : null;

            user.UpdatePhoneNumber(
                command.CountryName,
                command.CountryFlagUrl,
                command.CountryIsoCode,
                command.CountryDialCode,
                fullPhoneNumber,
                command.PartialPhoneNumber
            );
        }

        // Save changes to database
        await userRepository.UpdateAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = user.AvatarFileId.HasValue
            ? await fileRepository.GetByIdAsync(user.AvatarFileId.Value, cancellationToken)
            : null;

        // Map to userDTO with avatar
        var avatarDto = avatarFile.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new PublicUpdateOwnProfileResult(userDto);
    }
}
