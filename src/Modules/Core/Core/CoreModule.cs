using _116.Core.Application.Shared.Services;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Infrastructure.Services;
using _116.Shared.Infrastructure;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Core;

/// <summary>
/// Provides extension methods to register and configure the Core module's services and middleware.
/// </summary>
public static class CoreModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Core module.
    /// </summary>
    private static ModuleOptions<CoreDbContext> GetModuleOptions() => new()
    {
        ModuleName = "Core",
        SchemaName = "core",
        EnableMigrations = true,
        EnableSeeding = false
    };

    /// <summary>
    /// Adds the Core module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <remarks>
    /// Registers database context with interceptors for core management including file handling.
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddCoreModule(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddCoreModule(this IServiceCollection services)
    {
        // Register the database with base module infrastructure
        services.AddModuleDatabase(GetModuleOptions());

        // Register core repositories
        services.AddScoped<IFileRepository, FileRepository>();

        // Register core management services
        services.AddHttpClient<IFileService, FileService>();

        return services;
    }

    /// <summary>
    /// Configures the Core module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <remarks>
    /// Applies pending EF Core migrations for core management.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.UseCoreModule();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseCoreModule(this IApplicationBuilder app)
    {
        // Configure Http request pipeline.
        app.UseModuleDatabase(GetModuleOptions());

        return app;
    }
}
