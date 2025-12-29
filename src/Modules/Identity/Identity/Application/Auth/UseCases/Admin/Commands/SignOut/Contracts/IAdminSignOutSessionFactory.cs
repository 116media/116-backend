namespace _116.Identity.Application.Auth.UseCases.Admin.Commands.SignOut.Contracts;

/// <summary>
/// Factory for handling admin user sign-out session management.
/// </summary>
public interface IAdminSignOutSessionFactory
{
    /// <summary>
    /// Signs out an admin user by invalidating their session associated with the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to invalidate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SignOutAsync(
        string refreshToken,
        CancellationToken cancellationToken
    );
}
