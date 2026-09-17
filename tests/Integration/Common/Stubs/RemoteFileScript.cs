namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Shared script driving <see cref="StubRemoteFileHandler" />. Lets a test stage the redirect
/// chain a provider would answer with, so the per-hop SSRF re-validation runs for real.
/// </summary>
public sealed class RemoteFileScript : IResettableStub
{
    /// <summary>
    /// Locations answered as redirects, in order, before the image payload is served. An entry may
    /// be absolute or relative; a relative one is resolved against the hop that returned it.
    /// </summary>
    public Queue<string> Redirects { get; } = new();

    /// <summary>
    /// When set, the next response is a redirect carrying no Location header.
    /// </summary>
    public bool RedirectWithoutLocation { get; set; }

    /// <inheritdoc />
    public void Reset()
    {
        Redirects.Clear();
        RedirectWithoutLocation = false;
    }
}
