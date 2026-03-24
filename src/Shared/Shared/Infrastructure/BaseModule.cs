using _116.Shared.Application.Configurations;
using _116.Shared.Application.Services;
using _116.Shared.Infrastructure.Extensions;
using _116.Shared.Infrastructure.interceptors;
using _116.Shared.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace _116.Shared.Infrastructure;

/// <summary>
/// Base class for module registration providing common database and infrastructure setup.
/// </summary>
public static class BaseModule
{
    /// <summary>
    /// Registers the database context and common infrastructure services for a module.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type for the module</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="options">Module-specific configuration options</param>
    /// <returns>The updated service collection for chaining</returns>
    /// <remarks>
    /// This method handles the following operations:
    /// <list type="bullet">
    /// <item>Database connection string configuration</item>
    /// <item>EF Core interceptors registration</item>
    /// <item>DbContext registration with PostgreSQL and snake_case naming</item>
    /// <item>Connection pooling (if enabled)</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddModuleDatabase<TDbContext>(
        this IServiceCollection services,
        ModuleOptions<TDbContext> options
    )
        where TDbContext : DbContext
    {
        // Get connection string - use custom or default
        string? connectionString = options.ConnectionString ?? GetDefaultConnectionString();

        // Register EF Core interceptors if not already registered
        RegisterInterceptorsIfNotExists(services);

        // Register DbContext
        if (options.UseConnectionPooling)
        {
            services.AddDbContextPool<TDbContext>(
                (serviceProvider, dbOptions) =>
                {
                    ConfigureDbContextOptions(serviceProvider, dbOptions, connectionString, options);
                }
            );
        }
        else
        {
            services.AddDbContext<TDbContext>(
                (serviceProvider, dbOptions) =>
                {
                    ConfigureDbContextOptions(serviceProvider, dbOptions, connectionString, options);
                }
            );
        }

        return services;
    }

    /// <summary>
    /// Configures the module's middleware pipeline for database operations.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type for the module</typeparam>
    /// <param name="app">The application builder</param>
    /// <param name="options">Module-specific configuration options</param>
    /// <returns>The updated application builder for chaining</returns>
    /// <remarks>
    /// This method handles the following operations:
    /// <list type="bullet">
    /// <item>Database migrations (if enabled)</item>
    /// <item>Data seeding (if enabled)</item>
    /// </list>
    /// </remarks>
    public static IApplicationBuilder UseModuleDatabase<TDbContext>(
        this IApplicationBuilder app,
        ModuleOptions<TDbContext> options
    )
        where TDbContext : DbContext
    {
        if (options.EnableMigrations)
        {
            app.UseMigration<TDbContext>();
        }

        if (options.EnableSeeding)
        {
            app.UseSeed();
        }

        return app;
    }

    /// <summary>
    /// Gets the default database connection string from environment configuration.
    /// </summary>
    /// <returns>The formatted connection string</returns>
    private static string GetDefaultConnectionString()
    {
        var (host, port, db, user, pass) = AppEnvironment.Database();
        return $"Host={host};Port={port};Database={db};Username={user};Password={pass};";
    }

    /// <summary>
    /// Registers EF Core interceptors if they haven't been registered already.
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void RegisterInterceptorsIfNotExists(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<ICurrentActor, HttpCurrentActor>();

        // Check if interceptors are already registered to avoid duplicates
        bool auditInterceptorExists = services.Any(s =>
            s.ServiceType == typeof(ISaveChangesInterceptor)
            && s.ImplementationType == typeof(AuditableEntityInterceptor)
        );

        bool domainEventsInterceptorExists = services.Any(s =>
            s.ServiceType == typeof(ISaveChangesInterceptor)
            && s.ImplementationType == typeof(DispatchDomainEventsInterceptor)
        );

        if (!auditInterceptorExists)
        {
            services.AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        }

        if (!domainEventsInterceptorExists)
        {
            services.AddSingleton<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        }
    }

    /// <summary>
    /// Configures the DbContext options with interceptors and database provider.
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type</typeparam>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="options">The DbContext options builder</param>
    /// <param name="connectionString">The database connection string</param>
    /// <param name="moduleOptions">The module configuration options</param>
    private static void ConfigureDbContextOptions<TDbContext>(
        IServiceProvider serviceProvider,
        DbContextOptionsBuilder options,
        string connectionString,
        ModuleOptions<TDbContext> moduleOptions
    )
        where TDbContext : DbContext
    {
        // Add interceptors
        options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());

        // Configure PostgreSQL with snake_case naming
        options.UseNpgsql(connectionString).UseSnakeCaseNamingConvention();

        // Apply custom configuration if provided
        moduleOptions.ConfigureDbContext?.Invoke(options);
    }
}
