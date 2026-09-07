using _116.Content.Infrastructure.Persistence;
using _116.Core.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence;
using _116.Mailer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Api;

/// <summary>
/// Applies every module's pending EF Core migrations. Invoked by the <c>migrate</c> command
/// before deploys, and at startup in Development so the local loop keeps working.
/// </summary>
public static class DatabaseMigrator
{
    /// <summary>
    /// Migrates all module databases in dependency order.
    /// </summary>
    /// <param name="serviceProvider">Root provider the migration scope is created from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    public static async Task MigrateAllAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        await scope.ServiceProvider.GetRequiredService<CoreDbContext>().Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<ContentDbContext>().Database.MigrateAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<MailerDbContext>().Database.MigrateAsync(cancellationToken);
    }
}

/// <summary>
/// Applies pending migrations at startup. Registered in Development only — every other
/// environment migrates through the explicit <c>migrate</c> command so instances never race
/// the migration lock and <c>CONCURRENTLY</c> index builds can run out of band.
/// </summary>
/// <param name="serviceProvider">Root provider the migration scope is created from.</param>
public class DevelopmentMigrationHostedService(IServiceProvider serviceProvider) : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await DatabaseMigrator.MigrateAllAsync(serviceProvider, cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
