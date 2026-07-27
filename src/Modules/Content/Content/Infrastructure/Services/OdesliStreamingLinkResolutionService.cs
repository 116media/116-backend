using System.Net;
using System.Text.Json;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace _116.Content.Infrastructure.Services;

/// <summary>
/// Resolves streaming links through the Odesli (song.link) API: one platform URL in, deep
/// links for every platform Odesli matched via the catalog's ISRC linkage out. Keyless on the
/// free tier; an optional API key raises the rate limit when configured.
/// </summary>
/// <param name="httpClient">The HTTP client used to call the Odesli API.</param>
/// <param name="configuration">Configuration providing the base URL and optional API key.</param>
public class OdesliStreamingLinkResolutionService(HttpClient httpClient, IConfiguration configuration)
    : IStreamingLinkResolutionService
{
    /// <summary>
    /// Default Odesli API base URL, used when <c>ODESLI_API_URL</c> is not configured.
    /// </summary>
    private const string DefaultApiUrl = "https://api.song.link/v1-alpha.1";

    /// <summary>
    /// Link availability is region-dependent; our readers are the Congolese market.
    /// </summary>
    private const string UserCountry = "CD";

    /// <summary>
    /// Odesli platform keys mapped to the platforms this module models. The provider
    /// vocabulary stays entirely inside this implementation — unknown keys are skipped so
    /// Odesli adding a platform never breaks resolution.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, EnumStreamingPlatform> PlatformKeys = new Dictionary<
        string,
        EnumStreamingPlatform
    >(comparer: StringComparer.OrdinalIgnoreCase)
    {
        ["spotify"] = EnumStreamingPlatform.Spotify,
        ["appleMusic"] = EnumStreamingPlatform.AppleMusic,
        ["youtubeMusic"] = EnumStreamingPlatform.YoutubeMusic,
        ["tidal"] = EnumStreamingPlatform.Tidal,
        ["deezer"] = EnumStreamingPlatform.Deezer,
    };

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default
    )
    {
        string requestUrl = BuildRequestUrl(sourceUrl: sourceUrl);

        HttpResponseMessage response;

        try
        {
            response = await httpClient.GetAsync(requestUri: requestUrl, cancellationToken: cancellationToken);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            throw new StreamingLinkResolutionException(message: $"Odesli is unreachable: {exception.Message}");
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new StreamingLinkResolutionException(message: "Odesli rate limit hit.", isRateLimited: true);
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new StreamingLinkResolutionException(
                message: $"Odesli returned {(int)response.StatusCode} for the source URL."
            );
        }

        string payload = await response.Content.ReadAsStringAsync(cancellationToken: cancellationToken);

        return ParseLinksByPlatform(payload: payload);
    }

    /// <summary>
    /// Builds the Odesli request URL from configuration: base URL (defaulted), the encoded
    /// source URL, the user country, and the API key only when one is configured.
    /// </summary>
    /// <param name="sourceUrl">The platform URL to resolve.</param>
    /// <returns>The fully composed request URL.</returns>
    private string BuildRequestUrl(string sourceUrl)
    {
        string baseUrl = (configuration["ODESLI_API_URL"] ?? DefaultApiUrl).TrimEnd('/');
        string apiKey = configuration["ODESLI_API_KEY"] ?? string.Empty;

        string url = $"{baseUrl}/links?url={WebUtility.UrlEncode(sourceUrl)}&userCountry={UserCountry}";

        return apiKey.Length > 0 ? $"{url}&key={WebUtility.UrlEncode(apiKey)}" : url;
    }

    /// <summary>
    /// Extracts the modelled platform links from an Odesli response body. Unknown platform
    /// keys and non-https URLs are skipped; a body without <c>linksByPlatform</c> is a
    /// provider fault, not an empty result.
    /// </summary>
    /// <param name="payload">The raw Odesli JSON response body.</param>
    /// <returns>The resolved deep links, keyed by platform.</returns>
    private static IReadOnlyDictionary<EnumStreamingPlatform, string> ParseLinksByPlatform(string payload)
    {
        JsonDocument document;

        try
        {
            document = JsonDocument.Parse(json: payload);
        }
        catch (JsonException exception)
        {
            throw new StreamingLinkResolutionException(message: $"Odesli returned malformed JSON: {exception.Message}");
        }

        using (document)
        {
            if (!document.RootElement.TryGetProperty("linksByPlatform", out JsonElement linksByPlatform))
            {
                throw new StreamingLinkResolutionException(message: "Odesli response carries no linksByPlatform.");
            }

            var resolved = new Dictionary<EnumStreamingPlatform, string>();

            foreach (JsonProperty platformEntry in linksByPlatform.EnumerateObject())
            {
                if (!PlatformKeys.TryGetValue(platformEntry.Name, out EnumStreamingPlatform platform))
                {
                    continue;
                }

                if (
                    !platformEntry.Value.TryGetProperty("url", out JsonElement urlElement)
                    || urlElement.GetString() is not string url
                    || !Uri.TryCreate(uriString: url, uriKind: UriKind.Absolute, out Uri? parsed)
                    || parsed.Scheme != Uri.UriSchemeHttps
                )
                {
                    continue;
                }

                resolved[platform] = url;
            }

            return resolved;
        }
    }
}
