namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "AccountRateLimiting" xUnit test collection, the only host that keeps the real
/// per-account throttle active while the middleware limiter is disabled.
/// </summary>
/// <remarks>
/// Parallelization is disabled: the throttle is a single host-wide instance, and the host is
/// configured through process-wide environment variables that would otherwise race with other
/// collections booting their own hosts.
/// </remarks>
[CollectionDefinition("AccountRateLimiting", DisableParallelization = true)]
public class AccountRateLimitingCollection : ICollectionFixture<AccountRateLimitedPostgresFixture>;
