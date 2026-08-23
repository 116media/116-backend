namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application with a known CORS origin configured,
/// so the populated branch of the default policy in <c>Program.cs</c> is the one under test.
/// </summary>
/// <remarks>
/// The origin is set before the base fixture reads the environment, because
/// <c>AppEnvironment.CorsAllowedOrigins</c> is evaluated once during host construction. The
/// previous value is restored on dispose, since environment variables are process-global and
/// every host built afterwards would otherwise inherit a restricted policy. This host must never
/// be shared with the general suite: a test written against the permissive branch would pass or
/// fail depending on which host it landed on.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class CorsApiFixture(PostgresFixture db) : ApiFixture(db)
{
    /// <summary>
    /// The environment variable <c>AppEnvironment.CorsAllowedOrigins</c> reads the dashboard
    /// origin from.
    /// </summary>
    private const string DashboardOriginVariable = "DASHBOARD_ORIGIN";

    /// <summary>
    /// The origin configured as allowed for the lifetime of this host.
    /// </summary>
    public const string AllowedOrigin = "https://dashboard.116.test";

    /// <summary>
    /// An origin this host never allows, used to assert the negative branch of the policy.
    /// </summary>
    public const string UnconfiguredOrigin = "https://not-allowed.example";

    private string? _previousDashboardOrigin;

    /// <inheritdoc />
    protected override void ConfigureEnvironment()
    {
        base.ConfigureEnvironment();

        _previousDashboardOrigin = Environment.GetEnvironmentVariable(DashboardOriginVariable);
        Environment.SetEnvironmentVariable(DashboardOriginVariable, AllowedOrigin);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(DashboardOriginVariable, _previousDashboardOrigin);

        base.Dispose(disposing);
    }
}
