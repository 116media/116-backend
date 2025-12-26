using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ChangePassword;

/// <summary>
/// Handles the <see cref="PublicChangePasswordCommand" /> to change user password with current password verification.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing and verification operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicChangePasswordHandler(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IIdentityUnitOfWork unitOfWork
) : ICommandHandler<PublicChangePasswordCommand, PublicChangePasswordResult>
{
    /// <summary>
    /// Handles the password change command by verifying the old password and updating to the new password.
    /// </summary>
    /// <param name="command">The password change command containing user ID, old password, and new password.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="PublicChangePasswordResult" /> containing change status.</returns>
    /// <exception cref="NotFoundException">Thrown when no user is found with the specified ID.</exception>
    /// <exception cref="BadRequestException">Thrown when the account is not active or verified.</exception>
    /// <exception cref="BadRequestException">Thrown when password is not configured (OAuth users).</exception>
    /// <exception cref="BadRequestException">Thrown when the current password is incorrect.</exception>
    /// <exception cref="ConflictException">Thrown when new password is the same as old password.</exception>
    public async Task<PublicChangePasswordResult> Handle(
        PublicChangePasswordCommand command,
        CancellationToken cancellationToken
    )
    {
        UserEntity? user =
            await authRepository.FindUserByIdOrThrow(userId: command.UserId, cancellationToken: cancellationToken);
        // Validate user account status - must be active and verified
        authRepository.IsUserAccountActive(user!);
        authRepository.IsUserAccountVerified(user!);
        authRepository.IsUserLoggedIn(user!);

        // Check if password is configured (OAuth users don't have passwords)
        if (string.IsNullOrEmpty(value: user!.PasswordHash))
        {
            throw UserErrors.PasswordNotConfigured(provider: user.AuthProvider);
        }

        // Verify old password
        if (!passwordService.Verify(password: command.OldPassword, hash: user.PasswordHash))
        {
            throw UserErrors.IncorrectCurrentPassword();
        }

        // Check if new password is different from old password
        if (passwordService.Verify(password: command.NewPassword, hash: user.PasswordHash))
        {
            throw UserErrors.NewPasswordSameAsOld();
        }

        // Hash the new password
        string hashedNewPassword = passwordService.Hash(password: command.NewPassword);
        // Update user's password
        user.UpdatePassword(newPasswordHash: hashedNewPassword);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new PublicChangePasswordResult(
            true
        );
    }
}
