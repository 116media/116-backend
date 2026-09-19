using System.Net;
using System.Text;
using _116.Core.Contracts.Domain.Enums;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Stands in for Cloudinary's HTTP endpoint. Sitting at the transport rather than replacing
/// <c>ICloudStorageClient</c> means the real adapter, its resilience pipeline and the real
/// <c>CloudinaryService</c> all run under integration, while no request leaves the process.
/// </summary>
public sealed class StubCloudinaryEndpoint : HttpMessageHandler, IResettableStub
{
    /// <summary>
    /// When set, the next delete is answered with this provider error message once, then the
    /// field clears. Uploads are unaffected.
    /// </summary>
    public string? NextDeleteError { get; set; }

    /// <summary>
    /// When set, the next upload is answered with this provider error message once, then the
    /// field clears.
    /// </summary>
    public string? NextUploadError { get; set; }

    /// <summary>
    /// Every public id a delete call carried, in call order.
    /// </summary>
    public List<string> DeletedPublicIds { get; } = [];

    /// <summary>
    /// The kind each delete call addressed the asset under, in call order. This is what proves
    /// a video or raw asset is not deleted as an image.
    /// </summary>
    public List<EnumStoredFileKind> DeletedKinds { get; } = [];

    /// <summary>
    /// Every public id an upload call carried, in call order, so a test can assert that a
    /// request rejected on its way in never reached storage.
    /// </summary>
    public List<string> UploadedPublicIds { get; } = [];

    /// <summary>
    /// The kind each upload call stored the asset under, in call order.
    /// </summary>
    public List<EnumStoredFileKind> UploadedKinds { get; } = [];

    /// <inheritdoc />
    public void Reset()
    {
        NextDeleteError = null;
        NextUploadError = null;
        DeletedPublicIds.Clear();
        DeletedKinds.Clear();
        UploadedPublicIds.Clear();
        UploadedKinds.Clear();
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        string path = request.RequestUri!.AbsolutePath;
        IReadOnlyDictionary<string, string> fields = await ReadFieldsAsync(request, cancellationToken);

        if (path.EndsWith("/destroy", StringComparison.Ordinal))
        {
            return Answer(Destroy(path, fields));
        }

        return path.Contains("/resources/", StringComparison.Ordinal)
            ? Answer(DeleteResources(path, request.RequestUri))
            : Answer(Upload(path, fields));
    }

    /// <summary>
    /// Records a single-asset deletion and answers it.
    /// </summary>
    /// <param name="path">The request path, which carries the resource type.</param>
    /// <param name="fields">The submitted form fields.</param>
    /// <returns>The JSON body.</returns>
    private string Destroy(string path, IReadOnlyDictionary<string, string> fields)
    {
        DeletedPublicIds.Add(fields.GetValueOrDefault("public_id", string.Empty));
        DeletedKinds.Add(KindOf(path));

        string? error = TakeOnce(() => NextDeleteError, value => NextDeleteError = value);

        return error is not null ? Error(error) : """{"result":"ok"}""";
    }

    /// <summary>
    /// Records a batch deletion and answers it. The provider takes the ids as repeated query
    /// parameters rather than in the body.
    /// </summary>
    /// <param name="path">The request path, which carries the resource type.</param>
    /// <param name="uri">The full request URI.</param>
    /// <returns>The JSON body.</returns>
    private string DeleteResources(string path, Uri uri)
    {
        List<string> keys =
        [
            .. uri
                .Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Where(pair => pair.StartsWith("public_ids[]=", StringComparison.Ordinal))
                .Select(pair => Uri.UnescapeDataString(pair["public_ids[]=".Length..])),
        ];

        EnumStoredFileKind kind = KindOf(path);
        DeletedPublicIds.AddRange(keys);
        DeletedKinds.AddRange(keys.Select(_ => kind));

        string? error = TakeOnce(() => NextDeleteError, value => NextDeleteError = value);

        if (error is not null)
        {
            return Error(error);
        }

        string entries = string.Join(",", keys.Select(key => $"\"{key}\":\"deleted\""));

        return "{\"deleted\":{" + entries + "}}";
    }

