namespace _116.Identity.Application.Auth.UseCases.Public.Commands.SignOut.Contracts;

/// <summary>
/// Factory for handling user sign-out session management.
/// </summary>
public interface IPublicSignOutSessionFactory
{
    /// <summary>
    /// Signs out a user by invalidating their session associated with the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to invalidate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SignOutAsync(string refreshToken, CancellationToken cancellationToken);
}
