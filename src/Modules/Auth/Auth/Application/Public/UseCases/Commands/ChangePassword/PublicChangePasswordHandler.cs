using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Errors;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;

namespace _116.Auth.Application.Public.UseCases.Commands.ChangePassword;

/// <summary>
/// Handles the <see cref="PublicChangePasswordCommand"/> to change user password with current password verification.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing and verification operations.</param>
public class PublicChangePasswordHandler(
    IUserRepository userRepository,
    IPasswordService passwordService
) : ICommandHandler<PublicChangePasswordCommand, PublicChangePasswordResult>
{
    /// <summary>
    /// Handles the password change command by verifying the old password and updating to the new password.
    /// </summary>
    /// <param name="command">The password change command containing user ID, old password, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicChangePasswordResult"/> containing change status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    /// <exception cref="BadRequestException">Thrown when old password is invalid.</exception>
    /// <exception cref="ConflictException">Thrown when new password is the same as old password.</exception>
    public async Task<PublicChangePasswordResult> Handle(
        PublicChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        // Get user by ID
        UserEntity? user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        // Validate user account status - must be active and verified
        userRepository.IsUserAccountActive(user!);
        userRepository.IsUserAccountVerified(user!);

        // Verify old password
        if (!passwordService.Verify(command.OldPassword, user!.PasswordHash))
        {
            throw UserErrors.InvalidPassword();
        }

        // Check if new password is different from old password
        if (passwordService.Verify(command.NewPassword, user.PasswordHash))
        {
            throw UserErrors.NewPasswordSameAsOld();
        }

        // Hash the new password
        string hashedNewPassword = passwordService.Hash(command.NewPassword);

        // Update user's password
        user.UpdatePassword(hashedNewPassword);
        await userRepository.UpdateAsync(user, cancellationToken);

        // Save changes
        await userRepository.SaveChangesAsync(cancellationToken);

        return new PublicChangePasswordResult(
            IsSuccess: true
        );
    }
}
