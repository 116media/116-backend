namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "UnreachableDatabase" xUnit test collection, which owns the only host whose
/// readiness probe points at a closed database port.
/// </summary>
/// <remarks>
/// Parallelization is disabled because <see cref="UnreachableDatabaseApiFixture" /> redirects a
/// process-global environment variable; a host booting concurrently would read the wrong port.
/// </remarks>
[CollectionDefinition("UnreachableDatabase", DisableParallelization = true)]
public class UnreachableDatabaseCollection : ICollectionFixture<UnreachableDatabasePostgresFixture>;
