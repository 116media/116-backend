namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application as a Resend deployment, so the branch
/// of <c>MailerModule.RegisterEmailSender</c> a production Resend host takes is the one under test.
/// </summary>
/// <remarks>
/// The provider is set before the base fixture reads the environment, because the sender adapter is
/// chosen once during module registration. Both variables are restored on dispose, since environment
/// variables are process-global and every host built afterwards would otherwise inherit the Resend
/// provider. This host must never be shared with the general suite.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class ResendApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <summary>
    /// The environment variable naming the email adapter to register.
    /// </summary>
    public const string ProviderVariable = "EMAIL_PROVIDER";

    /// <summary>
    /// The environment variable carrying the Resend credential.
    /// </summary>
    public const string ApiKeyVariable = "RESEND_API_KEY";

    private string? _previousProvider;
    private string? _previousApiKey;

    /// <summary>
    /// The provider value this host boots with. Derived fixtures override it to boot a host whose
    /// registration is expected to fail.
    /// </summary>
    protected virtual string Provider => "resend";

    /// <summary>
    /// The credential this host boots with, or null to boot without one.
    /// </summary>
    protected virtual string? ApiKey => "test-resend-api-key";

    /// <inheritdoc />
    protected override void ConfigureEnvironment()
    {
        base.ConfigureEnvironment();

        _previousProvider = Environment.GetEnvironmentVariable(ProviderVariable);
        _previousApiKey = Environment.GetEnvironmentVariable(ApiKeyVariable);

        Environment.SetEnvironmentVariable(ProviderVariable, Provider);
        Environment.SetEnvironmentVariable(ApiKeyVariable, ApiKey);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(ProviderVariable, _previousProvider);
        Environment.SetEnvironmentVariable(ApiKeyVariable, _previousApiKey);

        base.Dispose(disposing);
    }
}
