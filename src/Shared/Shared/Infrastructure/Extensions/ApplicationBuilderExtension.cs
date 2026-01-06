using _116.Shared.Infrastructure.Seed;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/> to enable database migration and data seeding.
/// </summary>
public static class ApplicationBuilderExtension
{
    /// <summary>
    /// Applies any pending EF Core migrations for the specified DbContext type synchronously during application startup.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to migrate.</typeparam>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// app.UseMigration<CatalogDbContext>();
    /// ]]>
    /// </code>
    /// </example>
    public static IApplicationBuilder UseMigration<TContext>(this IApplicationBuilder app)
        where TContext : DbContext
    {
        MigrateDatabaseAsync<TContext>(app.ApplicationServices).GetAwaiter().GetResult();
        return app;
    }

    /// <summary>
    /// Executes all registered data seeders implementing <see cref="IDataSeeder"/> asynchronously during application startup.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> instance for chaining.</returns>
    /// <example>
    /// <code>
    /// app.UseSeed();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseSeed(this IApplicationBuilder app)
    {
        SeedDataAsync(app.ApplicationServices).GetAwaiter().GetResult();
        return app;
    }

    /// <summary>
    /// Asynchronously migrates the database for the specified <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type to migrate.</typeparam>
    /// <param name="serviceProvider">The service provider to resolve the context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task MigrateDatabaseAsync<TContext>(IServiceProvider serviceProvider)
        where TContext : DbContext
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Asynchronously executes all registered data seeders.
    /// </summary>
    /// <param name="serviceProvider">The service provider to resolve the seeders.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    private static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        IEnumerable<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>();

        // Executes all IDataSeeder in sequence, ensuring safe seeding
        // without concurrent DbContext access issues.
        foreach (IDataSeeder seeder in seeders)
        {
            await seeder.SeedAllAsync();
        }
    }
}
