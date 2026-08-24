using _116.Shared.Application.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace _116.Shared.Infrastructure.Seed;

/// <summary>
/// Runs every registered <see cref="IDataSeeder" /> once per startup under a Postgres advisory
/// lock, so N starting replicas seed exactly once between them instead of racing the same
/// existence checks.
/// </summary>
/// <param name="serviceProvider">Root provider the seeding scope is created from.</param>
/// <param name="logger">Logger recording each seeder run.</param>
public class DataSeedingHostedService(IServiceProvider serviceProvider, ILogger<DataSeedingHostedService> logger)
    : IHostedService
{
    private const long SeedLockKey = 116_001;

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        List<IDataSeeder> seeders = scope.ServiceProvider.GetServices<IDataSeeder>().ToList();

        if (seeders.Count == 0)
        {
            return;
        }

        var (host, port, db, user, pass) = AppEnvironment.Database();
        await using var connection = new NpgsqlConnection(
            $"Host={host};Port={port};Database={db};Username={user};Password={pass};"
        );
        await connection.OpenAsync(cancellationToken);

        await using (NpgsqlCommand acquire = new($"SELECT pg_advisory_lock({SeedLockKey})", connection))
        {
            await acquire.ExecuteNonQueryAsync(cancellationToken);
        }

        try
        {
            foreach (IDataSeeder seeder in seeders)
            {
                logger.LogInformation("Seeding via {Seeder}.", seeder.GetType().Name);
                await seeder.SeedAsync(cancellationToken);
            }
        }
        finally
        {
            await using NpgsqlCommand release = new($"SELECT pg_advisory_unlock({SeedLockKey})", connection);
            await release.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
