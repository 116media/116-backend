using System.Reflection;
using Carter;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for registering Carter modules from specified assemblies.
/// </summary>
public static class CarterExtension
{
    /// <summary>
    /// Registers Carter modules found in the specified assemblies with the dependency injection container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add Carter modules to.</param>
    /// <param name="assemblies">An array of <see cref="Assembly"/> instances to scan for <see cref="ICarterModule"/> implementations.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so that multiple calls can be chained.</returns>
    /// <remarks>
    /// This method scans all provided assemblies for types implementing <see cref="ICarterModule"/>
    /// and registers them with Carter in a single batch.
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddCarterWithAssemblies(typeof(SomeModule).Assembly, typeof(AnotherModule).Assembly);
    /// </code>
    /// </example>
    public static IServiceCollection AddCarterWithAssemblies(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        services.AddCarter(configurator: config =>
        {
            // For all given assemblies, find all types that implement ICarterModule
            Type[] modules = assemblies
                .SelectMany(assembly => assembly.GetTypes().Where(t => t.IsAssignableTo(typeof(ICarterModule))))
                .ToArray();

            config.WithModules(modules);
        });

        return services;
    }
}
