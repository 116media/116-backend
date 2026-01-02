using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.ResetPassword.Contracts;

/// <summary>
/// Contains admin user data for password reset operations.
/// </summary>
public record AdminResetPasswordAuthData(
    UserEntity User
);

/// <summary>
/// Factory for handling admin user password reset logic.
/// </summary>
public interface IAdminResetPasswordAuthFactory
{
    /// <summary>
    /// Gets and validates admin user by email for password reset.
    /// </summary>
    /// <param name="email">The admin user's email address.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Admin user data for password reset.</returns>
    Task<AdminResetPasswordAuthData> GetUserForResetAsync(
        string email,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Resets an admin user's password with a new password.
    /// </summary>
    /// <param name="user">The admin user entity to update.</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated user entity.</returns>
    Task<UserEntity> ResetPasswordAsync(
        UserEntity user,
        string newPassword,
        CancellationToken cancellationToken
    );
}
