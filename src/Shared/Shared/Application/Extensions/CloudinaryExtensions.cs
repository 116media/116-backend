using _116.Shared.Application.Configurations;
using _116.Shared.Application.Configurations.Schemas;
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
        var config = new CloudinarySettings
        {
            ApiKey = CloudinaryEnv.ApiKey.Value,
            CloudName = CloudinaryEnv.CloudName.Value,
            ApiSecret = CloudinaryEnv.ApiSecret.Value,
        };

        services.AddSingleton(config);

        return services;
    }
}
