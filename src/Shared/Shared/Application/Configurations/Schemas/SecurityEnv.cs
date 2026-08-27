namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Security material for account and OTP flows, plus the optional distributed-cache layer.
/// </summary>
public static class SecurityEnv
{
    public static readonly EnvVar<string> OtpPepper = EnvVar.Required("OTP_PEPPER").MinLength(length: 16);

    public static readonly EnvVar<string> DefaultUserPassword = EnvVar
        .Required("DEFAULT_USER_PASSWORD")
        .MinLength(length: 8);

    public static readonly EnvVar<string?> RedisUrl = EnvVar.Optional("REDIS_URL");
}
