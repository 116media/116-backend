namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// A <see cref="PostgresFixture" /> dedicated to the OTP pepper test collection. It leases its own
/// database and warms a healthy host for schema setup; the pepperless host itself is built inside
/// the test, because boot validation makes its construction throw.
/// </summary>
public class OtpPepperlessPostgresFixture : PostgresFixture;
