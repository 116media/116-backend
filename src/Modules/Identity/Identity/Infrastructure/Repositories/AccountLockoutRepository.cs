using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IAccountLockoutRepository" /> using set-based
/// <c>ExecuteUpdateAsync</c> statements, so two concurrent failures cannot both read the same count
/// and write the same increment. Login counters live on the user row; OTP counters live in
/// <see cref="UserOtpStateEntity" />, which outlives the OTP rows a resend replaces.
/// </summary>
/// <param name="context">The Identity database context.</param>
public class AccountLockoutRepository(IdentityDbContext context) : IAccountLockoutRepository
{
    /// <inheritdoc />
    public async Task<AccountLockoutState> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var login = await context
            .Users.Where(u => u.Id == userId)
            .Select(u => new { u.FailedLoginAttempts, u.LockedUntil })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var otp = await context
            .UserOtpStates.Where(s => s.Id == userId)
            .Select(s => new { s.FailedAttempts, s.LockedUntil })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return new AccountLockoutState(
            FailedLoginAttempts: login?.FailedLoginAttempts ?? 0,
            LockedUntil: login?.LockedUntil,
            OtpFailedAttempts: otp?.FailedAttempts ?? 0,
            OtpLockedUntil: otp?.LockedUntil
        );
    }

    /// <inheritdoc />
    public async Task<int> RegisterFailedLoginAsync(Guid userId, CancellationToken cancellationToken)
    {
        DateTime lockUntil = DateTime.UtcNow.AddMinutes(value: UserConstants.LoginLockoutMinutes);

        await context
            .Users.Where(u => u.Id == userId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(u => u.FailedLoginAttempts, u => u.FailedLoginAttempts + 1)
                        .SetProperty(
                            u => u.LockedUntil,
                            u => u.FailedLoginAttempts + 1 >= UserConstants.MaxLoginAttempts ? lockUntil : u.LockedUntil
                        ),
                cancellationToken: cancellationToken
            );

        AccountLockoutState current = await GetAsync(userId: userId, cancellationToken: cancellationToken);
        return current.FailedLoginAttempts;
    }

    /// <inheritdoc />
    public async Task ClearFailedLoginsAsync(Guid userId, CancellationToken cancellationToken)
    {
        await context
            .Users.Where(u => u.Id == userId && (u.FailedLoginAttempts != 0 || u.LockedUntil != null))
            .ExecuteUpdateAsync(
                setters =>
                    setters.SetProperty(u => u.FailedLoginAttempts, 0).SetProperty(u => u.LockedUntil, _ => null),
                cancellationToken: cancellationToken
            );
    }

    /// <inheritdoc />
    public async Task<int> RegisterFailedOtpAsync(Guid userId, CancellationToken cancellationToken)
    {
        await EnsureOtpStateAsync(userId: userId, cancellationToken: cancellationToken);

        DateTime lockUntil = DateTime.UtcNow.AddMinutes(value: UserConstants.OtpLockoutMinutes);

        await context
            .UserOtpStates.Where(s => s.Id == userId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(s => s.FailedAttempts, s => s.FailedAttempts + 1)
                        .SetProperty(
                            s => s.LockedUntil,
                            s => s.FailedAttempts + 1 >= UserConstants.MaxAccountOtpAttempts ? lockUntil : s.LockedUntil
                        ),
                cancellationToken: cancellationToken
            );

        AccountLockoutState current = await GetAsync(userId: userId, cancellationToken: cancellationToken);
        return current.OtpFailedAttempts;
    }

    /// <inheritdoc />
    public async Task ClearFailedOtpAsync(Guid userId, CancellationToken cancellationToken)
    {
        await context
            .UserOtpStates.Where(s => s.Id == userId && (s.FailedAttempts != 0 || s.LockedUntil != null))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.FailedAttempts, 0).SetProperty(s => s.LockedUntil, _ => null),
                cancellationToken: cancellationToken
            );
    }

    /// <summary>
    /// Creates the OTP throttling row on first use, so accounts that predate the table still throttle.
    /// </summary>
    /// <param name="userId">The account the row belongs to.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task EnsureOtpStateAsync(Guid userId, CancellationToken cancellationToken)
    {
        bool exists = await context.UserOtpStates.AnyAsync(s => s.Id == userId, cancellationToken: cancellationToken);
        if (exists)
        {
            return;
        }

        await context.UserOtpStates.AddAsync(
            entity: UserOtpStateEntity.Create(userId: userId),
            cancellationToken: cancellationToken
        );
        await context.SaveChangesAsync(cancellationToken: cancellationToken);
    }
}
