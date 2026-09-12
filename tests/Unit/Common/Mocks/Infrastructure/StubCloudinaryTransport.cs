using System.Net;
using System.Text;

namespace _116.Unit.Tests.Common.Mocks.Infrastructure;

/// <summary>
/// Stands in for Cloudinary's HTTP endpoint. The SDK sends real requests over the
/// <see cref="HttpClient"/> it is handed, so serving canned JSON here exercises the adapter's
/// own translation and error handling without a network call.
/// </summary>
public sealed class StubCloudinaryTransport : HttpMessageHandler
{
    /// <summary>
    /// Every request URI the SDK sent, in call order. Cloudinary puts the resource type in the
    /// path, so this is what proves an asset was addressed under the right one.
    /// </summary>
    public List<string> Requests { get; } = [];

    /// <summary>
    /// The body returned for the next request, overriding the default success payload.
    /// </summary>
    public string? NextBody { get; set; }

    /// <summary>
    /// When set, the next request throws this instead of answering.
    /// </summary>
    public Exception? NextFailure { get; set; }

    /// <summary>
    /// A successful upload payload carrying every field the adapter projects.
    /// </summary>
    public const string UploadOk =
        """{"public_id":"folder/asset","secure_url":"https://res.cloudinary.test/folder/asset.jpg","format":"jpg","width":800,"height":600,"bytes":1024,"resource_type":"image"}""";

    /// <summary>
    /// A successful single-asset deletion payload.
    /// </summary>
    public const string DestroyOk = """{"result":"ok"}""";

    /// <summary>
    /// A successful batch deletion payload.
    /// </summary>
    public const string DeleteResourcesOk = """{"deleted":{"folder/asset":"deleted"}}""";

    /// <summary>
    /// A payload the provider returns when it refuses the call.
    /// </summary>
    public const string ProviderError = """{"error":{"message":"provider said no"}}""";

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Requests.Add(request.RequestUri!.ToString());

        if (NextFailure is not null)
        {
            Exception failure = NextFailure;
            NextFailure = null;

            throw failure;
        }

        string body = NextBody ?? DefaultFor(request.RequestUri!.AbsolutePath);
        NextBody = null;

        return Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            }
        );
    }

    /// <summary>
    /// The payload a given endpoint answers with when no override is staged.
    /// </summary>
    /// <param name="path">The request path.</param>
    /// <returns>The JSON body.</returns>
    private static string DefaultFor(string path)
    {
        if (path.Contains("/destroy", StringComparison.OrdinalIgnoreCase))
        {
            return DestroyOk;
        }

        return path.Contains("/resources", StringComparison.OrdinalIgnoreCase) ? DeleteResourcesOk : UploadOk;
    }
}
