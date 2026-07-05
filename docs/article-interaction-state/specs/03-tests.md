# Spec 03 — Tests

Unit + integration tests for both phases, mirroring the existing article read tests. Moq +
AwesomeAssertions; `Handle_WhenCondition_ShouldExpectedBehavior` naming; factories/mocks from
`tests/Fixtures/` and `tests/Unit/Common/Mocks/`.

---

## 3.1 Mock helper additions

**File:** `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`

`SetupHasLikedAsync` / `SetupHasBookmarkedAsync` already exist. Add the batch setups + verifies:

```csharp
/// <summary>
/// Configures GetLikedArticleIdsAsync to return the given id set for any user/id-list input.
/// </summary>
public MockArticleRepository SetupGetLikedArticleIds(HashSet<Guid> ids)
{
    Mock
        .Setup(r => r.GetLikedArticleIdsAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(ids);
    return this;
}

/// <summary>
/// Configures GetBookmarkedArticleIdsAsync to return the given id set for any input.
/// </summary>
public MockArticleRepository SetupGetBookmarkedArticleIds(HashSet<Guid> ids)
{
    Mock
        .Setup(r => r.GetBookmarkedArticleIdsAsync(
            It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(ids);
    return this;
}

/// <summary>
/// Verifies GetLikedArticleIdsAsync was invoked exactly the given number of times.
/// </summary>
public void VerifyGetLikedArticleIdsCalled(Times times) =>
    Mock.Verify(r => r.GetLikedArticleIdsAsync(
        It.IsAny<Guid>(), It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()), times);

/// <summary>
/// Verifies neither HasLikedAsync nor HasBookmarkedAsync was invoked (anonymous fast path).
/// </summary>
public void VerifyExistenceChecksNotCalled()
{
    Mock.Verify(r => r.HasLikedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    Mock.Verify(r => r.HasBookmarkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
```

---

## 3.2 Phase 1 unit — `PublicGetArticleBySlugHandlerTests`

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetArticleBySlug/PublicGetArticleBySlugHandlerTests.cs`

```csharp
[Fact]
public async Task Handle_WhenAnonymous_ShouldReturnFalseFlagsAndSkipExistenceChecks()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
    var query = new PublicGetArticleBySlugQuery(Slug: article.Slug, CurrentUserId: null);
    _articleRepositoryMock.SetupGetBySlug(article.Slug, article);

    // Act
    PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Article.IsLiked.Should().BeFalse();
    result.Article.IsBookmarked.Should().BeFalse();
    _articleRepositoryMock.VerifyExistenceChecksNotCalled();
}

[Fact]
public async Task Handle_WhenUserLikedAndBookmarked_ShouldReturnTrueFlags()
{
    // Arrange
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
    var query = new PublicGetArticleBySlugQuery(Slug: article.Slug, CurrentUserId: Guid.NewGuid());
    _articleRepositoryMock.SetupGetBySlug(article.Slug, article);
    _articleRepositoryMock.SetupHasLikedAsync(true);
    _articleRepositoryMock.SetupHasBookmarkedAsync(true);

    // Act
    PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    result.Article.IsLiked.Should().BeTrue();
    result.Article.IsBookmarked.Should().BeTrue();
}

[Fact]
public async Task Handle_WhenUserLikedButNotBookmarked_ShouldReflectEachFlagIndependently()
{
    ArticleEntity article = ArticleFactory.CreatePublished(CategoryId);
    var query = new PublicGetArticleBySlugQuery(Slug: article.Slug, CurrentUserId: Guid.NewGuid());
    _articleRepositoryMock.SetupGetBySlug(article.Slug, article);
    _articleRepositoryMock.SetupHasLikedAsync(true);
    _articleRepositoryMock.SetupHasBookmarkedAsync(false);

    PublicGetArticleBySlugResult result = await _handler.Handle(query, CancellationToken.None);

    result.Article.IsLiked.Should().BeTrue();
    result.Article.IsBookmarked.Should().BeFalse();
}
```

---

## 3.3 Phase 2 unit — repository batch methods

Follow the existing repository-test convention (real/in-memory provider):

```csharp
[Fact]
public async Task GetLikedArticleIdsAsync_ShouldReturnOnlyIdsTheUserLiked()
{
    // Arrange: user liked a1 and a3 (not a2); another user liked a2.
    // Act
    HashSet<Guid> liked = await _repository.GetLikedArticleIdsAsync(userId, [a1, a2, a3], CancellationToken.None);
    // Assert
    liked.Should().BeEquivalentTo(new[] { a1, a3 });
    liked.Should().NotContain(a2);
}

