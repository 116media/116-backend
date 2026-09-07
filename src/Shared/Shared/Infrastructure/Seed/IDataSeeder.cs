namespace _116.Shared.Infrastructure.Seed;

/// <summary>
/// Defines a contract for seeding initial data into the application's data store.
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Executes the seed operations. Implementations are idempotent per row, so a re-run
    /// inserts only what is missing.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task SeedAsync(CancellationToken cancellationToken = default);
}
