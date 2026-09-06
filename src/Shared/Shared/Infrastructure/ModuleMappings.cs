using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace _116.Shared.Infrastructure;

/// <summary>
/// Merges every module's Mapster registrations into the single TypeAdapterConfig behind the
/// shared IMapper, so no module's mappings shadow another's.
/// </summary>
public static class ModuleMappings
{
    /// <summary>
    /// Registers a module's <see cref="IRegister" /> and ensures the merged config, IMapper,
    /// and the boot-time compilation are registered exactly once.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="register">The module's mapping registrations</param>
    /// <returns>The updated service collection for chaining</returns>
    public static IServiceCollection AddModuleMappings(this IServiceCollection services, IRegister register)
    {
        services.AddSingleton(register);

        services.TryAddSingleton(serviceProvider =>
        {
            var config = new TypeAdapterConfig();
            config.Apply(serviceProvider.GetServices<IRegister>());
            config.Compile();
            return config;
        });

        services.TryAddScoped<IMapper>(serviceProvider => new Mapper(
            serviceProvider.GetRequiredService<TypeAdapterConfig>()
        ));

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ModuleMappingsCompiler>());

        return services;
    }
}
