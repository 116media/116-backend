using _116.Integration.Tests.Common.Stubs;

namespace _116.Integration.Tests.Common.Base;

/// <summary>
/// Base class for repository-level integration tests that operate directly on a DbContext
/// without the full HTTP pipeline.
/// </summary>
[Collection("Database")]
public abstract class BaseRepositoryTest : IAsyncLifetime
{
    /// <summary>
    /// Every scope opened by this class, disposed at the end of the test
    /// so its Npgsql connection is released.
    /// </summary>
    private readonly List<IServiceScope> _scopes = [];

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
    /// Opens a scope, records it for disposal at the end of the test, and returns it.
    /// </summary>
    /// <returns>The scope, already tracked.</returns>
    private IServiceScope OpenScope()
    {
        IServiceScope scope = Api.Services.CreateScope();
        _scopes.Add(scope);
        return scope;
    }

    /// <summary>
    /// Creates a new <typeparamref name="TDbContext" /> scoped to the Testcontainer database.
    /// </summary>
    /// <typeparam name="TDbContext">The module context to resolve.</typeparam>
    /// <returns>The resolved context.</returns>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext => OpenScope().ServiceProvider.GetRequiredService<TDbContext>();

    /// <summary>
    /// Resolves a service from the DI container via a new scope.
    /// </summary>
    /// <typeparam name="TService">The service to resolve.</typeparam>
    /// <returns>The resolved service.</returns>
    protected TService Resolve<TService>()
        where TService : notnull => OpenScope().ServiceProvider.GetRequiredService<TService>();

    /// <summary>
    /// Creates a new DI scope and returns a tuple of (repository, dbContext) sharing that
    /// scope, so that <c>SaveChangesAsync</c> persists changes made by the repository.
    /// </summary>
    /// <typeparam name="TRepository">The repository to resolve.</typeparam>
    /// <typeparam name="TDbContext">The module context to resolve.</typeparam>
    /// <returns>The repository and the context that back the same scope.</returns>
    protected (TRepository Repo, TDbContext Db) CreateScopedRepository<TRepository, TDbContext>()
        where TRepository : notnull
        where TDbContext : DbContext
    {
        IServiceProvider provider = OpenScope().ServiceProvider;
        return (provider.GetRequiredService<TRepository>(), provider.GetRequiredService<TDbContext>());
    }

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await Postgres.ResetAsync();
        ResetStubs();
    }

    /// <summary>
    /// Resets every <see cref="IResettableStub" /> before each test, since the stubs are
    /// singletons in the shared <see cref="ApiFixture" /> and outlive the database reset.
    /// </summary>
    private void ResetStubs()
    {
        using IServiceScope scope = Api.Services.CreateScope();

        foreach (IResettableStub stub in scope.ServiceProvider.GetServices<IResettableStub>())
        {
            stub.Reset();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (IServiceScope scope in _scopes)
        {
            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
            else
            {
                scope.Dispose();
            }
        }

        _scopes.Clear();
    }
}
