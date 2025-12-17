using _116.Shared.Application.Configurations;

using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for registering Cloudinary configuration.
/// </summary>
public static class CloudinaryExtensions
{
    /// <summary>
    /// Registers Cloudinary configuration from environment variables.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCloudinaryConfiguration(this IServiceCollection services)
    {
        var (cloudName, apiKey, apiSecret) = AppEnvironment.Cloudinary();

        var config = new CloudinarySettings
        {
            ApiKey = apiKey ?? "",
            CloudName = cloudName ?? "",
            ApiSecret = apiSecret ?? ""
        };

        services.AddSingleton(config);

        return services;
    }
}
