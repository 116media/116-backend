namespace _116.Shared.Contracts.Application.CQRS;

/// <summary>
/// Marks a command whose handling must be throttled per target account — in addition to the
/// per-caller middleware limiter — keyed by a stable account identifier rather than the caller IP, so
/// one account cannot be brute-forced from many IPs. The account-rate-limit decorator applies the
/// throttle before the handler runs; commands without this interface pass straight through.
/// </summary>
public interface IAccountRateLimited
{
    /// <summary>
    /// The rate-limit policy name whose window the account is charged against.
    /// </summary>
    string RateLimitPolicy { get; }

    /// <summary>
    /// The account identifier (typically the email) the throttle buckets by.
    /// </summary>
    string AccountKey { get; }
}
