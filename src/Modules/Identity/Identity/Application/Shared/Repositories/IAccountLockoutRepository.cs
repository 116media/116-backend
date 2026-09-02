namespace _116.Identity.Application.Shared.Repositories;

/// <summary>
/// The brute-force counters currently recorded against an account.
/// </summary>
/// <param name="FailedLoginAttempts">Consecutive failed logins.</param>
/// <param name="LockedUntil">UTC instant until which login is refused.</param>
/// <param name="OtpFailedAttempts">Consecutive failed OTP attempts.</param>
/// <param name="OtpLockedUntil">UTC instant until which OTP verification is refused.</param>
public readonly record struct AccountLockoutState(
    int FailedLoginAttempts,
    DateTime? LockedUntil,
    int OtpFailedAttempts,
    DateTime? OtpLockedUntil
);

/// <summary>
/// Repository for the per-account brute-force counters. Every write is an atomic SQL update, so a
/// failed attempt is recorded even though the caller then throws.
/// </summary>
public interface IAccountLockoutRepository
{
    /// <summary>
    /// The counters currently recorded for the account, or their defaults when it does not exist.
    /// </summary>
    /// <param name="userId">The account to read.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The current lockout state.</returns>
    Task<AccountLockoutState> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically increments the failed-login counter, locking the account once it reaches the cap.
    /// </summary>
    /// <param name="userId">The account that failed a login.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The new failure count.</returns>
    Task<int> RegisterFailedLoginAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Clears the failed-login counter and any login lock.
    /// </summary>
    /// <param name="userId">The account that logged in successfully.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ClearFailedLoginsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically increments the failed-OTP counter, locking OTP flows once it reaches the cap.
    /// </summary>
    /// <param name="userId">The account that failed an OTP check.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The new failure count.</returns>
    Task<int> RegisterFailedOtpAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Clears the failed-OTP counter and any OTP lock.
    /// </summary>
    /// <param name="userId">The account that passed an OTP check.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task ClearFailedOtpAsync(Guid userId, CancellationToken cancellationToken);
}
