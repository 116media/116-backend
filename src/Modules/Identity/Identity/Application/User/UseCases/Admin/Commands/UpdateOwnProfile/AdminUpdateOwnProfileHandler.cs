using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateOwnProfile;

/// <summary>
/// Handles the <see cref="AdminUpdateOwnProfileCommand" /> to update admin user's own profile information.
/// This endpoint requires admin user authentication - only logged-in admin users can update their own profile.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="roleRepository">Repository for role and permission data operations.</param>
/// <param name="fileRepository">Repository for accessing file metadata.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUpdateOwnProfileHandler(
    IAuthRepository authRepository,
    IRoleRepository roleRepository,
    IFileRepository fileRepository,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<AdminUpdateOwnProfileCommand, AdminUpdateOwnProfileResult>
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
        UserEntity? user = await authRepository.GetUserWithRolesAndPermissionsByIdOrThrow(
            userId: command.UserId,
            cancellationToken: cancellationToken
        );
        // Validate user account status - admin accounts must be active
        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserLoggedIn(user!);

        bool isPhoneUpdated = !string.IsNullOrEmpty(value: command.PartialPhoneNumber);
        bool isUsernameUpdated = !string.IsNullOrEmpty(value: command.UserName) && user!.UserName != command.UserName;
        // Validate uniqueness for username if being updated - check against other users
        if (isUsernameUpdated)
        {
            await EnsureUsernameUnique(command.UserName!, cancellationToken: cancellationToken);
            user!.UpdateUserName(command.UserName!);
        }

        // Check phone number uniqueness if being updated - check against other users
        if (isPhoneUpdated)
        {
            await EnsurePhoneUnique(command: command, cancellationToken: cancellationToken);
            user!.UpdatePhoneNumber(
                countryName: command.CountryName,
                countryIsoCode: command.CountryIsoCode,
                countryDialCode: command.CountryDialCode,
                partialPhoneNumber: command.PartialPhoneNumber,
                fullPhoneNumber: $"{command.CountryDialCode}{command.PartialPhoneNumber}"
            );
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        // Extract roles and permissions using repository
        var (roles, permissions) = roleRepository.GetUserRolesAndPermissions(userRoles: user!.UserRoles);
        // Fetch the avatar file if the user has one
        FileEntity? avatarFile =
            await fileRepository.GetAvatarFileAsync(avatarFileId: user.AvatarFileId,
                cancellationToken: cancellationToken);
        // Map to userDTO with avatar
        var avatarDto = avatarFile?.ToFileDto();
        var userDto = user.ToUserResponseDto(roles: roles, permissions: permissions, avatar: avatarDto);
        return new AdminUpdateOwnProfileResult(User: userDto);
    }

    private async Task EnsureUsernameUnique(string username, CancellationToken cancellationToken)
    {
        if (await authRepository.ExistsByUserNameAsync(userName: username, cancellationToken: cancellationToken))
        {
            throw UserErrors.UsernameAlreadyExists(username: username);
        }
    }

    private async Task EnsurePhoneUnique(AdminUpdateOwnProfileCommand command, CancellationToken cancellationToken)
    {
        string fullPhone = $"{command.CountryDialCode}{command.PartialPhoneNumber}";
        UserEntity? existing =
            await authRepository.GetUserByPhoneNumberAsync(phoneNumber: fullPhone,
                cancellationToken: cancellationToken);
        if (existing is not null && existing.Id != command.UserId)
        {
            throw UserErrors.PhoneNumberAlreadyExists(phoneNumber: fullPhone);
        }
    }
}
