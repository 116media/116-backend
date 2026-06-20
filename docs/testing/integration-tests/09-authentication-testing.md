# Authentication & Authorization Testing

## Overview

The application uses JWT Bearer authentication with three roles: **SuperAdmin**, **Admin**, **Visitor**. Integration tests must verify that endpoints enforce authentication and authorization correctly.

## Two Authentication Strategies

| Strategy | Method | Speed | DB Required | Use Case |
|----------|--------|-------|-------------|----------|
| Direct JWT | `AuthenticateAsAdmin()`, `AuthenticateAsVisitor()`, `AuthenticateAsSuperAdmin()` | Instant (synchronous) | No | 95% of tests — auth is not the thing under test |
| Seed-then-login | `AuthenticateViaLoginAsync()` | ~50ms (async, hits real endpoint) | Yes (seeded user) | Auth flow tests where login itself is under test |

## HttpClient Auth Extensions

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Constants;
using Microsoft.IdentityModel.Tokens;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Common.Extensions;

/// <summary>
/// Auth helper record for login response deserialization.
/// </summary>
public record AuthTokenResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("refreshToken")] string RefreshToken
);

/// <summary>
/// Extensions for authenticating HttpClient instances in integration tests.
/// Provides two strategies: direct JWT minting (fast, no DB) and real login (for auth flow tests).
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Sets the Authorization header with a JWT containing the SuperAdmin role.
    /// No database seeding required — the token is minted in-memory.
    /// </summary>
    public static void AuthenticateAsSuperAdmin(this HttpClient client)
    {
        string token = GenerateToken(
            userId: Guid.NewGuid(),
            email: "superadmin@test.com",
            userName: "superadmin",
            role: "SuperAdmin");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Sets the Authorization header with a JWT containing the Admin role.
    /// No database seeding required — the token is minted in-memory.
    /// </summary>
    public static void AuthenticateAsAdmin(this HttpClient client)
    {
        string token = GenerateToken(
            userId: Guid.NewGuid(),
            email: "admin@test.com",
            userName: "admin",
            role: "Admin");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Sets the Authorization header with a JWT containing the Visitor role.
    /// No database seeding required — the token is minted in-memory.
    /// </summary>
    public static void AuthenticateAsVisitor(this HttpClient client)
    {
        string token = GenerateToken(
            userId: Guid.NewGuid(),
            email: "visitor@test.com",
            userName: "visitor",
            role: "Visitor");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Sets the Authorization header with a JWT for a specific user ID and role.
    /// Useful when the test needs to assert against a known user ID (e.g., CreatedBy).
    /// </summary>
    public static void AuthenticateAs(
        this HttpClient client,
        Guid userId,
        string role,
        string email = "custom@test.com")
    {
        string token = GenerateToken(userId, email, email.Split('@')[0], role);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clears the Authorization header (makes subsequent requests anonymous).
    /// </summary>
    public static void ClearAuthentication(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Authenticates by hitting the real login endpoint.
    /// The user must already exist in the database (seeded beforehand).
    /// Returns the raw HttpResponseMessage so auth tests can assert on it.
    /// </summary>
    public static async Task<HttpResponseMessage> AuthenticateViaLoginAsync(
        this HttpClient client,
        string email,
        string password,
        string loginEndpoint = $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.Login}")
    {
        var payload = new { email, password };
        HttpResponseMessage response = await client.PostAsJsonAsync(
            loginEndpoint, payload);

        if (response.IsSuccessStatusCode)
        {
            var tokens = await response.Content
                .ReadFromJsonAsync<AuthTokenResponse>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);
        }

        return response;
    }

    /// <summary>
    /// Mints a JWT in-memory using the same secret, issuer, and audience
    /// that ApiFixture configures via TestConstants.Jwt.
    /// </summary>
    private static string GenerateToken(
        Guid userId,
        string email,
        string userName,
        string role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Jwt.ValidSecret));
        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, $"{userId}"),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Sub, $"{userId}"),
            new(JwtRegisteredClaimNames.Jti, $"{Guid.NewGuid()}"),
            new(ClaimTypes.Role, role),
            new(JwtClaimsConstants.IsVerified, "true", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.IsActive, "true", ClaimValueTypes.Boolean),
            new(JwtClaimsConstants.SessionId, $"{Guid.NewGuid()}"),
            new(JwtClaimsConstants.AuthProvider, "Credentials"),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(Jwt.AccessTokenExpirationMinutes),
            SigningCredentials = credentials,
            Issuer = Jwt.ValidIssuer,
            Audience = Jwt.ValidAudience,
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
```

## Test Patterns

### Pattern 1: Anonymous Access Allowed

```csharp
[Fact]
public async Task Get_WithoutAuth_ShouldReturn200()
{
    HttpResponseMessage response = await Client.GetAsync(
        $"{ApiRoutes.Public.Categories}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

### Pattern 2: Authentication Required

```csharp
[Fact]
public async Task Post_WithoutAuth_ShouldReturn401()
{
    var request = new CreateCategoryRequestBuilder().Build();

    HttpResponseMessage response = await Client.PostAsJsonAsync(
        $"{ApiRoutes.Admin.Categories}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}

[Fact]
public async Task Post_AsAdmin_ShouldReturn201()
{
    Client.AuthenticateAsAdmin();
    var request = new CreateCategoryRequestBuilder().Build();

    HttpResponseMessage response = await Client.PostAsJsonAsync(
        $"{ApiRoutes.Admin.Categories}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Pattern 3: Role-Based Authorization

```csharp
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
public async Task Post_AsSuperAdmin_ShouldReturn201()
{
    Client.AuthenticateAsSuperAdmin();
    var request = new CreateCategoryRequestBuilder().Build();

    HttpResponseMessage response = await Client.PostAsJsonAsync(
        $"{ApiRoutes.Admin.Categories}", request);

    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

### Pattern 4: Known User ID (for CreatedBy assertions)

```csharp
[Fact]
public async Task CreateCategory_ShouldSetCreatedByToAuthenticatedUserId()
{
    var userId = Guid.NewGuid();
    Client.AuthenticateAs(userId, "Admin");

    var request = new CreateCategoryRequestBuilder().Build();
    await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Categories}", request);

    await using var context = CreateDbContext<ContentDbContext>();
    CategoryEntity category = await context.Categories
        .OrderByDescending(c => c.CreatedAt)
        .FirstAsync();

    category.CreatedBy.Should().Be(userId);
}
```

### Pattern 5: Expired Token

```csharp
[Fact]
public async Task Get_WithExpiredToken_ShouldReturn401()
{
    Client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", GenerateExpiredToken());

    HttpResponseMessage response = await Client.GetAsync(
        $"{ApiRoutes.Admin.Users}");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}

private static string GenerateExpiredToken()
{
    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(Jwt.ValidSecret));
    var credentials = new SigningCredentials(
        key, SecurityAlgorithms.HmacSha256);

    var descriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, $"{Guid.NewGuid()}"),
            new Claim(ClaimTypes.Role, "Admin"),
        ]),
        Expires = DateTime.UtcNow.AddMinutes(-1),
        SigningCredentials = credentials,
        Issuer = Jwt.ValidIssuer,
        Audience = Jwt.ValidAudience,
    };

    var handler = new JwtSecurityTokenHandler();
    return handler.WriteToken(handler.CreateToken(descriptor));
}
```

### Pattern 6: Real Login Flow (auth tests only)

```csharp
[Fact]
public async Task Login_WithValidCredentials_ShouldReturnTokens()
{
    await SeedVerifiedUserAsync("user@test.com", ExternalServices.DefaultUserPassword);

    HttpResponseMessage response = await Client.AuthenticateViaLoginAsync(
        "user@test.com", ExternalServices.DefaultUserPassword);

    response.StatusCode.Should().Be(HttpStatusCode.OK);

    var body = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
    body!.AccessToken.Split('.').Should().HaveCount(3);
    body.RefreshToken.Should().NotBeNullOrEmpty();
}

[Fact]
public async Task Login_WithInvalidPassword_ShouldReturn401()
{
    await SeedVerifiedUserAsync("user@test.com", ExternalServices.DefaultUserPassword);

    HttpResponseMessage response = await Client.AuthenticateViaLoginAsync(
        "user@test.com", "WrongPassword123!");

    response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
}
```

## Authorization Matrix

Test each endpoint against the expected authorization behavior:

### Identity Module

| Endpoint | Anonymous | Visitor | Admin | SuperAdmin |
|----------|-----------|---------|-------|------------|
| `POST /public/auth/login` | 200 | - | - | - |
| `POST /public/auth/signup` | 201 | - | - | - |
| `POST /admin/auth/login` | 200 | - | - | - |
| `GET /admin/users` | 401 | 403 | 200 | 200 |
| `POST /admin/roles` | 401 | 403 | 403 | 201 |
| `GET /public/me/profile` | 401 | 200 | - | - |

### Content Module

| Endpoint | Anonymous | Visitor | Admin | SuperAdmin |
|----------|-----------|---------|-------|------------|
| `GET /public/categories` | 200 | 200 | - | - |
| `GET /public/categories/exclusive` | 200/404 | 200/404 | - | - |
| `POST /admin/categories` | 401 | 403 | 201 | 201 |
| `PUT /admin/categories/{id}` | 401 | 403 | 200 | 200 |
| `PATCH /admin/categories/{id}/exclusive` | 401 | 403 | 200 | 200 |
| `GET /public/videos` | 200 | 200 | - | - |
| `POST /admin/videos` | 401 | 403 | 201 | 201 |

## Seeding Users for Auth Flow Tests

Only needed when testing the login endpoint itself. For all other tests, use `AuthenticateAsAdmin()` / `AuthenticateAsVisitor()`.

```csharp
/// <summary>
/// Seeds a verified user via direct DB access for auth flow tests.
/// </summary>
protected async Task SeedVerifiedUserAsync(string email, string password)
{
    using var scope = Api.Services.CreateScope();
    var sp = scope.ServiceProvider;

    var passwordService = sp.GetRequiredService<IPasswordService>();
    string hash = passwordService.Hash(password);

    await using var context = sp.GetRequiredService<IdentityDbContext>();
    var visitorRole = await context.Roles
        .FirstAsync(r => r.Name == "Visitor");

    var user = UserEntity.Create(
        id: Guid.NewGuid(),
        email: Email.Create(email),
        userName: $"visitor_{Guid.NewGuid():N}"[..14],
        passwordHash: hash,
        errors: TestErrorsFactory.CreateUserErrors()
    );
    user.Verify();
    context.Users.Add(user);

    var userRole = UserRoleEntity.Create(user.Id, visitorRole.Id);
    context.UserRoles.Add(userRole);

    await context.SaveChangesAsync();
}
```

## Token Refresh Flow

```csharp
[Fact]
public async Task Post_RefreshToken_ShouldReturnNewTokens()
{
    await SeedVerifiedUserAsync("refresh@test.com", ExternalServices.DefaultUserPassword);

    HttpResponseMessage loginResponse = await Client.AuthenticateViaLoginAsync(
        "refresh@test.com", ExternalServices.DefaultUserPassword);
    var tokens = await loginResponse.Content
        .ReadFromJsonAsync<AuthTokenResponse>();

    var refreshPayload = new { refreshToken = tokens!.RefreshToken };
    HttpResponseMessage response = await Client.PostAsJsonAsync(
        $"{ApiRoutes.Public.Auth}/{SessionRouteConstants.RefreshToken}", refreshPayload);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var newTokens = await response.Content
        .ReadFromJsonAsync<AuthTokenResponse>();
    newTokens!.AccessToken.Should().NotBe(tokens.AccessToken);
}
```
