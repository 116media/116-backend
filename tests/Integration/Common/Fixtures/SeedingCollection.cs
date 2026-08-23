namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "Seeding" xUnit collection, which owns the only host that boots outside the
/// Testing environment. Parallelization is disabled because the host is configured
/// through process-wide environment variables.
/// </summary>
[CollectionDefinition("Seeding", DisableParallelization = true)]
public class SeedingCollection : ICollectionFixture<SeedingPostgresFixture>;
