using _116.Identity.Domain.Entities;

namespace _116.Identity.Application.Auth.UseCases.Public.Commands.ResetPassword.Contracts;

/// <summary>
/// Contains user data for password reset operations.
/// </summary>
public record PublicResetPasswordAuthData(
    UserEntity User
);

/// <summary>
/// Factory for handling user password reset logic.
/// </summary>
public interface IPublicResetPasswordAuthFactory
{
    /// <summary>
    /// Gets and validates user by email for password reset.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>User data for password reset.</returns>
    Task<PublicResetPasswordAuthData> GetUserForResetAsync(
        string email,
        CancellationToken cancellationToken
    );

    /// <summary>
    /// Resets a user's password with a new password.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    /// <param name="newPassword">The new password to set.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The updated user entity.</returns>
    Task<UserEntity> ResetPasswordAsync(
        UserEntity user,
        string newPassword,
        CancellationToken cancellationToken
    );
}
