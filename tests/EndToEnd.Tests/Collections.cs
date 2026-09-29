namespace _116.EndToEnd.Tests;

/// <summary>
/// The test collections this suite joins. xunit resolves a collection definition only inside the assembly
/// that uses it, so every suite declares its own; the fixtures themselves live in the shared boot library.
/// </summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<PostgresFixture>;

[CollectionDefinition("UnreachableDatabase", DisableParallelization = true)]
public sealed class UnreachableDatabaseCollection : ICollectionFixture<UnreachableDatabasePostgresFixture>;
