using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.User.Application.Shared.Errors;
using _116.User.Application.Shared.Mappers;
using _116.User.Application.Shared.Repositories;
using _116.User.Domain.Entities;

namespace _116.User.Application.Admin.UseCases.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="AdminUpdateOwnProfileCommand"/> to update admin user's own profile information.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
public class AdminUpdateOwnProfileHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository
) : ICommandHandler<AdminUpdateOwnProfileCommand, AdminUpdateOwnProfileResult>
{
    /// <summary>
    /// Handles the profile update command by validating uniqueness and updating admin user information.
    /// </summary>
    /// <param name="command">The profile update command containing user ID and new profile data.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminUpdateOwnProfileResult"/> containing updated admin user profile data.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="ConflictException">Thrown when username or phone number already exists.</exception>
    public async Task<AdminUpdateOwnProfileResult> Handle(
        AdminUpdateOwnProfileCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user = await userRepository.GetUserWithRolesAndPermissionsByIdAsync(
            command.UserId,
            cancellationToken
        );

        // Validate user account status - admin accounts must be active
        userRepository.IsUserAccountActive(user!);

        bool isPhoneUpdated = !string.IsNullOrEmpty(command.PartialPhoneNumber);
        bool isUsernameUpdated = !string.IsNullOrEmpty(command.UserName) && user!.UserName != command.UserName;

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

        // Save changes to the DB
        await userRepository.UpdateAsync(user!, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(user!.UserRoles);

        // Fetch the avatar file if the user has one
        FileEntity? avatarFile = await fileRepository.GetAvatarFileAsync(user.AvatarFileId, cancellationToken);

        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles, permissions, avatarDto);

        return new AdminUpdateOwnProfileResult(userDto);
    }

    private async Task EnsureUsernameUnique(string username, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUserNameAsync(username, cancellationToken))
        {
            throw UserErrors.UsernameAlreadyExists(username);
        }
    }

    private async Task EnsurePhoneUnique(AdminUpdateOwnProfileCommand command, CancellationToken cancellationToken)
    {
        string fullPhone = $"{command.CountryDialCode}{command.PartialPhoneNumber}";
        UserEntity? existing = await userRepository.GetUserByPhoneNumberAsync(fullPhone, cancellationToken);

        if (existing is not null && existing.Id != command.UserId)
        {
            throw UserErrors.PhoneNumberAlreadyExists(fullPhone);
        }
    }
}
