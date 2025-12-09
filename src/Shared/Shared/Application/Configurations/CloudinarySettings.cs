namespace _116.Shared.Application.Configurations;

/// <summary>
/// Configuration settings for Cloudinary cloud storage integration.
/// </summary>
public class CloudinarySettings
{
    /// <summary>
    /// The Cloudinary cloud name from account settings.
    /// </summary>
    public string CloudName { get; init; } = string.Empty;

    /// <summary>
    /// The Cloudinary API key for authentication.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// The Cloudinary API secret for signing requests.
    /// </summary>
    public string ApiSecret { get; init; } = string.Empty;

    /// <summary>
    /// Validates that all required configuration values are present.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(CloudName) &&
        !string.IsNullOrWhiteSpace(ApiKey) &&
        !string.IsNullOrWhiteSpace(ApiSecret);
}
