# Phase 15: Cross-Module Workflow Tests Spec

## Tasks

### User Registration → Content Creation Flow
- [ ] `UserRegistrationToContentCreationTests.cs`
  - [ ] SignUp_Login_CreateCategory_ShouldWork
  - [ ] SignUp_VerifyOtp_Login_ShouldReturnValidTokens

### Order Lifecycle
- [ ] `OrderLifecycleTests.cs`
  - [ ] CreateOrder_AddItems_Submit_AttachPayment_Verify_ShouldComplete
  - [ ] CreateOrder_Submit_Cancel_ShouldTransitionCorrectly
  - [ ] CreateOrder_AddItems_Submit_AttachPayment_Reject_ShouldComplete

### Content Publishing Lifecycle
- [ ] `ContentPublishingLifecycleTests.cs`
  - [ ] CreateArticle_Submit_Approve_Publish_ShouldBePublished
  - [ ] CreateVideo_Submit_Reject_ShouldBeRejected
  - [ ] CreateVideo_Submit_Approve_Publish_Archive_ShouldBeArchived

### Authorization Matrix
- [ ] `AuthorizationMatrixTests.cs`
  - [ ] AllAdminEndpoints_AsAnonymous_ShouldReturn401
  - [ ] AllAdminEndpoints_AsVisitor_ShouldReturn403
  - [ ] SuperAdminOnlyEndpoints_AsAdmin_ShouldReturn403

### Session Management Flow
- [ ] `SessionManagementFlowTests.cs`
  - [ ] Login_GetSessions_RevokeSession_ShouldWork
  - [ ] Login_ForceLogout_ShouldRevokeAllSessions

## Test Approach

Workflow tests use `BaseApiTest` and exercise multiple endpoints in sequence within a single test. They verify that cross-cutting concerns (auth, audit, domain events) work correctly across module boundaries.

```csharp
[Collection("Database")]
public class ContentPublishingLifecycleTests(PostgresFixture db) : BaseApiTest(db)
{
    protected override async Task SeedAsync()
    {
        await using var context = CreateDbContext<ContentDbContext>();
        var videoType = ContentTypeEntity.Create(Guid.NewGuid(), "Video", "Videos");
        context.ContentTypes.Add(videoType);
        var category = CategoryEntity.Create(/* ... with videoType.Id */);
        context.Categories.Add(category);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task CreateArticle_Submit_Approve_Publish_ShouldBePublished()
    {
        // Step 1: Create
        Client.AuthenticateAsAdmin();
        var createRequest = new CreateArticleRequestBuilder()
            .WithCategoryId(_categoryId)
            .Build();
        var createResponse = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Articles}", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Extract article ID from response
        var created = await createResponse.Content.ReadFromJsonAsync<ArticleDto>();
        var articleId = created!.Id;

        // Step 2: Submit
        var submitResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Articles}/{articleId}/submit", null);
        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Approve
        var approveResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Articles}/{articleId}/approve", null);
        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Publish
        var publishResponse = await Client.PatchAsync(
            $"{ApiRoutes.Admin.Articles}/{articleId}/publish", null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify final state
        await using var context = CreateDbContext<ContentDbContext>();
        var article = await context.Articles.FindAsync(articleId);
        article!.Status.Should().Be(EnumContentStatus.Published);
    }
}
```

## File Locations

```
tests/_116.Integration.Tests/Workflows/
├── UserRegistrationToContentCreationTests.cs
├── OrderLifecycleTests.cs
├── ContentPublishingLifecycleTests.cs
├── AuthorizationMatrixTests.cs
└── SessionManagementFlowTests.cs
```

## Acceptance Criteria

1. End-to-end flows verified across module boundaries
2. Each workflow test exercises at least 3 API calls in sequence
3. Final database state verified with exact assertions
4. Authorization matrix covers all critical endpoints
5. `./scripts/run-tests-with-coverage.sh integration` passes
