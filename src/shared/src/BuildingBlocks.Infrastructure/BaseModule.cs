using System.Data.Common;
using _116.BuildingBlocks.Application.Configurations;
using _116.BuildingBlocks.Application.Configurations.Schemas;
using _116.BuildingBlocks.Application.Persistence;
using _116.BuildingBlocks.Application.Services;
using _116.BuildingBlocks.Infrastructure.interceptors;
using _116.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace _116.BuildingBlocks.Infrastructure;

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
    /// <item>Connection pooling</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddModuleDatabase<TDbContext>(
        this IServiceCollection services,
        ModuleOptions<TDbContext> options
    )
        where TDbContext : DbContext
    {
        RegisterInterceptorsIfNotExists(services);
        RegisterSharedConnectionIfNotExists(services);

        services.AddDbContext<TDbContext>(
            (serviceProvider, dbOptions) =>
            {
                ConfigureDbContextOptions(serviceProvider, dbOptions);
                ApplyTrackingDefault(dbOptions, options.UseNoTrackingByDefault);
            }
        );

        // Also resolvable as DbContext, so a unit of work can enlist every module context that shares the scope's connection.
        services.AddScoped<DbContext>(serviceProvider => serviceProvider.GetRequiredService<TDbContext>());

        return services;
    }

    /// <summary>
    /// Registers the connection every module context in a scope shares, so one transaction can
    /// span them. Opening and closing stays with EF; the scope owns disposal.
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void RegisterSharedConnectionIfNotExists(IServiceCollection services)
    {
        services.TryAddScoped<DbConnection>(_ => new NpgsqlConnection(GetDefaultConnectionString()));
    }

    /// <summary>
    /// Gets the default database connection string from environment configuration.
    /// </summary>
    /// <returns>The formatted connection string</returns>
    private static string GetDefaultConnectionString()
    {
        // Npgsql pools per connection string, so one shared string means one pool for every context.
        return $"{DatabaseEnv.ConnectionString()}Maximum Pool Size=100;";
    }

    /// <summary>
    /// Registers EF Core interceptors if they haven't been registered already.
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void RegisterInterceptorsIfNotExists(IServiceCollection services)
    {
        // The audit interceptor reads the clock through TimeProvider, so the seam belongs here too.
        services.TryAddSingleton(TimeProvider.System);

        // The exception strategies classify driver failures through this seam instead of naming it.
        services.TryAddSingleton<IUniqueConstraintDetector, PostgresUniqueConstraintDetector>();

        // The dispatch interceptor logs the post-commit failures it swallows, so logging must exist.
        services.AddLogging();

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
    /// Applies the module's query-tracking default. No-tracking modules opt their write-path
    /// repository methods back in with AsTracking.
    /// </summary>
    /// <param name="options">The DbContext options builder</param>
    /// <param name="useNoTrackingByDefault">Whether queries default to no-tracking</param>
    private static void ApplyTrackingDefault(DbContextOptionsBuilder options, bool useNoTrackingByDefault)
    {
        if (useNoTrackingByDefault)
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    /// <summary>
    /// Configures the DbContext options with interceptors and the database provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="options">The DbContext options builder</param>
    private static void ConfigureDbContextOptions(IServiceProvider serviceProvider, DbContextOptionsBuilder options)
    {
        options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());

        // One connection per scope, so one transaction covers every context; the timeout frees it.
        options
            .UseNpgsql(
                serviceProvider.GetRequiredService<DbConnection>(),
                npgsql =>
                {
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null
                    );
                    npgsql.CommandTimeout(30);
                }
            )
            .UseSnakeCaseNamingConvention();
    }
}
