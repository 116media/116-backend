using System.Net;
using System.Net.Sockets;
using _116.Storage.Application.Errors.Facade;
using _116.Storage.Application.Ports;
using Microsoft.Extensions.Hosting;

namespace _116.Storage.Infrastructure.Services;

/// <summary>
/// Rejects URLs that would make the server dial itself or the private network (SSRF). Resolves the
/// host and refuses loopback, link-local, private, unique-local, and multicast addresses, non-default
/// ports, and — outside Development — any non-HTTPS scheme.
/// </summary>
/// <param name="environment">The hosting environment, used to relax the HTTPS rule in Development.</param>
/// <param name="i18n">The Storage i18n facade, used to surface a generic, non-leaking failure.</param>
public sealed class UrlSafetyGuard(IHostEnvironment environment, StorageI18n i18n) : IUrlSafetyGuard
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

        return address.AddressFamily == AddressFamily.InterNetwork && IsReservedIPv4(address.GetAddressBytes());
    }

    /// <summary>
    /// Reports whether an IPv4 address sits in a private, link-local, loopback or reserved range.
    /// </summary>
    /// <param name="octets">The four address octets.</param>
    /// <returns><c>true</c> when the address must not be reached.</returns>
    private static bool IsReservedIPv4(byte[] octets) =>
        octets[0] switch
        {
            10 => true, // 10.0.0.0/8
            172 => octets[1] is >= 16 and <= 31, // 172.16.0.0/12
            192 => octets[1] == 168, // 192.168.0.0/16
            169 => octets[1] == 254, // 169.254.0.0/16 link-local
            >= 224 => true, // 224.0.0.0/4 multicast + reserved
            _ => false,
        };
}
