namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "Cors" xUnit test collection, which owns the only host booted with an allowed
/// CORS origin configured.
/// </summary>
/// <remarks>
/// Parallelization is disabled because <see cref="CorsApiFixture" /> varies a process-global
/// environment variable that <c>Program.cs</c> reads once during host construction. Running
/// concurrently with a collection that boots its own host would leak the restricted policy into
/// that host.
/// </remarks>
[CollectionDefinition("Cors", DisableParallelization = true)]
public class CorsCollection : ICollectionFixture<CorsPostgresFixture>;
