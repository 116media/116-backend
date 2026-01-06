using System.Reflection;
using _116.Shared.Application.Exceptions.Handlers;
using _116.Shared.Application.Exceptions.Handlers.Contracts;
using _116.Shared.Application.Exceptions.Handlers.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for configuring the enterprise-grade exception handling system.
/// </summary>
public static class ExceptionHandlerExtension
{
    /// <summary>
    /// Registers the complete exception handling system with automatic strategy discovery.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// This method automatically discovers and registers all exception strategies in the assembly.
    /// New exception types only require creating a strategy class - no registration code needed.
    /// </remarks>
    public static IServiceCollection AddAppExceptionHandler(this IServiceCollection services)
    {
        // Register the main exception handler
        services.AddExceptionHandler<ExceptionHandler>();
        services.AddProblemDetails();

        // Register the default strategy explicitly
        services.AddSingleton<DefaultExceptionHandler>();

        // Auto-register all exception strategies
        RegisterExceptionStrategies(services);

        // Register the strategy registry
        services.AddSingleton<ExceptionStrategyRegistry>();

        return services;
    }

    /// <summary>
    /// Automatically discovers and registers all exception strategies in the current assembly.
    /// Uses reflection to find all classes implementing IExceptionStrategy for zero-config registration.
    /// </summary>
    /// <param name="services">The service collection to register strategies with.</param>
    private static void RegisterExceptionStrategies(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        List<Type> strategyTypes = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } && typeof(IExceptionStrategy).IsAssignableFrom(type)
            )
            .ToList();

        foreach (Type strategyType in strategyTypes)
        {
            services.AddSingleton(typeof(IExceptionStrategy), strategyType);
        }
    }

    /// <summary>
    /// Configures the application to use the enterprise exception handling middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseAppExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
