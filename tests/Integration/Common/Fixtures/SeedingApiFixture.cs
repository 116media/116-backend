namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// An <see cref="ApiFixture" /> that boots the application as Development, so the migration
/// and seeding branches of every module's <c>Use*Module</c> extension run at startup.
/// </summary>
/// <remarks>
/// This host also carries a real <c>TRUSTED_PROXY_NETWORKS</c> value (one valid network plus two
/// malformed entries), so booting it exercises the CIDR-parsing branch of
/// <c>AppEnvironment.TrustedProxyNetworks</c> and both reject branches of its parser — the default
/// host boots with the variable empty to cover the "no trusted proxies" branch. The value is restored
/// on dispose since environment variables are process-global.
/// </remarks>
/// <param name="db">The Testcontainer database backing this host.</param>
public class SeedingApiFixture(PostgresFixture db) : ApiFixture(db)
{
    private const string TrustedProxyNetworksVariable = "TRUSTED_PROXY_NETWORKS";
    private const string TrustedProxyNetworksValue = "10.0.0.0/8, badcidr, 10.0.0.0/xx";

    private string? _previousTrustedProxyNetworks;

    /// <inheritdoc />
    protected override string EnvironmentName => "Development";

    /// <inheritdoc />
    protected override void ConfigureEnvironment()
    {
        base.ConfigureEnvironment();

        _previousTrustedProxyNetworks = Environment.GetEnvironmentVariable(TrustedProxyNetworksVariable);
        Environment.SetEnvironmentVariable(TrustedProxyNetworksVariable, TrustedProxyNetworksValue);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        Environment.SetEnvironmentVariable(TrustedProxyNetworksVariable, _previousTrustedProxyNetworks);

        base.Dispose(disposing);
    }
}
