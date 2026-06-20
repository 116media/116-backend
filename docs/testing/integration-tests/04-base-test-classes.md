# Base Test Classes

## Two Base Classes

Integration tests have two base classes mirroring the two test levels:

| Base Class | Purpose | Uses HttpClient | Uses DbContext |
|-----------|---------|-----------------|----------------|
| `BaseRepositoryTest` | Test EF Core repositories against real PostgreSQL | No | Yes |
| `BaseApiTest` | Test full HTTP request/response cycle | Yes | Yes (for seeding/assertion) |

## BaseRepositoryTest

For testing repository implementations (e.g., `CategoryRepository`, `VideoRepository`) directly against PostgreSQL without the HTTP pipeline.

```csharp
using Microsoft.EntityFrameworkCore;
using _116.Integration.Tests.Common.Fixtures;

namespace _116.Integration.Tests.Common.Abstractions;

/// <summary>
/// Base class for repository integration tests.
/// Provides a real DbContext connected to the Testcontainers PostgreSQL instance.
/// </summary>
[Collection("Database")]
public abstract class BaseRepositoryTest : IAsyncLifetime
{
    protected readonly PostgresFixture Db;

    protected BaseRepositoryTest(PostgresFixture db)
    {
        Db = db;
    }

    public async ValueTask InitializeAsync()
    {
        await Db.ResetAsync();
        await SeedAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Override to seed test data before each test method runs.
    /// Default implementation does nothing.
    /// </summary>
    protected virtual Task SeedAsync() => Task.CompletedTask;

    /// <summary>
    /// Creates a new DbContext instance for the specified module.
    /// Each call returns a fresh instance — use separate instances
    /// for "arrange" and "act" to avoid EF Core change tracker leaks.
    /// </summary>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(Db.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
    }
}
```

### Usage Pattern

```csharp
public class CategoryRepositoryTests : BaseRepositoryTest
{
    public CategoryRepositoryTests(PostgresFixture db) : base(db) { }

    [Fact]
    public async Task GetBySlugAsync_CaseInsensitive_ShouldFindCategory()
    {
        // Arrange — use one DbContext to seed
        await using var arrangeContext = CreateDbContext<ContentDbContext>();
        var category = CategoryEntity.Create(/* ... */);
        arrangeContext.Categories.Add(category);
        await arrangeContext.SaveChangesAsync();

        // Act — use a separate DbContext to query (avoids tracker interference)
        await using var actContext = CreateDbContext<ContentDbContext>();
        var repository = new CategoryRepository(actContext);
        CategoryEntity? result = await repository.GetBySlugAsync("MY-SLUG");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("my-slug");
    }
}
```

### Why Separate DbContext Instances

EF Core's change tracker caches entities. If you seed and query with the same DbContext, the query may return the cached entity without hitting the database — hiding real query bugs. Always use separate instances:

- **Arrange DbContext** — insert seed data, then dispose
- **Act DbContext** — create the repository, run the query
- **Assert** — verify the result from the Act DbContext

## BaseApiTest

For testing the full HTTP pipeline — endpoint routing, auth, validation, error responses.

```csharp
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using _116.Integration.Tests.Common.Fixtures;

namespace _116.Integration.Tests.Common.Abstractions;

/// <summary>
/// Base class for API integration tests.
/// Provides an HttpClient connected to the in-process WebApplicationFactory
/// and database access for seeding and verification.
/// </summary>
[Collection("Database")]
public abstract class BaseApiTest : IAsyncLifetime, IDisposable
{
    protected readonly PostgresFixture Db;
    protected readonly ApiFixture Api;
    protected readonly HttpClient Client;

    protected BaseApiTest(PostgresFixture db)
    {
        Db = db;
        Api = new ApiFixture(db);
        Client = Api.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await Db.ResetAsync();
        await SeedAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Override to seed test data before each test method runs.
    /// </summary>
    protected virtual Task SeedAsync() => Task.CompletedTask;

    /// <summary>
    /// Creates a new DbContext instance for the specified module.
    /// Use separate instances for seeding (arrange) and verification (assert)
    /// to avoid EF Core change tracker interference.
    /// </summary>
    protected TDbContext CreateDbContext<TDbContext>()
        where TDbContext : DbContext
    {
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(Db.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), options)!;
    }

    /// <summary>
    /// Creates a scoped service from the test application's DI container.
    /// Useful for resolving seeders, repositories, or DbContexts.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        return Api.Services.CreateScope().ServiceProvider
            .GetRequiredService<T>();
    }

    public void Dispose()
    {
        Client.Dispose();
        Api.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

### Usage Pattern

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

public class PublicGetActiveCategoriesEndpointTests : BaseApiTest
{
    public PublicGetActiveCategoriesEndpointTests(PostgresFixture db)
        : base(db) { }

    protected override async Task SeedAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var videoType = ContentTypeEntity.Create(/* Video */);
        context.ContentTypes.Add(videoType);

        var category = CategoryEntity.Create(/* ... */);
        context.Categories.Add(category);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Get_ShouldReturnActiveCategories()
    {
        HttpResponseMessage response = await Client.GetAsync(
            $"{ApiRoutes.Public.Categories}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content
            .ReadFromJsonAsync<GetActiveCategoriesResponse>();

        body!.Categories.Should().ContainSingle();
    }

    [Fact]
    public async Task Get_WithInvalidVersion_ShouldReturn404()
    {
        HttpResponseMessage response = await Client.GetAsync(
            "/api/v99/public/categories");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

public class AdminCreateCategoryEndpointTests : BaseApiTest
{
    public AdminCreateCategoryEndpointTests(PostgresFixture db)
        : base(db) { }

    [Fact]
    public async Task Post_AsAdmin_ShouldReturn201()
    {
        // Direct JWT — no seeding, no login round-trip
        Client.AuthenticateAsAdmin();
        var request = new CreateCategoryRequestBuilder().Build();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_AsVisitor_ShouldReturn403()
    {
        Client.AuthenticateAsVisitor();
        var request = new CreateCategoryRequestBuilder().Build();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Post_WithoutAuth_ShouldReturn401()
    {
        var request = new CreateCategoryRequestBuilder().Build();

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## Write Meaningful Assertions

Every assertion must prove something specific about the behavior under test. Testing that a value is "not null" or "not empty" proves almost nothing — it tells you the system returned *something*, not that it returned the *right thing*. A test that passes when the code is broken is worse than no test at all.

### Bad — weak assertions that pass even if the logic is wrong

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

[Fact]
public async Task Get_ShouldReturnCategory()
{
    HttpResponseMessage response = await Client.GetAsync(
        $"{ApiRoutes.Public.Categories}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await response.Content
        .ReadFromJsonAsync<PaginatedResponse<CategoryDto>>();

    // These prove nothing — any category would pass
    body.Should().NotBeNull();
    body!.Items.Should().NotBeEmpty();
    body.Items.First().Name.Should().NotBeNullOrEmpty();
    body.Items.First().Id.Should().NotBe(Guid.Empty);
}
```

