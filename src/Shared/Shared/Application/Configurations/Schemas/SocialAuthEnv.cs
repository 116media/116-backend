namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Social login provider credentials.
/// </summary>
public static class SocialAuthEnv
{
    public static readonly EnvVar<string> GoogleClientId = EnvVar.Required("GOOGLE_CLIENT_ID");

    public static readonly EnvVar<string> FacebookAppId = EnvVar.Required("FACEBOOK_APP_ID");

    public static readonly EnvVar<string> FacebookAppSecret = EnvVar.Required("FACEBOOK_APP_SECRET");
}