[Fact]
public async Task GetLikedArticleIdsAsync_WithEmptyInput_ShouldReturnEmptySet()
{
    HashSet<Guid> liked = await _repository.GetLikedArticleIdsAsync(userId, [], CancellationToken.None);
    liked.Should().BeEmpty();
}
```

Mirror both for `GetBookmarkedArticleIdsAsync`.

---

## 3.4 Phase 2 unit — feed handlers

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPublishedArticles/PublicGetPublishedArticlesHandlerTests.cs`

```csharp
[Fact]
public async Task Handle_WhenAnonymous_ShouldReturnAllFalseFlagsAndSkipBatchLookups()
{
    List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
    var query = new PublicGetPublishedArticlesQuery(
        PaginatedRequest: new PaginatedRequest(0, 10), Search: null, CategoryId: null, TagSlug: null);
    _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);

    PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

    result.Articles.Items.Should().OnlyContain(a => !a.IsLiked && !a.IsBookmarked);
    _articleRepositoryMock.VerifyGetLikedArticleIdsCalled(Times.Never());
}

[Fact]
public async Task Handle_WhenAuthenticated_ShouldStampOnlyInteractedItems_WithSingleBatchPerType()
{
    List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
    Guid likedId = articles[0].Id;
    Guid bookmarkedId = articles[1].Id;
    var query = new PublicGetPublishedArticlesQuery(
        PaginatedRequest: new PaginatedRequest(0, 10), Search: null, CategoryId: null, TagSlug: null)
        { CurrentUserId = Guid.NewGuid() }; // or a positional variant per the record shape

    _articleRepositoryMock.SetupGetAllAsync(articles, articles.Count);
    _articleRepositoryMock.SetupGetLikedArticleIds([likedId]);
    _articleRepositoryMock.SetupGetBookmarkedArticleIds([bookmarkedId]);

    PublicGetPublishedArticlesResult result = await _handler.Handle(query, CancellationToken.None);

    result.Articles.Items.Single(a => a.Id == likedId).IsLiked.Should().BeTrue();
    result.Articles.Items.Single(a => a.Id == bookmarkedId).IsBookmarked.Should().BeTrue();
    result.Articles.Items.Single(a => a.Id == articles[2].Id).IsLiked.Should().BeFalse();

    _articleRepositoryMock.VerifyGetLikedArticleIdsCalled(Times.Once());
}
```

Add the equivalent anonymous + authenticated + single-batch tests for the promoted handler and
the promotion-feed handler (the promotion-feed test asserts the batch is called **once** total
across all sub-collections).

---

## 3.5 Integration — Phase 1 (get-by-slug)

**File:** `tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetArticleBySlug/V1/PublicGetArticleBySlugEndpointV1Tests.cs`

