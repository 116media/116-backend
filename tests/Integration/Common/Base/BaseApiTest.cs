using _116.Content.Application.Shared.Cache;
using _116.Identity.Infrastructure.Persistence;
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
    /// Creates a new <typeparamref name="TDbContext" /> scoped to the Testcontainer database.
    /// </summary>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        var scope = Api.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TDbContext>();
    }

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
    /// Saves seeded aggregates as reconstituted state rather than as behavior.
    /// Builders reach the state a test needs by calling real domain methods, which raise the
    /// domain events those methods own. The contexts returned by
    /// <see cref="CreateDbContext{TDbContext}" /> come from the application container, so the
    /// dispatch interceptor is attached and those events would fire their production handlers —
    /// welcome emails, notification rows, promotion stamps — against the arrangement of every
    /// test. Discarding the pending events immediately before the save makes seeding equivalent
    /// to loading rows that already existed, which is what an arrangement means.
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
        InvalidateTagCache();
        InvalidatePopularArticlesCache();
        InvalidatePopularVideosCache();
        await SeedTestUsersAsync();
        await SeedAsync();
    }

    /// <summary>
    /// Clears the in-process tag cache before each test.
    /// The <see cref="IMemoryCache" /> lives in the shared <see cref="ApiFixture" /> singleton and
    /// is not touched by <see cref="PostgresFixture.ResetAsync" />, so cached tag lists would
    /// otherwise leak across tests. Integration tests seed rows directly via
    /// <see cref="SeedAsync{TDbContext}" />, bypassing the mutation handlers that invalidate the
    /// eviction token in production; cancelling the shared token here reproduces that invalidation
    /// so each test reads its own freshly seeded data.
    /// </summary>
    private void InvalidateTagCache()
    {
        using var scope = Api.Services.CreateScope();
        var invalidator = scope.ServiceProvider.GetRequiredService<IPopularTagsCacheInvalidator>();
        invalidator.Invalidate();
    }

    /// <summary>
    /// Clears the in-process popular-articles cache before each test, for the same reason as
    /// <see cref="InvalidateTagCache" />: the shared <see cref="IMemoryCache" /> outlives the
    /// database reset, so ranked article lists cached by one test would otherwise be served
    /// to the next.
    /// </summary>
    private void InvalidatePopularArticlesCache()
    {
        using var scope = Api.Services.CreateScope();
        var invalidator = scope.ServiceProvider.GetRequiredService<IPopularArticlesCacheInvalidator>();
        invalidator.Invalidate();
    }

    /// <summary>
    /// Clears the in-process popular-videos cache before each test, for the same reason as
    /// <see cref="InvalidatePopularArticlesCache" />: the shared <see cref="IMemoryCache" />
    /// outlives the database reset, so ranked video lists cached by one test would otherwise
    /// be served to the next.
    /// </summary>
    private void InvalidatePopularVideosCache()
    {
        using var scope = Api.Services.CreateScope();
        var invalidator = scope.ServiceProvider.GetRequiredService<IPopularVideosCacheInvalidator>();
        invalidator.Invalidate();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Client.Dispose();
        return ValueTask.CompletedTask;
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
