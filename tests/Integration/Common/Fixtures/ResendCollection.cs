namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "Resend" xUnit test collection, which owns the only host booted as a Resend
/// deployment.
/// </summary>
/// <remarks>
/// Parallelization is disabled because <see cref="ResendApiFixture" /> redirects process-global
/// environment variables, and the tests in this collection build further hosts of their own; a host
/// booting concurrently would read the wrong provider.
/// </remarks>
[CollectionDefinition("Resend", DisableParallelization = true)]
public class ResendCollection : ICollectionFixture<ResendPostgresFixture>;
