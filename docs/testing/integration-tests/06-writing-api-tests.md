# Writing API Integration Tests

## What to Test

API integration tests verify the full HTTP request/response contract:

| Concern | Example |
|---------|---------|
| Endpoint routing | `GET /api/v1/public/categories` returns 200 |
| Request/response shape | Response body matches expected DTO structure |
| Authentication | Anonymous vs. authenticated access |
| Authorization | Admin-only endpoints reject visitor tokens |
| Validation | Invalid input returns 400 with validation errors |
| Error responses | Not found returns 404 ProblemDetails |
| Rate limiting | Excessive requests return 429 |
| Content negotiation | JSON serialization with enum-as-string |
| Pagination | `pageIndex` and `pageSize` query params work correctly |
| API versioning | `/api/v1/` routes resolve, unknown versions return 404 |

## Test File Structure

```
tests/Integration/Modules/Content/Endpoints/
├── Public/
│   ├── PublicGetActiveCategoriesEndpointTests.cs
│   ├── PublicGetExclusiveCategoryEndpointTests.cs
│   ├── PublicGetPublishedVideosEndpointTests.cs
│   └── PublicGetPublishedArticlesEndpointTests.cs
├── Admin/
│   ├── AdminCreateCategoryEndpointTests.cs
│   ├── AdminUpdateCategoryEndpointTests.cs
│   ├── AdminSetExclusiveCategoryEndpointTests.cs
│   ├── AdminPublishArticleEndpointTests.cs
│   └── AdminCreateVideoEndpointTests.cs
└── Auth/
    ├── PublicLoginEndpointTests.cs
    ├── PublicSignUpEndpointTests.cs
    └── AdminLoginEndpointTests.cs
```

## Naming Convention

```
{HttpMethod}_{Scenario}_{ExpectedStatusCode}
```

Examples:
- `Get_WithActiveCategories_ShouldReturn200WithCategories`
- `Get_WhenUnauthenticated_ShouldReturn401`
- `Post_WithInvalidPayload_ShouldReturn400WithValidationErrors`
- `Delete_WhenCategoryNotFound_ShouldReturn404`
- `Post_AsSuperAdmin_ShouldReturn201`

## Example: Public Endpoint Tests

```csharp
using System.Net;
using System.Net.Http.Json;
using _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory.V1;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Integration.Tests.Common.Abstractions;
using _116.Integration.Tests.Common.Fixtures;
using AwesomeAssertions;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Modules.Content.Endpoints.Public;

/// <summary>
/// Integration tests for GET /api/v1/public/categories/exclusive.
/// </summary>
public class PublicGetExclusiveCategoryEndpointTests : BaseApiTest
{
    private static readonly string Endpoint = $"{ApiRoutes.Public.Categories}/exclusive";

    public PublicGetExclusiveCategoryEndpointTests(PostgresFixture db)
        : base(db) { }

    [Fact]
    public async Task Get_WithExclusiveCategory_ShouldReturn200()
    {
        // Arrange — seed a video content type + exclusive category
        await SeedExclusiveCategoryAsync();

        // Act
        HttpResponseMessage response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content
            .ReadFromJsonAsync<PublicGetExclusiveCategoryResponse>();

        body.Should().NotBeNull();
        body!.Category.Should().NotBeNull();
        body.Category.IsExclusive.Should().BeTrue();
        body.Videos.Should().NotBeNull();
    }

    [Fact]
    public async Task Get_WhenNoExclusiveCategory_ShouldReturn404()
    {
        // Arrange — empty database (no exclusive category)

        // Act
        HttpResponseMessage response = await Client.GetAsync(Endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_WithPagination_ShouldRespectPageSize()
    {
        // Arrange
        await SeedExclusiveCategoryWithVideosAsync(videoCount: 15);

        // Act
        HttpResponseMessage response = await Client.GetAsync(
            $"{Endpoint}?pageIndex=0&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content
            .ReadFromJsonAsync<PublicGetExclusiveCategoryResponse>();

        body!.Videos.Items.Should().HaveCount(5);
        body.Videos.Count.Should().Be(15);
    }

    [Fact]
    public async Task Get_ShouldAllowAnonymousAccess()
    {
        // Arrange
        await SeedExclusiveCategoryAsync();

        // Act — no auth header
        HttpResponseMessage response = await Client.GetAsync(Endpoint);

        // Assert — should not return 401
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    #region Helpers

    private async Task SeedExclusiveCategoryAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var videoType = ContentTypeEntity.Create(
            Guid.NewGuid(), "Video", "Video content");
        context.ContentTypes.Add(videoType);

        var category = CategoryEntity.Create(
            Guid.NewGuid(), videoType.Id, "Exclusive Show",
            "exclusive-show", "The exclusive show", false,
            TestErrorsFactory.CreateCategoryErrors(),
            false, true);
        context.Categories.Add(category);
        await context.SaveChangesAsync();
    }

    private async Task SeedExclusiveCategoryWithVideosAsync(int videoCount)
    {
        // Seed exclusive category + N published videos in that category
    }

    #endregion
}
```

## Example: Authenticated Admin Endpoint Tests

