using _116.Shared.Application.Configurations;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Builds public frontend URLs for content records, for links embedded in
/// customer-facing emails.
/// </summary>
public static class ContentPublicLinks
{
    /// <summary>
    /// Fallback base URL for local development when <c>FRONTEND_BASE_URL</c>
    /// is not configured.
    /// </summary>
    private const string DefaultBaseUrl = "http://localhost:3000";

    /// <summary>
    /// Builds the public URL of a published article.
    /// </summary>
    /// <param name="slug">The article slug.</param>
    /// <returns>The absolute frontend URL.</returns>
    public static string Article(string slug) => $"{BaseUrl()}/articles/{slug}";

    /// <summary>
    /// Builds the public URL of a published video.
    /// </summary>
    /// <param name="slug">The video slug.</param>
    /// <returns>The absolute frontend URL.</returns>
    public static string Video(string slug) => $"{BaseUrl()}/videos/{slug}";

    /// <summary>
    /// Builds the public URL of a published lyrics page.
    /// </summary>
    /// <param name="slug">The lyrics slug.</param>
    /// <returns>The absolute frontend URL.</returns>
    public static string Lyrics(string slug) => $"{BaseUrl()}/lyrics/{slug}";

    /// <summary>
    /// Resolves the configured frontend base URL, without a trailing slash.
    /// </summary>
    private static string BaseUrl()
    {
        return AppEnvironment.FrontendBaseUrl() ?? DefaultBaseUrl;
    }
}
