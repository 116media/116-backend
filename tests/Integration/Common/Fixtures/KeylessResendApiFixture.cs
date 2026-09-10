namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="ResendApiFixture" /> booting without the Resend credential, so the guard that
/// refuses a keyless Resend deployment is the one under test.
/// </summary>
/// <param name="db">The Testcontainer database backing this host.</param>
public class KeylessResendApiFixture(PostgresFixture db) : ResendApiFixture(db)
{
    /// <inheritdoc />
    protected override string? ApiKey => null;
}