```csharp
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Modules.Content.Endpoints.Admin;

/// <summary>
/// Integration tests for POST /api/v1/admin/categories.
/// </summary>
public class AdminCreateCategoryEndpointTests : BaseApiTest
{
    private static readonly string Endpoint = $"{ApiRoutes.Admin.Categories}";

    public AdminCreateCategoryEndpointTests(PostgresFixture db)
        : base(db) { }

    protected override async Task SeedAsync()
    {
        // Seed auth data so we can get JWT tokens
        var seeder = new TestDataSeeder(Api.Services);
        await seeder.SeedAuthenticationDataAsync();
        await seeder.SeedContentTypesAsync();
    }

    [Fact]
    public async Task Post_AsAdmin_ShouldReturn201()
    {
        // Arrange
        Client.AuthenticateAsAdmin();
        Guid contentTypeId = await SeedContentTypeAsync("Video");
        var payload = new
        {
            ContentTypeId = contentTypeId,
            Name = "New Category",
            Slug = "new-category",
            Description = "A new category",
            IsFree = false,
            IsGossip = false,
            IsExclusive = false
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Post_WhenUnauthenticated_ShouldReturn401()
    {
        // Act — no auth header
        var payload = new { name = "Test" };
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithDuplicateSlug_ShouldReturn409()
    {
        // Arrange — seed a category with slug "existing-slug"
        Client.AuthenticateAsAdmin();

        // Create first category
        Guid contentTypeId = await SeedContentTypeAsync("Video");
        var payload = new
        {
            ContentTypeId = contentTypeId,
            Name = "Existing Category",
            Slug = "existing-slug",
            Description = "First category",
            IsFree = false,
            IsGossip = false,
            IsExclusive = false
        };
        await Client.PostAsJsonAsync(Endpoint, payload);

        // Act — create second with same slug
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Post_WithInvalidPayload_ShouldReturn400()
    {
        // Arrange
        Client.AuthenticateAsAdmin();
        var payload = new { name = "", slug = "" }; // Empty required fields

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## Example: Authentication Flow Tests

```csharp
using _116.Identity.Application.Auth.Constants;
using static _116.Tests.Fixtures.Constants.TestConstants;

namespace _116.Integration.Tests.Modules.Identity.Endpoints.Auth;

/// <summary>
/// Integration tests for the public login flow.
/// </summary>
public class PublicLoginEndpointTests : BaseApiTest
{
    private static readonly string Endpoint = $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.Login}";

    public PublicLoginEndpointTests(PostgresFixture db) : base(db) { }

    protected override async Task SeedAsync()
    {
        var seeder = new TestDataSeeder(Api.Services);
        await seeder.SeedAuthenticationDataAsync();
    }

    [Fact]
    public async Task Post_WithValidCredentials_ShouldReturn200WithTokens()
    {
        // Arrange — SuperAdmin is seeded with known credentials
        var payload = new
        {
            email = "superadmin@test.com",
            password = "TestPassword123!"
        };

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Split('.').Should().HaveCount(3);
        body.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Post_WithInvalidPassword_ShouldReturn401()
    {
        var payload = new
        {
            email = "superadmin@test.com",
            password = "WrongPassword!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_WithNonexistentEmail_ShouldReturn401()
    {
        var payload = new
        {
            email = "nobody@test.com",
            password = "AnyPassword123!"
        };

        HttpResponseMessage response = await Client.PostAsJsonAsync(
            Endpoint, payload);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## ProblemDetails Assertion Helper

```csharp
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace _116.Integration.Tests.Common.Extensions;

/// <summary>
/// Extensions for asserting ProblemDetails error responses.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Reads the response body as ProblemDetails and asserts the status code.
    /// </summary>
    public static async Task<ProblemDetails> ReadProblemDetailsAsync(
        this HttpResponseMessage response)
    {
        var problem = await response.Content
            .ReadFromJsonAsync<ProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Status.Should().Be((int)response.StatusCode);

        return problem;
    }
}
```

## Test Coverage Priority

Focus API integration tests on endpoints that unit tests cannot cover:

### High Priority (Unit Tests Skip Entirely)

1. **All Carter `AddRoutes()` methods** — routing, middleware, auth
2. **Authentication flow** — login, signup, token refresh, sign out
3. **Authorization enforcement** — admin-only, visitor-only, super-admin-only
4. **Error response format** — ProblemDetails structure

### Medium Priority (Unit Tests Cover Partially)

5. **Validation pipeline** — FluentValidation through the decorator
6. **Pagination** — query param binding, default values
7. **Cross-module flows** — Content endpoint using Core file service

### Low Priority (Unit Tests Cover Well)

8. **Business logic edge cases** — already covered by handler unit tests
9. **Entity domain rules** — already covered by domain unit tests

## Rate Limiting Tests

Rate limiting is enforced by middleware. To test it:

```csharp
[Fact]
public async Task Get_ExceedingRateLimit_ShouldReturn429()
{
    // Act — send many requests rapidly
    var tasks = Enumerable.Range(0, 100)
        .Select(_ => Client.GetAsync(Endpoint));
    HttpResponseMessage[] responses = await Task.WhenAll(tasks);

    // Assert — at least one should be 429
    responses.Should().Contain(r =>
        r.StatusCode == HttpStatusCode.TooManyRequests);
}
```

Note: Rate limit thresholds are configured in `RateLimitConstants`. You may need to adjust the request count based on the `ContentBrowsing` fixed window policy settings.
