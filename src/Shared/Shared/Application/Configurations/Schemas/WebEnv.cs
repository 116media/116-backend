using System.Net;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Browser origin, public URL and proxy trust variables.
/// </summary>
public static class WebEnv
{
    public static readonly EnvVar<string> WebAppOrigin = EnvVar.Required("WEBAPP_ORIGIN").AbsoluteUrlList();

    public static readonly EnvVar<string> DashboardOrigin = EnvVar.Required("DASHBOARD_ORIGIN").AbsoluteUrlList();

    public static readonly EnvVar<string> FrontendBaseUrl = EnvVar.Required("FRONTEND_BASE_URL").AbsoluteUrl();

    public static readonly EnvVar<string?> TrustedProxyNetworks = EnvVar.Optional("TRUSTED_PROXY_NETWORKS");

    /// <summary>
    /// The allowed CORS origins, combined from the dashboard and web-app variables. Each
    /// variable accepts a single origin or a comma-separated list.
    /// </summary>
    /// <returns>The allowed origins; empty when none are configured.</returns>
    public static string[] AllowedOrigins()
    {
        return
        [
            .. new[] { DashboardOrigin.Value, WebAppOrigin.Value }
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .SelectMany(origin =>
                    origin.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ),
        ];
    }

    /// <summary>
    /// The public frontend base URL without a trailing slash, used to build links embedded in
    /// emails and shared content.
    /// </summary>
    /// <returns>The normalized base URL.</returns>
    public static string FrontendBase()
    {
        return FrontendBaseUrl.Value.TrimEnd('/');
    }

    /// <summary>
    /// The proxy networks whose <c>X-Forwarded-*</c> headers are trusted, parsed from the
    /// comma-separated CIDR list. An unset value yields an empty list, which keeps forwarded
    /// headers untrusted — correct for direct-connection local development.
    /// </summary>
    /// <returns>The trusted proxy networks; empty when unset or blank.</returns>
    public static IReadOnlyList<IPNetwork> TrustedProxies()
    {
        string? raw = TrustedProxyNetworks.Value;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return
        [
            .. raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ParseCidr)
                .Where(network => network is not null)
                .Select(network => network!),
        ];
    }

    /// <summary>
    /// Parses a single CIDR block (<c>address/prefix</c>) into an <see cref="IPNetwork" />,
    /// returning null for a malformed entry so one bad value cannot take down startup.
    /// </summary>
    /// <param name="cidr">The CIDR text to parse.</param>
    /// <returns>The parsed network, or null when the entry is malformed.</returns>
    private static IPNetwork? ParseCidr(string cidr)
    {
        string[] parts = cidr.Split('/', 2);
        if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress? address))
        {
            return null;
        }

        return int.TryParse(parts[1], out int prefix) ? new IPNetwork(address, prefix) : null;
    }
}
