using _116.Core.Application.Shared.Errors;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Application.Shared.Errors.Messages;
using _116.Core.Application.Shared.EventHandlers;
using _116.Core.Application.Shared.Persistence;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Constants;
using _116.Core.Domain.Events;
using _116.Core.Infrastructure.Persistence;
using _116.Core.Infrastructure.Repositories;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Services;
using _116.Shared.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace _116.Core;

/// <summary>
/// Provides extension methods to register and configure the Core module's services and middleware.
/// </summary>
public static class CoreModule
{
    /// <summary>
    /// Gets the shared module configuration options for the Core module.
    /// Migrations run in every environment except Testing; the module owns no seeders.
    /// </summary>
    /// <param name="environment">The host environment the options are derived from.</param>
    /// <returns>The module options for the supplied environment.</returns>
    private static ModuleOptions<CoreDbContext> GetModuleOptions(IHostEnvironment environment) =>
        new()
        {
            ModuleName = CoreConstants.ModuleName,
            SchemaName = CoreConstants.SchemaName,
            EnableMigrations = !environment.IsEnvironment("Testing"),
            EnableSeeding = false,
        };

    /// <summary>
    /// Adds the Core module's services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="environment">The host environment deciding whether the module migrates at startup.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddCoreModule(builder.Environment);
    /// </code>
    /// </example>
    public static IServiceCollection AddCoreModule(this IServiceCollection services, IHostEnvironment environment)
    {
        // Register the database with base module infrastructure
        services.AddModuleDatabase(GetModuleOptions(environment));

        // Register error message classes (IStringLocalizer-backed)
        services.AddScoped<ValidationErrorMessage>();
        services.AddScoped<InternalServerErrorMessage>();

        // Register error factory classes
        services.AddScoped<FileErrors>();
        services.AddScoped<CoreI18n>();

        // Register Unit of Work for transaction management
        services.AddScoped<ICoreUnitOfWork, CoreUnitOfWork>();

        // Register core repositories
        services.AddScoped<IFileRepository, FileRepository>();

        // Register core management services
        services.AddHttpClient<IFileService, FileService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddScoped<IImageColorService, ImageColorService>();

        // File lifecycle domain event handlers
        services.AddScoped<IDomainEventHandler<FileReplacedEvent>, FileAssetCleanupHandler>();
        services.AddScoped<IDomainEventHandler<FileSoftDeletedEvent>, FileAssetCleanupHandler>();

        return services;
    }

    /// <summary>
    /// Configures the Core module's middleware in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated <see cref="IApplicationBuilder"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// app.UseCoreModule();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseCoreModule(this IApplicationBuilder app)
    {
        // Configure Http request pipeline.
        IHostEnvironment environment = app.ApplicationServices.GetRequiredService<IHostEnvironment>();
        app.UseModuleDatabase(GetModuleOptions(environment));

        return app;
    }
}
