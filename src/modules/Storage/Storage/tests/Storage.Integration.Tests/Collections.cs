namespace _116.Storage.Integration.Tests;

/// <summary>
/// The test collections this suite joins. xunit resolves a collection definition only inside the assembly
/// that uses it, so every suite declares its own; the fixtures themselves live in the shared boot library.
/// </summary>
[CollectionDefinition("Database")]
public sealed class DatabaseCollection : ICollectionFixture<PostgresFixture>;
