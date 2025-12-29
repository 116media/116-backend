using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Application.Shared.Persistence;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword;

/// <summary>
/// Factory implementation for handling admin user password reset logic.
/// </summary>
/// <param name="authRepository">Repository for user data access operations.</param>
/// <param name="passwordService">Service for password hashing operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminResetPasswordAuthFactory(
    IAuthRepository authRepository,
    IPasswordService passwordService,
    IIdentityUnitOfWork unitOfWork
) : IAdminResetPasswordAuthFactory
{
    /// <summary>
    /// Gets and validates admin user by email for password reset.
    /// </summary>
    public async Task<AdminResetPasswordAuthData> GetUserForResetAsync(
        string email,
        CancellationToken cancellationToken
    )
    {
        var emailValue = new Email(value: email);
        UserEntity? user =
            await authRepository.GetUserWithRolesByEmailOrThrow(email: emailValue, cancellationToken: cancellationToken);

        authRepository.IsUserAdmin(user!);
        authRepository.IsUserAccountActive(user!);

        return new AdminResetPasswordAuthData(User: user!);
    }

    /// <summary>
    /// Resets an admin user's password with a new password.
    /// </summary>
    public async Task<UserEntity> ResetPasswordAsync(
        UserEntity user,
        string newPassword,
        CancellationToken cancellationToken
    )
    {
        if (passwordService.Verify(password: newPassword, hash: user.PasswordHash))
        {
            throw UserErrors.NewPasswordSameAsOld();
        }

        string hashedPassword = passwordService.Hash(password: newPassword);
        user.UpdatePassword(newPasswordHash: hashedPassword);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return user;
    }
}