### Good — strong assertions that verify exact behavior

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

[Fact]
public async Task Get_ShouldReturnActiveCategories()
{
    HttpResponseMessage response = await Client.GetAsync(
        $"{ApiRoutes.Public.Categories}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await response.Content
        .ReadFromJsonAsync<PaginatedResponse<CategoryDto>>();

    // Assert against the exact data we seeded
    body!.Items.Should().HaveCount(2);
    body.Count.Should().Be(2);

    CategoryDto first = body.Items.First();
    first.Name.Should().Be("Music Videos");
    first.Slug.Should().Be("music-videos");
    first.ContentTypeName.Should().Be("Video");
    first.IsActive.Should().BeTrue();

    // Verify ordering
    body.Items.Should().BeInDescendingOrder(c => c.CreatedAt);

    // Verify inactive categories are excluded
    body.Items.Should().OnlyContain(c => c.IsActive);
}
```

### Rules of thumb

1. **Assert against seeded values** — you seeded `"Music Videos"`, so assert `Name.Should().Be("Music Videos")`, not `Name.Should().NotBeNullOrEmpty()`
2. **Assert counts exactly** — `HaveCount(3)` not `NotBeEmpty()`. If your query should return 3, and it returns 47, `NotBeEmpty()` still passes
3. **Assert field values, not just existence** — `Slug.Should().Be("music-videos")` not `Slug.Should().NotBeNull()`
4. **Assert what should be absent** — if inactive categories must be excluded, assert `OnlyContain(c => c.IsActive)` or `Should().NotContain(c => c.Slug == "inactive-slug")`
5. **Assert ordering** — if the endpoint sorts by `CreatedAt` descending, assert `BeInDescendingOrder(c => c.CreatedAt)`
6. **Assert side effects** — if creating a category should set `CreatedBy`, query the DB and assert `category.CreatedBy.Should().Be(userId)`
7. **Assert error shapes** — for 422 responses, assert specific field names and messages: `errors["Slug"].Should().Contain("already exists")`, not just `StatusCode.Should().Be(422)`

### Database assertions follow the same rule

```csharp
// Bad — proves the row exists, nothing else
var saved = await context.Categories.FindAsync(id);
saved.Should().NotBeNull();

// Good — proves the data is correct
var saved = await context.Categories
    .Include(c => c.ContentType)
    .FirstAsync(c => c.Id == id);

saved.Name.Should().Be("Music Videos");
saved.Slug.Should().Be("music-videos");
saved.IsActive.Should().BeTrue();
saved.ContentType.Name.Should().Be("Video");
saved.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
```

## Anti-Patterns to Avoid

### 1. Sharing State Between Tests

Each test method must be independent. `ResetAsync()` runs before each test class, but tests within the same class share the database state. If test A inserts a row and test B depends on its absence, tests become order-dependent.

**Fix**: Seed only what the specific test needs, or accept that all tests in a class share the same seed data from `SeedAsync()`.

### 2. Using the Same DbContext for Seed and Assert

```csharp
// BAD — change tracker returns cached entity, never hits DB
var context = CreateDbContext<ContentDbContext>();
context.Categories.Add(category);
await context.SaveChangesAsync();
var result = await context.Categories.FindAsync(category.Id); // cached!
```

```csharp
// GOOD — separate contexts force a real DB round-trip
await using var seedContext = CreateDbContext<ContentDbContext>();
seedContext.Categories.Add(category);
await seedContext.SaveChangesAsync();

await using var queryContext = CreateDbContext<ContentDbContext>();
var result = await queryContext.Categories.FindAsync(category.Id); // hits DB
```

### 3. Depending on Seed Data Order

Don't assume `SERIAL` IDs or insertion order. Use explicit IDs (GUIDs) and query by known values.

### 4. Leaking HttpClient

Always dispose `HttpClient` and `ApiFixture`. The `BaseApiTest` handles this via `IDisposable`, but if you create additional clients, dispose them manually.

### 5. Testing Handler Logic in Integration Tests

Integration tests should verify the HTTP contract — status codes, response shapes, auth behavior. Don't re-test handler logic (edge cases, business rules) that unit tests already cover with mocks. Focus on what unit tests cannot test.
