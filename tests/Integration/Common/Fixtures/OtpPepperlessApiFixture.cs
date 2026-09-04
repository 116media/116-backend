namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application with no <c>OTP_PEPPER</c> configured, so
/// the fail-closed branch of the OTP service is the one under test.
/// </summary>
/// <remarks>
/// The variable is cleared before the base fixture reads the environment and restored on dispose,
/// since environment variables are process-global and any host built in the meantime would
/// otherwise be unable to hash an OTP. This host must never be shared with the general suite.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class OtpPepperlessApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <summary>
    /// The environment variable <c>AppEnvironment.OtpPepper</c> reads the hashing key from.
    /// </summary>
    private const string OtpPepperVariable = "OTP_PEPPER";

    /// <summary>
    /// Blanks the variable without removing it: clearing it outright lets <c>Env.NoClobber()</c>
    /// refill it from the developer's <c>.env</c>, and whitespace already reads as missing.
    /// </summary>
    private const string BlankPepper = " ";

    private string? _previousOtpPepper;

    /// <inheritdoc />
    protected override void ConfigureEnvironment()
    {
        base.ConfigureEnvironment();

        _previousOtpPepper = Environment.GetEnvironmentVariable(OtpPepperVariable);
        Environment.SetEnvironmentVariable(OtpPepperVariable, BlankPepper);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(OtpPepperVariable, _previousOtpPepper);

        base.Dispose(disposing);
    }
}
