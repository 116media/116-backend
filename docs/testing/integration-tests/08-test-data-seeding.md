# Test Data Seeding

## Strategy

Integration tests need real data in the database. There are three seeding strategies, each appropriate for different scenarios:

| Strategy | When to Use | Scope |
|----------|------------|-------|
| **Inline seeding** | Test-specific data, small datasets | Single test method |
| **SeedAsync override** | Shared data for all tests in a class | Test class |
| **TestDataSeeder** | Reusable setup across multiple test classes | Cross-class |

## Inline Seeding

Insert data directly in the test method using a DbContext. Best for tests that need specific, non-shared data.

```csharp
[Fact]
public async Task Get_WithThreeCategories_ShouldReturnAll()
{
    // Arrange
    await using var context = CreateDbContext<ContentDbContext>();
    Guid typeId = await SeedContentTypeAsync(context);

    for (int i = 0; i < 3; i++)
    {
        var category = CategoryEntity.Create(
            Guid.NewGuid(), typeId, $"Cat {i}", $"cat-{i}",
            "Description", false,
            TestErrorsFactory.CreateCategoryErrors(), false, false);
        context.Categories.Add(category);
    }
    await context.SaveChangesAsync();

    // Act & Assert ...
}
```

## SeedAsync Override

The `BaseRepositoryTest` and `BaseApiTest` provide a virtual `SeedAsync()` method that runs after `ResetAsync()` — before every test method in the class. Use this for data that all tests in the class need.

```csharp
public class VideoEndpointTests : BaseApiTest
{
    private Guid _categoryId;
    private Guid _videoId;

    public VideoEndpointTests(PostgresFixture db) : base(db) { }

    protected override async Task SeedAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();

        var videoType = ContentTypeEntity.Create(
            Guid.NewGuid(), "Video", "Videos");
        context.ContentTypes.Add(videoType);

        var category = CategoryEntity.Create(
            Guid.NewGuid(), videoType.Id, "Shows", "shows",
            "Video shows", false,
            TestErrorsFactory.CreateCategoryErrors(), false, false);
        context.Categories.Add(category);
        _categoryId = category.Id;

        var video = VideoEntity.Create(/* ... */);
        context.Videos.Add(video);
        _videoId = video.Id;

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_ById_ShouldReturn200()
    {
        var response = await Client.GetAsync(
            $"{ApiRoutes.Public.Videos}/{_videoId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## TestDataSeeder

A utility class that wraps the production seeders for integration test setup. Use it for auth-related data that many test classes need.

```csharp
using _116.Content.Infrastructure.Persistence.Seeds.ContentTypes;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.Infrastructure.Persistence.Seeds.Visitor;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Integration.Tests.Common.Seeders;

/// <summary>
/// Orchestrates production seeders to create the minimum data
/// required for integration tests.
/// </summary>
public class TestDataSeeder
{
    private readonly IServiceProvider _services;

    public TestDataSeeder(IServiceProvider services) => _services = services;

    /// <summary>
    /// Seeds SuperAdmin user, Visitor role, and their permissions.
    /// Required for any test that authenticates via HTTP.
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
```

## Data Dependencies

Entities have FK dependencies. Seed them in order:

```
1. ContentTypes (no FK dependencies)
2. PricingTiers (no FK dependencies)
3. PromotionLevels (no FK dependencies)
4. Tags (no FK dependencies)
5. Roles, Permissions (Identity, no FK dependencies)
6. Users (FK → none, but needs Role for auth)
7. UserRoles (FK → Users, Roles)
8. Categories (FK → ContentTypes)
9. CategoryPricings (FK → Categories, PricingTiers)
10. Customers (FK → Users)
11. Articles (FK → Categories, Customers)
12. Videos (FK → Categories, Customers)
13. Packages (FK → none)
14. Orders (FK → Customers)
15. OrderItems (FK → Orders, Articles/Videos)
16. Interactions (FK → Articles/Videos, Users)
```

## Respawn and Seed Data

Respawn truncates ALL tables by default. If you want content types or roles to persist across tests (to avoid re-seeding), configure Respawn to ignore those tables:

```csharp
_respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
{
    DbAdapter = DbAdapter.Postgres,
    SchemasToInclude =
    [
        IdentityConstants.SchemaName,
        CoreConstants.SchemaName,
        ContentConstants.SchemaName,
    ],
    TablesToIgnore = [
        new Respawn.Graph.Table(ContentConstants.SchemaName, "content_types"),
        new Respawn.Graph.Table(IdentityConstants.SchemaName, "roles"),
        new Respawn.Graph.Table(IdentityConstants.SchemaName, "permissions"),
        new Respawn.Graph.Table(IdentityConstants.SchemaName, "role_permissions"),
    ],
});
```

**Tradeoff**: Faster tests (no re-seeding) but less isolation. If a test modifies a content type, it leaks to subsequent tests. Use this only for truly immutable seed data.

## Using Existing Fixtures Factories

The `_116.Tests.Fixtures` factories (`CategoryFactory`, `VideoFactory`, etc.) create in-memory entities suitable for unit tests. For integration tests, be aware of differences:

### When Fixtures Factories Work

- Creating entities to insert via DbContext (e.g., `CategoryFactory.Create()` + `context.Add()`)
- Creating command payloads for HTTP requests
- Generating fake data with consistent structure

### When They Don't Work

- Factories that set navigation properties via reflection — EF Core loads these from FK relationships, not manual assignment
- Entities that need real FK IDs from seeded data — use the actual seeded IDs, not random GUIDs
- Entities with auto-generated IDs that conflict with existing rows

### Recommendation

For repository tests, prefer creating entities inline with the domain factory methods (`CategoryEntity.Create(...)`) using real FK IDs from seeded data. Reserve the fixture factories for HTTP request payloads where you only need the shape, not the FK integrity.

## Anti-Patterns

### 1. Global Seed Data

Don't seed data in `PostgresFixture.InitializeAsync()`. It creates hidden dependencies and makes tests fragile. Each test class should seed its own data.

### 2. Over-Seeding

Don't seed 100 entities when the test only needs 3. Excessive seeding slows tests and makes failures harder to diagnose.

### 3. Shared Mutable IDs

Don't store seeded entity IDs in `static` fields. Tests may run in parallel across classes (though within a collection they run sequentially). Use instance fields on the test class.

### 4. Assuming Auto-Increment Order

PostgreSQL sequences may not start at 1 after Respawn. Always query by known GUIDs, not sequential IDs.
