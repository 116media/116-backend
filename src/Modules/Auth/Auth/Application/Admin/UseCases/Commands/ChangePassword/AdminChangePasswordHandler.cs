using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;
using _116.Auth.Application.Shared.Errors;
using _116.Auth.Application.Shared.Repositories;
using _116.Auth.Application.Shared.Services;
using _116.Auth.Domain.Entities;

namespace _116.Auth.Application.Admin.UseCases.Commands.ChangePassword;

/// <summary>
/// Handles the <see cref="AdminChangePasswordCommand"/> to change admin user password with current password verification.
/// </summary>
/// <param name="userRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing and verification operations.</param>
public class AdminChangePasswordHandler(
    IUserRepository userRepository,
    IPasswordService passwordService
) : ICommandHandler<AdminChangePasswordCommand, AdminChangePasswordResult>
{
    /// <summary>
    /// Handles the password change command by verifying the old password and updating to the new password.
    /// </summary>
    /// <param name="command">The password change command containing user ID, old password, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="AdminChangePasswordResult"/> containing change status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active.</exception>
    /// <exception cref="BadRequestException">Thrown when old password is invalid.</exception>
    /// <exception cref="ConflictException">Thrown when new password is the same as old password.</exception>
    public async Task<AdminChangePasswordResult> Handle(
        AdminChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        // Get user by ID
        UserEntity? user = await userRepository.GetUserByIdAsync(command.UserId, cancellationToken);

        // Validate user account status - admin accounts must be active
        userRepository.IsUserAccountActive(user!);

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

        return new AdminChangePasswordResult(
            IsSuccess: true
        );
    }
}
