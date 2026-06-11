# WebApplicationFactory (API Fixture)

## Purpose

The API fixture boots the entire ASP.NET Core application in-process using `WebApplicationFactory<Program>`. It replaces the real PostgreSQL connection with the Testcontainers instance and stubs external services (Cloudinary, YouTube).

## ApiFixture

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Common.Fixtures;

/// <summary>
/// Custom WebApplicationFactory that wires the app to the Testcontainers
/// PostgreSQL instance and stubs external HTTP services.
/// </summary>
public class ApiFixture : WebApplicationFactory<Program>
{
    private readonly PostgresFixture _postgres;

    public ApiFixture(PostgresFixture postgres)
    {
        _postgres = postgres;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment variables BEFORE the host builds.
        // Modules read these in their static GetModuleOptions() methods.
        SetEnvironmentVariables();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace all three DbContext registrations to point at Testcontainers
            ReplaceDbContext<IdentityDbContext>(services);
            ReplaceDbContext<CoreDbContext>(services);
            ReplaceDbContext<ContentDbContext>(services);

            // Stub external HTTP services to avoid real network calls
            StubExternalServices(services);
        });
    }

    /// <summary>
    /// Replaces an existing DbContext registration with one pointing
    /// at the Testcontainers PostgreSQL instance.
    /// </summary>
    private void ReplaceDbContext<TDbContext>(IServiceCollection services)
        where TDbContext : DbContext
    {
        // Remove the existing DbContext and its options
        ServiceDescriptor? descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (descriptor is not null) services.Remove(descriptor);

        // Remove DbContextPool if registered
        ServiceDescriptor? poolDescriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (poolDescriptor is not null) services.Remove(poolDescriptor);

        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(_postgres.ConnectionString)
                   .UseSnakeCaseNamingConvention();
        });
    }

    /// <summary>
    /// Stubs external services that make real HTTP/network calls.
    /// </summary>
    private static void StubExternalServices(IServiceCollection services)
    {
        services.RemoveAll<ICloudinaryService>();
        services.AddScoped<ICloudinaryService, StubCloudinaryService>();

        services.RemoveAll<IYoutubeThumbnailService>();
        services.AddScoped<IYoutubeThumbnailService, StubYoutubeThumbnailService>();
    }

    /// <summary>
    /// Sets environment variables that modules read during registration.
    /// Must be called before the host builds because modules call
    /// AppEnvironment.Database() and AppEnvironment.Jwt() at registration time.
    /// </summary>
    private void SetEnvironmentVariables()
    {
        var connBuilder = new Npgsql.NpgsqlConnectionStringBuilder(
            _postgres.ConnectionString);

        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("POSTGRES_HOST", connBuilder.Host);
        Environment.SetEnvironmentVariable("POSTGRES_PORT", connBuilder.Port.ToString());
        Environment.SetEnvironmentVariable("POSTGRES_DB", connBuilder.Database);
        Environment.SetEnvironmentVariable("POSTGRES_USER", connBuilder.Username);
        Environment.SetEnvironmentVariable("POSTGRES_PASSWORD", connBuilder.Password);

        Environment.SetEnvironmentVariable("JWT_SECRET", Jwt.ValidSecret);
        Environment.SetEnvironmentVariable("JWT_ISSUER", Jwt.ValidIssuer);
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", Jwt.ValidAudience);
        Environment.SetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION",
            Jwt.AccessTokenExpirationMinutes.ToString());
        Environment.SetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION",
            Jwt.RefreshTokenExpirationDays.ToString());

        Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", ExternalServices.DefaultUserPassword);

        Environment.SetEnvironmentVariable("CLOUDINARY_CLOUD_NAME", ExternalServices.CloudinaryCloudName);
        Environment.SetEnvironmentVariable("CLOUDINARY_API_KEY", ExternalServices.CloudinaryApiKey);
        Environment.SetEnvironmentVariable("CLOUDINARY_API_SECRET", ExternalServices.CloudinaryApiSecret);

        Environment.SetEnvironmentVariable("DASHBOARD_ORIGIN", ExternalServices.DashboardOrigin);
        Environment.SetEnvironmentVariable("WEBAPP_ORIGIN", ExternalServices.WebappOrigin);
    }
}
```

## Why Replace DbContext Registrations

The production modules use `AddModuleDatabase()` which calls `AddDbContextPool<T>()` with `UseNpgsql(connectionString)` where the connection string comes from environment variables. We have two options:

1. **Set environment variables before host builds** — the modules read the Testcontainers connection string naturally
2. **Replace the DbContext registration in ConfigureServices** — override whatever the modules registered

We do **both** for safety: environment variables ensure modules that read the connection string during registration get the right value, and `ReplaceDbContext` ensures the DbContext options point at Testcontainers regardless.

## Creating an HttpClient

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

// In test class
HttpClient client = _apiFixture.CreateClient();

// The client hits the in-process app — no network, no port binding
var response = await client.GetAsync($"{ApiRoutes.Public.Categories}");
```

