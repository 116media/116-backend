namespace _116.Identity.Application.Adapters.SocialAuth;

/// <summary>
/// Verified-social-login provider credentials, bound once at startup so a misconfigured deploy fails
/// fast instead of at the first login attempt.
/// </summary>
public sealed class SocialAuthOptions
{
    /// <summary>
    /// The Google OAuth client id, used as the audience the Google ID token must be minted for.
    /// </summary>
    public string GoogleClientId { get; set; } = string.Empty;

    /// <summary>
    /// The Facebook app id, used to build the app access token and to check the token's owner.
    /// </summary>
    public string FacebookAppId { get; set; } = string.Empty;

    /// <summary>
    /// The Facebook app secret, used to build the app access token for token introspection.
    /// </summary>
    public string FacebookAppSecret { get; set; } = string.Empty;
}
