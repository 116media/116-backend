namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Rejects URLs that would make the server dial itself or the private network (SSRF).
/// </summary>
public interface IUrlSafetyGuard
{
    /// <summary>
    /// Validates <paramref name="uri" /> and throws a generic download failure if it is unsafe. Every
    /// hop (initial URL and each redirect target) must pass this before it is fetched.
    /// </summary>
    /// <param name="uri">The URL about to be fetched.</param>
    /// <param name="cancellationToken">Token to cancel the DNS resolution.</param>
    Task EnsureSafeAsync(Uri uri, CancellationToken cancellationToken);
}
