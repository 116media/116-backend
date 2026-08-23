namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application as Development, so the migration
/// and seeding branches of every module's <c>Use*Module</c> extension run at startup.
/// </summary>
/// <param name="db">The Testcontainer database backing this host.</param>
public class SeedingApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <inheritdoc />
    protected override string EnvironmentName => "Development";
}
