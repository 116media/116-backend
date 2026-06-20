using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;

namespace _116.Integration.Tests.Common.Seeders;

/// <summary>
/// Provides reusable seed-data helpers for integration tests that need
/// production-seeder data (roles, permissions, content types) in the database.
/// </summary>
public class TestDataSeeder
{
    private readonly IServiceProvider _services;

    /// <summary>
    /// Creates a new <see cref="TestDataSeeder" /> backed by the application's DI container.
    /// </summary>
    public TestDataSeeder(IServiceProvider services) => _services = services;

    /// <summary>
    /// Seeds SuperAdmin user, Visitor role, and their permissions.
    /// Required for any test that authenticates via the real login endpoint.
    /// </summary>
    public async Task SeedAuthenticationDataAsync()
    {
        using IServiceScope scope = _services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        await sp.GetRequiredService<SuperAdminSeeder>().SeedAllAsync();
        await sp.GetRequiredService<VisitorRoleSeeder>().SeedAllAsync();
    }

    /// <summary>
    /// Seeds content types (Article, Video, ShortVideo, Lyrics).
    /// Required for any test that creates content entities.
    /// </summary>
    public async Task SeedContentTypesAsync()
    {
        using IServiceScope scope = _services.CreateScope();
        IServiceProvider sp = scope.ServiceProvider;

        await sp.GetRequiredService<ContentTypeSeeder>().SeedAllAsync();
    }

    /// <summary>
    /// Seeds all prerequisite data — auth + content types.
    /// Convenience method for tests that need the full foundation.
    /// </summary>
    public async Task SeedAllAsync()
    {
        await SeedAuthenticationDataAsync();
        await SeedContentTypesAsync();
    }
}
