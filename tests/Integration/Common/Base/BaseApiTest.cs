using _116.Content.Application.Shared.Cache;
using _116.Identity.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Stubs;
using _116.Shared.Domain;
using _116.Tests.Fixtures.Factories.Identity;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Caching.Memory;

namespace _116.Integration.Tests.Common.Base;

/// <summary>
/// Base class for integration tests that need an HTTP client and the full API pipeline.
/// Resets the database, seeds well-known test users, and calls <see cref="SeedAsync" />
/// before each test.
/// </summary>
[Collection("Database")]
public abstract class BaseApiTest : IAsyncLifetime
{
    /// <summary>
    /// Every scope opened by <see cref="CreateDbContext{TDbContext}" />, disposed at the
    /// end of the test so its Npgsql connection is released.
    /// </summary>
    private readonly List<IServiceScope> _scopes = [];

    /// <summary>
    /// The shared Testcontainer database fixture.
    /// </summary>
    protected PostgresFixture Db { get; }

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}" /> wired to the Testcontainer.
    /// Use <c>Api.Services</c> to resolve DI services.
    /// </summary>
    protected ApiFixture Api { get; }

    /// <summary>
    /// An <see cref="HttpClient" /> that targets the test server.
    /// Use the <c>AuthenticateAs*</c> extension methods to add JWT headers.
    /// </summary>
    protected HttpClient Client { get; }

    /// <summary>
    /// Creates a new test instance backed by the shared database container
    /// and its shared API fixture.
    /// </summary>
    protected BaseApiTest(PostgresFixture db)
    {
        Db = db;
        Api = db.Api;
        Client = Api.CreateClient();
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
    /// Resolves an application message class from the host container and invokes
    /// <paramref name="select" /> under <paramref name="culture" />, producing the exact string
    /// the application would have put in a ProblemDetails <c>Detail</c>. Pair it with
    /// <c>ShouldBeProblem&lt;TException&gt;</c> instead of hardcoding the sentence.
    /// </summary>
    /// <typeparam name="TMessage">The message class to resolve (e.g. <c>ArticleErrorMessage</c>).</typeparam>
    /// <param name="select">Selects the message method to invoke, with its arguments.</param>
    /// <param name="culture">
    /// The culture to resolve in. Defaults to the culture the host answers a request that sends
    /// no <c>Accept-Language</c> header, which is what <see cref="Client" /> does.
    /// </param>
    /// <returns>The localized message text.</returns>
    protected string Localized<TMessage>(
        Func<TMessage, string> select,
        string culture = LocalizedMessage.DefaultCulture
    )
        where TMessage : notnull => LocalizedMessage.Resolve(Api.Services, select, culture);

    /// <summary>
    /// Creates a new <typeparamref name="TDbContext" /> scoped to the Testcontainer database.
    /// </summary>
    /// <typeparam name="TDbContext">The module context to resolve.</typeparam>
    /// <returns>The resolved context.</returns>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext => OpenScope().ServiceProvider.GetRequiredService<TDbContext>();

    /// <summary>
    /// Seeds data within a scoped <typeparamref name="TDbContext" /> and saves,
    /// removing the create-context / add / save boilerplate repeated across tests.
    /// </summary>
    /// <typeparam name="TDbContext">The database context to seed against.</typeparam>
    /// <param name="seed">An action that adds entities to the context.</param>
    protected async Task SeedAsync<TDbContext>(Action<TDbContext> seed)
        where TDbContext : DbContext
    {
        await using var context = CreateDbContext<TDbContext>();
        seed(context);
        await SaveSeededAsync(context);
    }

    /// <summary>
    /// Seeds an entity within a scoped <typeparamref name="TDbContext" />, saves,
    /// and returns the seeded entity for use in the test.
    /// </summary>
    /// <typeparam name="TDbContext">The database context to seed against.</typeparam>
    /// <typeparam name="TEntity">The seeded entity type.</typeparam>
    /// <param name="seed">A function that adds and returns an entity.</param>
    /// <returns>The seeded entity.</returns>
    protected async Task<TEntity> SeedAsync<TDbContext, TEntity>(Func<TDbContext, TEntity> seed)
        where TDbContext : DbContext
    {
        await using var context = CreateDbContext<TDbContext>();
        TEntity entity = seed(context);
        await SaveSeededAsync(context);
        return entity;
    }

    /// <summary>
    /// Saves seeded aggregates as reconstituted state rather than as behavior, discarding the
    /// pending domain events so the dispatch interceptor never fires production handlers
    /// against a test's arrangement.
    /// </summary>
    /// <param name="context">The context holding the seeded, not-yet-saved aggregates.</param>
    /// <returns>A task that completes once the seeded rows are persisted.</returns>
    private static async Task SaveSeededAsync(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        foreach (EntityEntry<IAggregate> entry in context.ChangeTracker.Entries<IAggregate>())
        {
            entry.Entity.ClearDomainEvents();
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Override to seed test data after the database has been reset.
    /// Called once per test method, after the well-known test users have been seeded.
    /// </summary>
    protected virtual Task SeedAsync() => Task.CompletedTask;

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        await Db.ResetAsync();
        ResetStubs();
        InvalidateTagCache();
        InvalidatePopularArticlesCache();
        InvalidatePopularVideosCache();
        await SeedTestUsersAsync();
        await SeedAsync();
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

    /// <summary>
    /// Clears the in-process tag cache before each test, since the shared
    /// <see cref="IMemoryCache" /> is not touched by
    /// <see cref="PostgresFixture.ResetAsync" />.
    /// </summary>
    private void InvalidateTagCache()
    {
        using var scope = Api.Services.CreateScope();
        var invalidator = scope.ServiceProvider.GetRequiredService<IPopularTagsCacheInvalidator>();
        invalidator.Invalidate();
    }

    /// <summary>
    /// Clears the in-process popular-articles cache before each test, for the same reason as
    /// <see cref="InvalidateTagCache" />.
    /// </summary>
    private void InvalidatePopularArticlesCache()
    {
        using var scope = Api.Services.CreateScope();
        var invalidator = scope.ServiceProvider.GetRequiredService<IPopularArticlesCacheInvalidator>();
        invalidator.Invalidate();
    }

    /// <summary>
    /// Clears the in-process popular-videos cache before each test, for the same reason as
    /// <see cref="InvalidateTagCache" />.
    /// </summary>
    private void InvalidatePopularVideosCache()
    {
        using var scope = Api.Services.CreateScope();
        var invalidator = scope.ServiceProvider.GetRequiredService<IPopularVideosCacheInvalidator>();
        invalidator.Invalidate();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        Client.Dispose();

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

    /// <summary>
    /// Seeds SuperAdmin, Admin, and Visitor users with well-known IDs so that
    /// the <c>AccountStatusRequirementHandler</c> can find them during authorization.
    /// </summary>
    private async Task SeedTestUsersAsync()
    {
        await using var context = CreateDbContext<IdentityDbContext>();

        var superAdmin = UserFactory.CreateWithId(User.SuperAdminId, User.SuperAdminEmail);
        superAdmin.MarkAsVerified();
        superAdmin.Activate();

        var admin = UserFactory.CreateWithId(User.AdminId, User.AdminEmail);
        admin.MarkAsVerified();
        admin.Activate();

        var visitor = UserFactory.CreateWithId(User.VisitorId, User.VisitorEmail);
        visitor.MarkAsVerified();
        visitor.Activate();

        context.Users.AddRange(superAdmin, admin, visitor);
        await SaveSeededAsync(context);
    }
}
