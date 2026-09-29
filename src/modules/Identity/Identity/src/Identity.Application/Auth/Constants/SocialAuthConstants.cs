namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Fixed provider endpoints for social-token verification. Pinned here so the Graph API version and
/// paths are declared once, not scattered as magic strings across the adapters and DI wiring.
/// </summary>
public static class SocialAuthConstants
{
    /// <summary>
    /// Base address of the Facebook Graph API used for token introspection.
    /// </summary>
    public const string FacebookGraphBaseUrl = "https://graph.facebook.com/v19.0/";

    /// <summary>
    /// Relative Graph endpoint that validates an access token against the app.
    /// </summary>
    public const string FacebookDebugTokenEndpoint = "debug_token";

    /// <summary>
    /// Relative Graph endpoint that returns the authenticated user's profile.
    /// </summary>
    public const string FacebookProfileEndpoint = "me";

    /// <summary>
    /// Profile fields requested from the Graph profile endpoint.
    /// </summary>
    public const string FacebookProfileFields = "id,name,email,picture";
}
