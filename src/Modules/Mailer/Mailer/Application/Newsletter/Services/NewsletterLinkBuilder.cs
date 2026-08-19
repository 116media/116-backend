using _116.Shared.Application.Configurations;

namespace _116.Mailer.Application.Newsletter.Services;

/// <summary>
/// Builds the frontend links embedded in newsletter emails. The API never
/// renders pages itself: confirm and unsubscribe links land on frontend routes
/// that call the corresponding public endpoints.
/// </summary>
public static class NewsletterLinkBuilder
{
    /// <summary>
    /// Fallback base URL for local development when <c>FRONTEND_BASE_URL</c>
    /// is not configured.
    /// </summary>
    private const string DefaultBaseUrl = "http://localhost:3000";

    /// <summary>
    /// Builds the double opt-in confirmation URL for a subscriber token.
    /// </summary>
    /// <param name="token">The subscriber's confirmation token.</param>
    /// <returns>The absolute frontend confirmation URL.</returns>
    public static string ConfirmUrl(string token)
    {
        return $"{BaseUrl()}/newsletter/confirm/{token}";
    }

    /// <summary>
    /// Builds the one-click unsubscribe URL for a subscriber token.
    /// </summary>
    /// <param name="token">The subscriber's unsubscribe token.</param>
    /// <returns>The absolute frontend unsubscribe URL.</returns>
    public static string UnsubscribeUrl(string token)
    {
        return $"{BaseUrl()}/newsletter/unsubscribe/{token}";
    }

    /// <summary>
    /// Resolves the configured frontend base URL, without a trailing slash.
    /// </summary>
    private static string BaseUrl()
    {
        return AppEnvironment.FrontendBaseUrl() ?? DefaultBaseUrl;
    }
}
