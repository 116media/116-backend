namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "RateLimiting" xUnit test collection, which owns the only host where the
/// production rate limit policies are active.
/// </summary>
/// <remarks>
/// Parallelization is disabled for this collection for two reasons. The rate limit policies are
/// registered as single, host-wide limiters, so two concurrent tests would consume each other's
/// permits. And <see cref="ApiFixture" /> configures the application through process-wide
/// environment variables, which would race with the "Database" collection booting its own host
/// against a different container.
/// </remarks>
[CollectionDefinition("RateLimiting", DisableParallelization = true)]
public class RateLimitingCollection : ICollectionFixture<RateLimitedPostgresFixture>;