    /// <summary>
    /// Records an upload and answers it with the asset the provider would have stored.
    /// </summary>
    /// <param name="path">The request path, which carries the resource type.</param>
    /// <param name="fields">The submitted form fields.</param>
    /// <returns>The JSON body.</returns>
    private string Upload(string path, IReadOnlyDictionary<string, string> fields)
    {
        string publicId = fields.GetValueOrDefault("public_id", string.Empty);
        EnumStoredFileKind kind = KindOf(path);
        UploadedPublicIds.Add(publicId);
        UploadedKinds.Add(kind);

        string? error = TakeOnce(() => NextUploadError, value => NextUploadError = value);

        if (error is not null)
        {
            return Error(error);
        }

        string folder = fields.GetValueOrDefault("folder", string.Empty);
        string stored = folder.Length == 0 ? publicId : $"{folder}/{publicId}";

        (string resourceType, string format) = kind switch
        {
            EnumStoredFileKind.Video => ("video", "mp4"),
            EnumStoredFileKind.Raw => ("raw", "pdf"),
            _ => ("image", "jpg"),
        };

        int width = kind == EnumStoredFileKind.Raw ? 0 : 800;
        int height = kind == EnumStoredFileKind.Raw ? 0 : 600;
        string secureUrl = $"https://res.cloudinary.com/test-cloud/{resourceType}/upload/{stored}.{format}";

        return $$"""
            {"public_id":"{{stored}}","secure_url":"{{secureUrl}}","format":"{{format}}","width":{{width}},"height":{{height}},"bytes":1024,"resource_type":"{{resourceType}}"}
            """;
    }

    /// <summary>
    /// The stored-file kind the request path addresses the asset under.
    /// </summary>
    /// <param name="path">The request path.</param>
    /// <returns>The kind.</returns>
    private static EnumStoredFileKind KindOf(string path)
    {
        if (path.Contains("/video/", StringComparison.Ordinal))
        {
            return EnumStoredFileKind.Video;
        }

        return path.Contains("/raw/", StringComparison.Ordinal) ? EnumStoredFileKind.Raw : EnumStoredFileKind.Image;
    }

    /// <summary>
    /// Reads a queued one-shot value and clears it.
    /// </summary>
    /// <param name="read">Reads the current value.</param>
    /// <param name="clear">Writes the cleared value back.</param>
    /// <returns>The value that was queued, or null.</returns>
    private static string? TakeOnce(Func<string?> read, Action<string?> clear)
    {
        string? value = read();

        if (value is not null)
        {
            clear(null);
        }

        return value;
    }

    /// <summary>
    /// A provider error payload. The SDK surfaces it as a result error rather than an exception,
    /// which is the failure shape the adapter is written against.
    /// </summary>
    /// <param name="message">The provider's message.</param>
    /// <returns>The JSON body.</returns>
    private static string Error(string message) => "{\"error\":{\"message\":\"" + message + "\"}}";

    /// <summary>
    /// Wraps a JSON body in the response the SDK expects.
    /// </summary>
    /// <param name="json">The body.</param>
    /// <returns>The response.</returns>
    private static HttpResponseMessage Answer(string json) =>
        new(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    /// <summary>
    /// The text fields of a multipart request, keyed by form name. The file part is skipped, so
    /// its bytes never take part in the lookup.
    /// </summary>
    /// <param name="request">The request to read.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The submitted fields.</returns>
    private static async Task<IReadOnlyDictionary<string, string>> ReadFieldsAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Dictionary<string, string> fields = [];

        string? boundary = request
            .Content?.Headers.ContentType?.Parameters.FirstOrDefault(parameter =>
                parameter.Name.Equals("boundary", StringComparison.OrdinalIgnoreCase)
            )
            ?.Value?.Trim('"');

        if (request.Content is null || boundary is null)
        {
            return fields;
        }

        string body = await request.Content.ReadAsStringAsync(cancellationToken);

        foreach (string part in body.Split($"--{boundary}", StringSplitOptions.RemoveEmptyEntries))
        {
            int headerEnd = part.IndexOf("\r\n\r\n", StringComparison.Ordinal);

            if (headerEnd < 0)
            {
                continue;
            }

            string headers = part[..headerEnd];
            string? name = NameOf(headers);

            if (name is null || headers.Contains("filename", StringComparison.Ordinal))
            {
                continue;
            }

            fields[name] = part[(headerEnd + 4)..].TrimEnd('\r', '\n', '-');
        }

        return fields;
    }

    /// <summary>
    /// The form name a part's headers declare.
    /// </summary>
    /// <param name="headers">The part's headers.</param>
    /// <returns>The name, or null when the part declares none.</returns>
    private static string? NameOf(string headers)
    {
        const string Marker = "name=\"";
        int at = headers.IndexOf(Marker, StringComparison.Ordinal);

        if (at < 0)
        {
            return null;
        }

        int start = at + Marker.Length;
        int end = headers.IndexOf('"', start);

        return end < 0 ? null : headers[start..end];
    }
}
