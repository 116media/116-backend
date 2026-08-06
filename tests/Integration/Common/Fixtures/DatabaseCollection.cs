namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Defines the "Database" xUnit test collection so that all integration tests
/// sharing the <see cref="PostgresFixture" /> run serially against a single database.
/// </summary>
[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<PostgresFixture>;
