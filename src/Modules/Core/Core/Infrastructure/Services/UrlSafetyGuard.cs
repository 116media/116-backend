using System.Net;
using System.Net.Sockets;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Application.Shared.Services;
using Microsoft.Extensions.Hosting;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Rejects URLs that would make the server dial itself or the private network (SSRF). Resolves the
/// host and refuses loopback, link-local, private, unique-local, and multicast addresses, non-default
/// ports, and — outside Development — any non-HTTPS scheme.
/// </summary>
/// <param name="environment">The hosting environment, used to relax the HTTPS rule in Development.</param>
/// <param name="i18n">The Core i18n facade, used to surface a generic, non-leaking failure.</param>
public sealed class UrlSafetyGuard(IHostEnvironment environment, CoreI18n i18n) : IUrlSafetyGuard
{
    /// <inheritdoc />
    public async Task EnsureSafeAsync(Uri uri, CancellationToken cancellationToken)
    {
        bool isDevelopment = environment.IsDevelopment();

        if (uri.Scheme != Uri.UriSchemeHttps && !(isDevelopment && uri.Scheme == Uri.UriSchemeHttp))
        {
            throw i18n.File.FileDownloadFailed();
        }

        if (!uri.IsDefaultPort)
        {
            throw i18n.File.FileDownloadFailed();
        }

        IPAddress[] addresses = await Dns.GetHostAddressesAsync(uri.DnsSafeHost, cancellationToken);

        if (addresses.Length == 0 || addresses.Any(IsBlocked))
        {
            throw i18n.File.FileDownloadFailed();
        }
    }

    /// <summary>
    /// Determines whether an address is in a range the server must never dial for a client URL.
    /// </summary>
    private static bool IsBlocked(IPAddress address)
    {
        if (
            IPAddress.IsLoopback(address)
            || address.IsIPv6Multicast
            || address.IsIPv6LinkLocal
            || address.IsIPv6SiteLocal
            || address.IsIPv6UniqueLocal
        )
        {
            return true;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            byte[] b = address.GetAddressBytes();
            return b[0] == 10 // 10.0.0.0/8
                || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) // 172.16.0.0/12
                || (b[0] == 192 && b[1] == 168) // 192.168.0.0/16
                || (b[0] == 169 && b[1] == 254) // 169.254.0.0/16 link-local
                || b[0] == 127 // 127.0.0.0/8 loopback
                || b[0] >= 224; // 224.0.0.0/4 multicast + reserved
        }

        return false;
    }
}
