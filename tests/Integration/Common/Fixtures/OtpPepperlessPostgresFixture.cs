namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the OTP pepper test collection. It leases its own
/// database and boots its own host, so the missing key is never observed by the "Database"
/// collection.
/// </summary>
public class OtpPepperlessPostgresFixture : PostgresFixture
{
    /// <inheritdoc />
    protected override ApiFixture CreateApiFixture() => new OtpPepperlessApiFixture(this);
}