## Authenticated Requests

The project provides two authentication strategies, each for a different purpose:

| Helper | How It Works | When to Use |
|--------|-------------|-------------|
| `AuthenticateAsAdmin()` | Mints a JWT in-memory using `TestConstants.Jwt` | 95% of tests — fast, no DB seeding |
| `AuthenticateViaLoginAsync()` | Seeds a user, hits `POST /auth/login` | Auth flow tests where login itself is under test |

### Strategy 1: Direct JWT (default for most tests)

Mints a token in-memory — synchronous, instant, no database seeding required. Uses the same `SymmetricSecurityKey`, issuer, and audience that `ApiFixture` configures.

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

// One-liner — no seeding, no async, no login round-trip
Client.AuthenticateAsAdmin();

var response = await Client.GetAsync($"{ApiRoutes.Admin.Categories}");
response.StatusCode.Should().Be(HttpStatusCode.OK);
```

Test authorization by switching roles:

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

// Admin can create
Client.AuthenticateAsAdmin();
var response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}", request);
response.StatusCode.Should().Be(HttpStatusCode.Created);

// Visitor cannot
Client.AuthenticateAsVisitor();
response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}", request);
response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

// Anonymous cannot
Client.ClearAuthentication();
response = await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}", request);
response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
```

### Strategy 2: Seed-then-login (auth tests only)

Hits the real login endpoint. Use this **only** in `AuthenticationFlowTests` and `PublicLoginEndpointTests` where the login pipeline itself is the thing under test.

```csharp
// Seed a verified user first
await SeedVerifiedUserAsync("user@test.com", ExternalServices.DefaultUserPassword);

// Hit the real login endpoint
HttpResponseMessage response = await Client.AuthenticateViaLoginAsync(
    "user@test.com", ExternalServices.DefaultUserPassword);

response.StatusCode.Should().Be(HttpStatusCode.OK);
var body = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
body!.AccessToken.Split('.').Should().HaveCount(3);
```

See [09-authentication-testing.md](09-authentication-testing.md) for the full `HttpClientExtensions` implementation.

## Module Seeding in Tests

The modules check `ASPNETCORE_ENVIRONMENT == "Testing"` and disable seeding. This means no SuperAdmin, no Visitor role, and no content types are auto-seeded. Integration tests must seed their own data, which is intentional — it avoids hidden dependencies on seed data.

However, for tests that need those roles/users, create a `TestDataSeeder` that replicates the essential seed data:

```csharp
public class TestDataSeeder
{
    private readonly IServiceProvider _services;

    public TestDataSeeder(IServiceProvider services) => _services = services;

    /// <summary>
    /// Seeds the minimum data required for authentication flows.
    /// </summary>
    public async Task SeedAuthenticationDataAsync()
    {
        using var scope = _services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
        await seeder.SeedAllAsync();

        var visitorSeeder = scope.ServiceProvider.GetRequiredService<VisitorRoleSeeder>();
        await visitorSeeder.SeedAllAsync();
    }

    /// <summary>
    /// Seeds content types (Article, Video, ShortVideo, Lyrics).
    /// </summary>
    public async Task SeedContentTypesAsync()
    {
        using var scope = _services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ContentTypeSeeder>();
        await seeder.SeedAllAsync();
    }
}
```

## Fixture Composition with PostgresFixture

The `ApiFixture` depends on `PostgresFixture` for the connection string. In test classes:

```csharp
[Collection("Database")]
public class CategoryEndpointTests : IAsyncLifetime
{
    private readonly PostgresFixture _db;
    private readonly ApiFixture _api;
    private readonly HttpClient _client;

    public CategoryEndpointTests(PostgresFixture db)
    {
        _db = db;
        _api = new ApiFixture(db);
        _client = _api.CreateClient();
    }

    public async ValueTask InitializeAsync()
    {
        await _db.ResetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _api.DisposeAsync();
    }
}
```

## What Gets Tested

With `WebApplicationFactory`, every HTTP request flows through the full pipeline:

```
HttpClient.SendAsync()
  → Kestrel (in-process)
    → ForwardedHeaders middleware
    → Serilog request logging
    → Exception handler middleware
    → CORS middleware
    → Rate limiter middleware
    → Localization middleware
    → Authentication middleware (JWT Bearer)
    → Authorization middleware
    → API versioning
    → Carter endpoint routing
      → Dispatcher.Send()
        → ValidationDecorator (FluentValidation)
        → LoggingDecorator
        → Handler
          → Repository (real EF Core → real PostgreSQL)
    → Response serialization
  → HttpResponseMessage
```

This covers everything that unit tests cannot: middleware ordering, authentication, authorization, rate limiting, endpoint routing, and real database queries.
