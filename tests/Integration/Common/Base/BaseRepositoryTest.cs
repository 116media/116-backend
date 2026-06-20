namespace _116.Integration.Tests.Common.Base;

/// <summary>
/// Base class for repository-level integration tests that operate directly on a DbContext
/// without the full HTTP pipeline.
/// </summary>
[Collection("Database")]
public abstract class BaseRepositoryTest : IAsyncLifetime
{
    /// <summary>
    /// The shared Testcontainer database fixture.
    /// </summary>
    protected PostgresFixture Postgres { get; }

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}" /> wired to the Testcontainer.
    /// Used to resolve DI services and create DbContexts.
    /// </summary>
    protected ApiFixture Api { get; }

    /// <summary>
    /// Creates a new test instance backed by the shared database container
    /// and its shared API fixture.
    /// </summary>
    protected BaseRepositoryTest(PostgresFixture postgres)
    {
        Postgres = postgres;
        Api = postgres.Api;
    }

    /// <summary>
    /// Creates a new <typeparamref name="TDbContext" /> scoped to the Testcontainer database.
    /// </summary>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        var scope = Api.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

    /// <summary>
    /// Resolves a service from the DI container via a new scope.
    /// </summary>
    protected TService Resolve<TService>()
        where TService : notnull
    {
        var scope = Api.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TService>();
    }

    /// <summary>
    /// Creates a new DI scope and returns a tuple of (repository, dbContext) sharing the same scope,
    /// so that <c>SaveChangesAsync</c> persists changes made by the repository.
    /// </summary>
    protected (TRepository Repo, TDbContext Db) CreateScopedRepository<TRepository, TDbContext>()
        where TRepository : notnull
        where TDbContext : DbContext
    {
        var scope = Api.Services.CreateScope();
        return (
            scope.ServiceProvider.GetRequiredService<TRepository>(),
            scope.ServiceProvider.GetRequiredService<TDbContext>()
        );
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await Postgres.ResetAsync();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
