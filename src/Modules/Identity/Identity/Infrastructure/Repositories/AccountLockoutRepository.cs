using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace _116.Identity.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IAccountLockoutRepository" /> using set-based
/// <c>ExecuteUpdateAsync</c> statements, so two concurrent failures cannot both read the same count
/// and write the same increment. Login counters live in <see cref="UserLoginStateEntity" /> and
/// OTP counters in <see cref="UserOtpStateEntity" />, both outliving the rows a flow replaces.
/// </summary>
/// <param name="context">The Identity database context.</param>
public class AccountLockoutRepository(IdentityDbContext context) : IAccountLockoutRepository
{
    /// <inheritdoc />
    public async Task<AccountLockoutState> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var login = await context
            .UserLoginStates.Where(s => s.Id == userId)
            .Select(s => new { s.FailedAttempts, s.LockedUntil })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var otp = await context
            .UserOtpStates.Where(s => s.Id == userId)
            .Select(s => new { s.FailedAttempts, s.LockedUntil })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return new AccountLockoutState(
            FailedLoginAttempts: login?.FailedAttempts ?? 0,
            LockedUntil: login?.LockedUntil,
            OtpFailedAttempts: otp?.FailedAttempts ?? 0,
            OtpLockedUntil: otp?.LockedUntil
        );
    }

    /// <inheritdoc />
    public async Task<int> RegisterFailedLoginAsync(Guid userId, CancellationToken cancellationToken)
    {
        await EnsureLoginStateAsync(userId: userId, cancellationToken: cancellationToken);

        DateTime lockUntil = DateTime.UtcNow.AddMinutes(value: UserConstants.LoginLockoutMinutes);

        await context
            .UserLoginStates.Where(s => s.Id == userId)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(s => s.FailedAttempts, s => s.FailedAttempts + 1)
                        .SetProperty(
                            s => s.LockedUntil,
                            s => s.FailedAttempts + 1 >= UserConstants.MaxLoginAttempts ? lockUntil : s.LockedUntil
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
            .UserLoginStates.Where(s => s.Id == userId && (s.FailedAttempts != 0 || s.LockedUntil != null))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.FailedAttempts, 0).SetProperty(s => s.LockedUntil, _ => null),
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
    /// Creates the login lockout row on first use, so accounts that predate the table still lock.
    /// </summary>
    /// <param name="userId">The account the row belongs to.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task EnsureLoginStateAsync(Guid userId, CancellationToken cancellationToken)
    {
        await EnsureStateRowAsync(
            states: context.UserLoginStates,
            state: UserLoginStateEntity.Create(userId: userId),
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
        await EnsureStateRowAsync(
            states: context.UserOtpStates,
            state: UserOtpStateEntity.Create(userId: userId),
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Inserts the state row when it is missing. Two concurrent first failures can both pass the
    /// existence check; the loser's insert hits the primary key, is detached, and the counter
    /// update proceeds against the winner's row.
    /// </summary>
    /// <param name="states">The state table.</param>
    /// <param name="state">The row provisioned on first use.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    private async Task EnsureStateRowAsync<TState>(
        DbSet<TState> states,
        TState state,
        CancellationToken cancellationToken
    )
        where TState : Aggregate<Guid>
    {
        bool exists = await states.AnyAsync(s => s.Id == state.Id, cancellationToken: cancellationToken);
        if (exists)
        {
            return;
        }

        await states.AddAsync(entity: state, cancellationToken: cancellationToken);
        try
        {
            await context.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            context.Entry(entity: state).State = EntityState.Detached;
        }
    }
}
