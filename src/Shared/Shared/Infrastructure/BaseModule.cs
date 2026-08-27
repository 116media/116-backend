using _116.Shared.Application.Configurations;
using _116.Shared.Application.Configurations.Schemas;
using _116.Shared.Application.Services;
using _116.Shared.Infrastructure.interceptors;
using _116.Shared.Infrastructure.Services;
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
        string connectionString = GetDefaultConnectionString();

        // Register EF Core interceptors if not already registered
        RegisterInterceptorsIfNotExists(services);

        // Register DbContext
        if (options.UseConnectionPooling)
        {
            services.AddDbContextPool<TDbContext>(
                (serviceProvider, dbOptions) =>
                {
                    ConfigureDbContextOptions(serviceProvider, dbOptions, connectionString);
                    ApplyTrackingDefault(dbOptions, options.UseNoTrackingByDefault);
                }
            );
        }
        else
        {
            services.AddDbContext<TDbContext>(
                (serviceProvider, dbOptions) =>
                {
                    ConfigureDbContextOptions(serviceProvider, dbOptions, connectionString);
                    ApplyTrackingDefault(dbOptions, options.UseNoTrackingByDefault);
                }
            );
        }

        return services;
    }

    /// <summary>
    /// Gets the default database connection string from environment configuration.
    /// </summary>
    /// <returns>The formatted connection string</returns>
    private static string GetDefaultConnectionString()
    {
        // All module contexts share this string, so Npgsql serves them from one physical pool;
        // the cap is per connection string, not per context.
        return $"{DatabaseEnv.ConnectionString()}Maximum Pool Size=100;";
    }

    /// <summary>
    /// Registers EF Core interceptors if they haven't been registered already.
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void RegisterInterceptorsIfNotExists(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddSingleton<ICurrentActor, HttpCurrentActor>();

        // The audit interceptor reads the clock through TimeProvider, so the seam must be present
        // wherever a module database is registered, not only in the API host.
        services.TryAddSingleton(TimeProvider.System);

        // The dispatch interceptor logs the post-commit failures it swallows, so the logging
        // services must be present wherever a module database is registered.
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
    /// Configures the DbContext options with interceptors and database provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    /// <param name="options">The DbContext options builder</param>
    /// <param name="connectionString">The database connection string</param>
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

    private static void ConfigureDbContextOptions(
        IServiceProvider serviceProvider,
        DbContextOptionsBuilder options,
        string connectionString
    )
    {
        // Add interceptors
        options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());

        // Configure PostgreSQL with snake_case naming. Transient faults retry with backoff;
        // the command timeout keeps a wedged statement from holding a pooled connection open.
        options
            .UseNpgsql(
                connectionString,
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
