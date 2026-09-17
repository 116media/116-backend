using System.Net;
using System.Net.Http.Headers;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Stub HTTP transport for the remote-file client. Answers every request with a small, valid JPEG
/// payload so <c>FileService</c>'s download path runs end-to-end — through the SSRF guard and metadata
/// resolution — without any real outbound request. The guard still runs against the request URL, so a
/// blocked address is rejected before this handler is ever reached. A test can stage redirects on the
/// shared <see cref="RemoteFileScript" /> to drive the manual redirect chain.
/// </summary>
public sealed class StubRemoteFileHandler(RemoteFileScript script) : HttpMessageHandler
{
    private static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (script.RedirectWithoutLocation)
        {
            script.RedirectWithoutLocation = false;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Redirect));
        }

        if (script.Redirects.Count > 0)
        {
            var redirect = new HttpResponseMessage(HttpStatusCode.Redirect);
            redirect.Headers.Location = new Uri(script.Redirects.Dequeue(), UriKind.RelativeOrAbsolute);

            return Task.FromResult(redirect);
        }

        var content = new ByteArrayContent(JpegBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Headers.ContentLength = JpegBytes.Length;

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }
}
