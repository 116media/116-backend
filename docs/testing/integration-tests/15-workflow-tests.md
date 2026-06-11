# Cross-Module Workflow Tests

## Why Workflow Tests

Individual repository and endpoint tests verify single operations in isolation. Workflow tests verify that **multi-step, cross-module flows** work end-to-end — the kind of scenarios that break when module boundaries have mismatched contracts, missing seeders, or incorrect FK relationships.

These tests use `BaseApiTest` with real HTTP requests through the full pipeline.

## Authentication Flow

Tests the complete Identity module lifecycle: signup → email verification → login → token refresh → sign out → session revocation.

```csharp
[Collection("Database")]
public class AuthenticationFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task FullAuthenticationLifecycle()
    {
        // Step 1: Sign up
        var signupRequest = new
        {
            Email = "workflow-test@example.com",
            Password = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        HttpResponseMessage signupResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SignUp}", signupRequest);
        signupResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 2: Verify the user exists in DB (simulate email verification)
        await using (var context = CreateDbContext<IdentityDbContext>())
        {
            AuthUserEntity user = await context.Users
                .FirstAsync(u => u.Email == "workflow-test@example.com");
            user.Verify();
            await context.SaveChangesAsync();
        }

        // Step 3: Login
        var loginRequest = new
        {
            Email = "workflow-test@example.com",
            Password = "SecurePassword123!"
        };

        HttpResponseMessage loginResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.Login}", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginBody = await loginResponse.Content
            .ReadFromJsonAsync<LoginResponse>();
        loginBody!.AccessToken.Split('.').Should().HaveCount(3);
        loginBody.RefreshToken.Should().NotBeNullOrEmpty();

        // Step 4: Access protected endpoint with token
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginBody.AccessToken);

        HttpResponseMessage profileResponse = await Client.GetAsync(
            $"{ApiRoutes.Public.Me}/{UserRouteConstants.Profile}");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Refresh token
        var refreshRequest = new { RefreshToken = loginBody.RefreshToken };
        HttpResponseMessage refreshResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Auth}/{SessionRouteConstants.RefreshToken}", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshBody = await refreshResponse.Content
            .ReadFromJsonAsync<LoginResponse>();
        refreshBody!.AccessToken.Should().NotBe(loginBody.AccessToken);

        // Step 6: Sign out
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshBody.AccessToken);

        HttpResponseMessage signoutResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Auth}/{AuthRouteConstants.SignOut}", null);
        signoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 7: Old token should no longer work
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", refreshBody.AccessToken);

        HttpResponseMessage afterSignout = await Client.GetAsync(
            $"{ApiRoutes.Public.Me}/{UserRouteConstants.Profile}");
        afterSignout.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## Content Publication Flow

Tests creating content as an admin and verifying it appears on public endpoints: login as admin → create category → create video → publish → verify public visibility.

```csharp
[Collection("Database")]
public class ContentPublicationFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CreateAndPublishVideo_ShouldBeVisiblePublicly()
    {
        // Step 1: Seed required data (content type, admin user)
        Guid contentTypeId = await SeedContentType("Video");
        Client.AuthenticateAsAdmin();

        // Step 2: Create a category
        var categoryRequest = new
        {
            Name = "Workflow Test Category",
            Slug = "workflow-test-category",
            Description = "Test category for workflow",
            ContentTypeId = contentTypeId,
            IsActive = true
        };

        HttpResponseMessage categoryResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Categories}", categoryRequest);
        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        Guid categoryId = await ExtractIdFromResponse(categoryResponse);

        // Step 3: Create a draft video
        var videoRequest = new
        {
            Title = "Workflow Test Video",
            Slug = "workflow-test-video",
            Description = "Test video",
            CategoryId = categoryId,
            YoutubeUrl = "https://youtube.com/watch?v=test123"
        };

        HttpResponseMessage videoResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Videos}", videoRequest);
        videoResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        Guid videoId = await ExtractIdFromResponse(videoResponse);

        // Step 4: Publish the video
        HttpResponseMessage publishResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Videos}/{videoId}/{EditorialRouteConstants.Publish}", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Verify public visibility (anonymous)
        Client.DefaultRequestHeaders.Authorization = null;

        HttpResponseMessage publicResponse = await Client.GetAsync(
            $"{ApiRoutes.Public.Videos}");
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var publicBody = await publicResponse.Content
            .ReadFromJsonAsync<PaginatedResponse<VideoSummaryDto>>();
        publicBody!.Items.Should().ContainSingle(v => v.Slug == "workflow-test-video");

        // Step 6: Draft video should NOT appear publicly
        var draftRequest = new
        {
            Title = "Draft Video",
            Slug = "draft-video",
            Description = "Should not be visible",
            CategoryId = categoryId,
            YoutubeUrl = "https://youtube.com/watch?v=draft"
        };

        Client.AuthenticateAsAdmin();
        await Client.PostAsJsonAsync($"{ApiRoutes.Admin.Videos}", draftRequest);

        Client.DefaultRequestHeaders.Authorization = null;
        HttpResponseMessage publicCheck = await Client.GetAsync(
            $"{ApiRoutes.Public.Videos}");
        var checkBody = await publicCheck.Content
            .ReadFromJsonAsync<PaginatedResponse<VideoSummaryDto>>();
        checkBody!.Items.Should().NotContain(v => v.Slug == "draft-video");
    }
}
```

## Interaction Flow

Tests the content interaction lifecycle: login as visitor → like → bookmark → comment → verify counts → unlike → verify counts decremented.

```csharp
[Collection("Database")]
public class InteractionFlowTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task LikeAndUnlikeArticle_ShouldUpdateCounts()
    {
        // Step 1: Seed a published article and a visitor user
        Guid articleId = await SeedPublishedArticle();
        Client.AuthenticateAsVisitor();

        // Step 2: Like the article
        HttpResponseMessage likeResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/{InteractionsRouteConstants.Likes}", null);
        likeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Verify like count increased
        HttpResponseMessage articleResponse = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}");
        var article = await articleResponse.Content
            .ReadFromJsonAsync<ArticleDto>();
        article!.LikeCount.Should().Be(1);
        article.IsLikedByCurrentUser.Should().BeTrue();

        // Step 4: Unlike (toggle)
        HttpResponseMessage unlikeResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/{InteractionsRouteConstants.Likes}", null);
        unlikeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Verify like count decremented
        HttpResponseMessage afterUnlike = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}");
        var afterBody = await afterUnlike.Content
            .ReadFromJsonAsync<ArticleDto>();
        afterBody!.LikeCount.Should().Be(0);
        afterBody.IsLikedByCurrentUser.Should().BeFalse();
    }

    [Fact]
    public async Task BookmarkArticle_ShouldPersistAcrossRequests()
    {
        // Step 1: Seed and authenticate
        Guid articleId = await SeedPublishedArticle();
        Client.AuthenticateAsVisitor();

        // Step 2: Bookmark
        HttpResponseMessage bookmarkResponse = await Client.PostAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/{InteractionsRouteConstants.Bookmarks}", null);
        bookmarkResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Verify bookmark persists via article detail
        HttpResponseMessage articleResponse = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}");
        var article = await articleResponse.Content
            .ReadFromJsonAsync<ArticleDto>();
        article!.IsBookmarkedByCurrentUser.Should().BeTrue();
    }

    [Fact]
    public async Task CommentOnArticle_ShouldAppearInCommentList()
    {
        // Step 1: Seed and authenticate
        Guid articleId = await SeedPublishedArticle();
        Client.AuthenticateAsVisitor();

        // Step 2: Add comment
        var commentRequest = new { Content = "Great article!" };
        HttpResponseMessage commentResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/{InteractionsRouteConstants.Comments}", commentRequest);
        commentResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 3: Verify comment appears
        HttpResponseMessage commentsResponse = await Client.GetAsync(
            $"{ApiRoutes.Public.Articles}/{articleId}/{InteractionsRouteConstants.Comments}");
        var comments = await commentsResponse.Content
            .ReadFromJsonAsync<PaginatedResponse<CommentDto>>();
        comments!.Items.Should().ContainSingle(c => c.Content == "Great article!");
    }

    [Fact]
    public async Task RateVideo_ShouldUpdateAverageRating()
    {
        // Step 1: Seed a published video
        Guid videoId = await SeedPublishedVideo();
        Client.AuthenticateAsVisitor();

        // Step 2: Rate the video
        var rateRequest = new { Value = 4 };
        HttpResponseMessage rateResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Public.Videos}/{videoId}/{InteractionsRouteConstants.Ratings}", rateRequest);
        rateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Verify rating
        HttpResponseMessage videoResponse = await Client.GetAsync(
            $"{ApiRoutes.Public.Videos}/{videoId}");
        var video = await videoResponse.Content
            .ReadFromJsonAsync<VideoDto>();
        video!.AverageRating.Should().Be(4.0);
        video.RatingCount.Should().Be(1);
    }
}
```

## Order Lifecycle Flow

Tests the commerce workflow: login → create order → add items → submit → verify payment → verify customer access.

```csharp
[Collection("Database")]
public class OrderLifecycleTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task CompleteOrderLifecycle()
    {
        // Step 1: Seed required data (customer, published content with pricing)
        Guid customerId = await SeedCustomer();
        Guid videoId = await SeedPublishedVideoWithPricing();
        Client.AuthenticateAsAdmin();

        // Step 2: Create order
        var createOrderRequest = new
        {
            CustomerId = customerId,
            Items = new[] { new { ContentItemId = videoId, Quantity = 1 } }
        };

        HttpResponseMessage createResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Orders}", createOrderRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        Guid orderId = await ExtractIdFromResponse(createResponse);

        // Step 3: Submit order
        HttpResponseMessage submitResponse = await Client.PostAsync(
            $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Submit}", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Verify payment
        var paymentRequest = new
        {
            PaymentMethod = "MobileMoney",
            TransactionReference = "TXN-123456"
        };

        HttpResponseMessage paymentResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Orders}/{orderId}/{CommerceRouteConstants.Payment}/{CommerceRouteConstants.Verify}", paymentRequest);
        paymentResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 5: Verify order status in DB
        await using var context = CreateDbContext<ContentDbContext>();
        var order = await context.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == orderId);

        order.Status.Should().Be(OrderStatus.Completed);
        order.Items.Should().HaveCount(1);
    }
}
```

## Design Principles for Workflow Tests

### 1. Self-Contained Data

Each workflow test seeds all data it needs. No dependency on other tests or shared seeders.

### 2. Verify Through the API, Not the DB

Prefer asserting via public API responses (what the user sees) over direct DB queries. Only query the DB to verify state that isn't exposed via API (e.g., password hashes, internal flags).

### 3. Test the Happy Path + Key Error Paths

Each workflow should have:
- One happy-path test covering the full lifecycle
- Key error-path tests (e.g., unauthorized access, invalid state transitions)

### 4. Use Descriptive Method Names

```csharp
// Good — describes the full flow
public async Task CreateAndPublishVideo_ShouldBeVisiblePublicly()

// Bad — too vague
public async Task TestVideoWorkflow()
```

### 5. Keep Workflows Short

Each workflow test should complete in under 5 seconds. If a workflow has too many steps, split it into smaller flows.

## Test File Locations

```
tests/Integration/
└── Workflows/
    ├── AuthenticationFlowTests.cs
    ├── ContentPublicationFlowTests.cs
    ├── InteractionFlowTests.cs
    └── OrderLifecycleTests.cs
```
