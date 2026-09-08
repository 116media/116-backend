namespace _116.Shared.Application.Configurations.Schemas;

/// <summary>
/// Cloudinary storage credentials.
/// </summary>
public static class CloudinaryEnv
{
    public static readonly EnvVar<string> CloudName = EnvVar.Required("CLOUDINARY_CLOUD_NAME");

    public static readonly EnvVar<string> ApiKey = EnvVar.Required("CLOUDINARY_API_KEY");

    public static readonly EnvVar<string> ApiSecret = EnvVar.Required("CLOUDINARY_API_SECRET");
}