```csharp
[Fact]
public async Task GetArticleBySlug_WhenUserLikedAndBookmarked_ReturnsTrueFlags()
{
    Guid categoryId = await SeedCategoryAsync();
    ArticleEntity article = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
    {
        ArticleEntity a = ArticleFactory.CreatePublished(categoryId);
        ctx.Articles.Add(a);
        return a;
    });
    Guid userId = /* seeded visitor id */;
    await SeedAsync<ContentDbContext, ArticleLikeEntity>(ctx =>
    {
        var like = ArticleLikeEntity.Create(Guid.NewGuid(), userId, article.Id);
        ctx.ArticleLikes.Add(like);
        return like;
    });
    await SeedAsync<ContentDbContext, ArticleBookmarkEntity>(ctx =>
    {
        var bm = ArticleBookmarkEntity.Create(Guid.NewGuid(), userId, article.Id);
        ctx.ArticleBookmarks.Add(bm);
        return bm;
    });

    Client.AuthenticateAs(userId, "Visitor");

    var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
    body.Article.IsLiked.Should().BeTrue();
    body.Article.IsBookmarked.Should().BeTrue();
}

[Fact]
public async Task GetArticleBySlug_WhenAnonymous_ReturnsFalseFlags()
{
    // seed a published article with a like by some user...
    Client.ClearAuthentication();
    var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");
    PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
    body.Article.IsLiked.Should().BeFalse();
    body.Article.IsBookmarked.Should().BeFalse();
}

[Fact]
public async Task GetArticleBySlug_WhenDifferentUser_DoesNotLeakAnotherUsersState()
{
    // user A liked the article; request as user B.
    Client.AuthenticateAs(userB, "Visitor");
    var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/{article.Slug}");
    PublicGetArticleBySlugResponse body = await response.ReadAsAsync<PublicGetArticleBySlugResponse>();
    body.Article.IsLiked.Should().BeFalse();      // A's like must not appear for B
    body.Article.IsBookmarked.Should().BeFalse();
}
```

---

## 3.6 Integration — Phase 2 (feed) + cross-user cache-leak gate

Under the published / promoted / promotion-feed integration folders:

```csharp
[Fact]
public async Task GetPublishedArticles_WhenAuthenticated_StampsOnlyTheUsersInteractions()
{
    // seed 3 published articles; user liked article[0], bookmarked article[1].
    Client.AuthenticateAs(userId, "Visitor");
    var response = await Client.GetAsync(ApiRoutes.Public.Articles);
    PublicGetPublishedArticlesResponse body = await response.ReadAsAsync<PublicGetPublishedArticlesResponse>();

    body.Articles.Items.Single(a => a.Id == article0.Id).IsLiked.Should().BeTrue();
    body.Articles.Items.Single(a => a.Id == article1.Id).IsBookmarked.Should().BeTrue();
    body.Articles.Items.Single(a => a.Id == article2.Id).IsLiked.Should().BeFalse();
}

[Fact]
public async Task GetPublishedArticles_DoesNotLeakInteractionStateAcrossUsers()
{
    // user A liked articleX. Request the feed as A, then as B.
    Client.AuthenticateAs(userA, "Visitor");
    var _ = await Client.GetAsync(ApiRoutes.Public.Articles);

    Client.AuthenticateAs(userB, "Visitor");
    var responseB = await Client.GetAsync(ApiRoutes.Public.Articles);
    PublicGetPublishedArticlesResponse bodyB = await responseB.ReadAsAsync<PublicGetPublishedArticlesResponse>();

    bodyB.Articles.Items.Single(a => a.Id == articleX.Id).IsLiked.Should().BeFalse();
}
```

> The cross-user test is the **caching-correctness gate**: it passes now (no cache) and must
> keep passing if/when the feed is cached per `07-caching-and-rollout.md`.

---

## Tasks

- [ ] Add `SetupGetLikedArticleIds` / `SetupGetBookmarkedArticleIds` / `VerifyGetLikedArticleIdsCalled` / `VerifyExistenceChecksNotCalled` to `MockArticleRepository`.
- [ ] Phase 1 unit: anonymous-false + skip-checks, liked+bookmarked-true, independent-flags in `PublicGetArticleBySlugHandlerTests`.
- [ ] Phase 2 unit: `GetLikedArticleIdsAsync` / `GetBookmarkedArticleIdsAsync` correctness (subset, other-user isolation, empty input).
- [ ] Phase 2 unit: anonymous-all-false + skip-batch, authenticated-stamps-subset, single-batch-per-type for published/promoted/promotion-feed handlers.
- [ ] Integration Phase 1: liked/bookmarked-true, anonymous-false, cross-user-no-leak on get-by-slug.
- [ ] Integration Phase 2: stamps-only-users-interactions + cross-user-no-leak on the feed(s).
- [ ] User runs `dotnet test` (do not run it here).
