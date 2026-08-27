namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// JWT signing and lifetime variables.
/// </summary>
public static class JwtEnv
{
    public static readonly EnvVar<string> Secret = EnvVar.Required("JWT_SECRET").MinLength(length: 32);

    public static readonly EnvVar<string> Issuer = EnvVar.Required("JWT_ISSUER");

    public static readonly EnvVar<string> Audience = EnvVar.Required("JWT_AUDIENCE");

    public static readonly EnvVar<int> AccessTokenExpirationMinutes = EnvVar
        .Int("JWT_ACCESS_TOKEN_EXPIRATION", @default: 60)
        .InRange(min: 1, max: 24 * 60);

    public static readonly EnvVar<int> RefreshTokenExpirationMinutes = EnvVar
        .Int("JWT_REFRESH_TOKEN_EXPIRATION", @default: 43_200)
        .InRange(min: 1, max: 365 * 24 * 60);

    public static readonly EnvVar<int> SessionAbsoluteLifetimeDays = EnvVar
        .Int("JWT_SESSION_ABSOLUTE_LIFETIME_IN_DAYS", @default: 30)
        .InRange(min: 1, max: 365);
}
