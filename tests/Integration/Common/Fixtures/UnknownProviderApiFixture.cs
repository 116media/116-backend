namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="ResendApiFixture" /> booting with a provider no adapter implements, so the
/// unknown-provider guard is the one under test.
/// </summary>
/// <param name="db">The Testcontainer database backing this host.</param>
public class UnknownProviderApiFixture(PostgresFixture db) : ResendApiFixture(db)
{
    /// <inheritdoc />
    protected override string Provider => "carrier-pigeon";
}
