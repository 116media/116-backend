using System.Net;
using System.Net.Http.Headers;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Stub HTTP transport for the remote-file client. Answers every request with a small, valid JPEG
/// payload so <c>FileService</c>'s download path runs end-to-end — through the SSRF guard and metadata
/// resolution — without any real outbound request. The guard still runs against the request URL, so a
/// blocked address is rejected before this handler is ever reached.
/// </summary>
public sealed class StubRemoteFileHandler : HttpMessageHandler
{
    private static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10];

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var content = new ByteArrayContent(JpegBytes);
        content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Headers.ContentLength = JpegBytes.Length;

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content });
    }
}
