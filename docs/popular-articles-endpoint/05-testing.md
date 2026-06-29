# Testing

Tests mirror the existing published-articles and popular-tags conventions exactly. The user
runs `dotnet test` themselves — do not run it here.

---

## Conventions to mirror

| Concern | Convention | Reference |
|---------|-----------|-----------|
| Unit base class | `BaseContentHandlerTest` (provides `IMapper Mapper`) | `tests/Unit/Common/BaseContentHandlerTest.cs` |
| Mock article repo | `MockArticleRepository.Create()` + fluent `SetupXxx` | `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs` |
| Cache in unit tests | **real** `new MemoryCache(...)`, **mocked** invalidator | `PublicGetPopularTagsHandlerTests` |
| Article factory | `ArticleFactory.CreatePublished(categoryId)`, `CreateManyPublished(categoryId, n)` | `tests/Fixtures/Factories/Content/ArticleFactory.cs` |
| Counters in fixtures | **no** builder setter — call `article.IncrementLikeCount()` etc. after Build | — |
| Integration base | `BaseApiTest(PostgresFixture db)`, `[Collection("Database")]` | `tests/Integration/Common/Base/BaseApiTest.cs` |
| Seeding | `SeedAsync<ContentDbContext, TEntity>(ctx => ...)` | `PublicGetPublishedArticlesEndpointV1Tests` |
| Assertions | `AwesomeAssertions` (`.Should()...`) | all tests |
| Route constants | `TestConstants.ApiRoutes.Public.Articles` | `tests/Fixtures/Constants/Shared/TestConstants.ApiRoutes.cs` |

**Important fixture note:** `ArticleBuilder` has **no** `WithLikeCount` / `WithShareCount` /
etc. Engagement counters are set by calling the domain increment methods on the built entity.
For a seeded article with, say, 5 shares and 2 comments:

```csharp
ArticleEntity a = ArticleFactory.CreatePublished(categoryId);
for (int i = 0; i < 5; i++) a.IncrementShareCount();
for (int i = 0; i < 2; i++) a.IncrementCommentCount();
```

A small local test helper (e.g. `WithEngagement(article, likes, comments, shares, bookmarks)`)
keeps the arrange blocks readable — it just loops the increment methods.

---

## Unit tests — handler

`tests/Unit/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPopularArticles/PublicGetPopularArticlesHandlerTests.cs`

Constructor mirrors the popular-tags handler test (real cache, mocked invalidator):

```csharp
public PublicGetPopularArticlesHandlerTests()
{
    _articleRepositoryMock = MockArticleRepository.Create();
    _fileRepositoryMock = MockFileRepository.Create();
    _cacheInvalidatorMock = MockPopularArticlesCacheInvalidator.Create();
    _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
    _handler = new PublicGetPopularArticlesHandler(
        _articleRepositoryMock.Object,
        _fileRepositoryMock.Object,
        _cache,
        _cacheInvalidatorMock.Object,
        Mapper
    );
}
```

Cases:

| Test | Asserts |
|------|---------|
| `Handle_WhenPopularArticlesExist_ShouldReturnMappedList` | result count matches repo return; DTOs mapped |
| `Handle_ShouldPassLimitCategoryAndExcludeToRepository` | repo called with the exact `limit`/`categoryId`/`excludeId` args |
| `Handle_CalledTwiceWithSameArgs_ShouldHitRepositoryOnce` | second call served from cache (`Times.Once` on `GetPopularArticlesAsync`) |
| `Handle_CalledWithDifferentExcludeId_ShouldHitRepositoryTwice` | different cache keys → repo hit per distinct args |
| `Handle_ShouldNotCallInvalidate` | reads never invalidate (`VerifyInvalidateNotCalled()`) |

> Scoring/ordering correctness itself is proven where the ordering actually runs — the query
> builder against a real database, i.e. in the integration test below. Unit tests over a
> mocked repository only prove the handler forwards args and caches; they cannot exercise the
> SQL `ORDER BY`.

Add a `MockArticleRepository.SetupGetPopularArticlesAsync(list)` extension mirroring
`SetupGetAllAsync`:

```csharp
public static Mock<IArticleRepository> SetupGetPopularArticlesAsync(
    this Mock<IArticleRepository> mock,
    IReadOnlyList<ArticleEntity> articles)
{
    mock.Setup(x => x.GetPopularArticlesAsync(
            It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(articles);
    return mock;
}
```

And a `MockPopularArticlesCacheInvalidator` mirroring `MockPopularTagsCacheInvalidator`
(`Create()` returning `CancellationToken.None`, `VerifyInvalidateCalled/NotCalled`).

---

## Unit tests — invalidator wiring on mutation handlers

For each engagement/publish handler that gains an `Invalidate()` call (see
`06-caching-and-rollout.md`), add/extend a test asserting the invalidator fires after commit,
using `_cacheInvalidatorMock.VerifyInvalidateCalled()`. Handlers:

- `PublicLikeArticleHandler`, `PublicUnlikeArticleHandler`
- `PublicAddArticleCommentHandler`, `PublicDeleteArticleCommentHandler`
- `PublicShareArticleHandler`
- `PublicBookmarkArticleHandler`, `PublicUnbookmarkArticleHandler`
- `AdminPublishArticleHandler`, `AdminArchiveArticleHandler` (and any unpublish/delete that
  removes a published article from ranking)

---

## Integration tests — the ordering is proven here

`tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1Tests.cs`

Seed published articles with **known, distinct** engagement so the expected order is
unambiguous under the `4/3/2/1` weights, then assert the returned order.

```csharp
[Fact]
public async Task GetPopularArticles_AsAnonymous_OrdersByWeightedEngagementScore()
{
    Guid categoryId = await SeedCategoryAsync();

    // scores: high = 5 likes (20), mid = 3 comments (9), low = 4 shares (8)
    ArticleEntity high = await SeedArticleAsync(categoryId, likes: 5);
    ArticleEntity mid  = await SeedArticleAsync(categoryId, comments: 3);
    ArticleEntity low  = await SeedArticleAsync(categoryId, shares: 4);

    Client.ClearAuthentication();

    var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/popular?limit=10");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    PublicGetPopularArticlesResponse body =
        await response.ReadAsAsync<PublicGetPopularArticlesResponse>();

    body.Articles.Select(a => a.Id).Should()
        .ContainInOrder(high.Id, mid.Id, low.Id);
}
```

Additional cases:

| Test | Asserts |
|------|---------|
| `GetPopularArticles_ExcludesGivenId` | `?excludeId={high.Id}` → `high` absent, order otherwise preserved |
| `GetPopularArticles_FiltersByCategory` | `?categoryId=` → only that category's articles returned |
| `GetPopularArticles_ReturnsOnlyPublished` | seed a draft with huge counts → it never appears |
| `GetPopularArticles_RespectsLimit` | `?limit=2` → at most 2 items |
| `GetPopularArticles_TieBrokenByPublishedAtDesc` | two equal-score articles → newer `PublishedAt` first |

Seed helper (uses the counter increments, since the builder has no counter setters):

```csharp
private async Task<ArticleEntity> SeedArticleAsync(
    Guid categoryId, int likes = 0, int comments = 0, int shares = 0, int bookmarks = 0)
{
    return await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
    {
        ArticleEntity a = ArticleFactory.CreatePublished(categoryId);
        for (int i = 0; i < likes; i++) a.IncrementLikeCount();
        for (int i = 0; i < comments; i++) a.IncrementCommentCount();
        for (int i = 0; i < shares; i++) a.IncrementShareCount();
        for (int i = 0; i < bookmarks; i++) a.IncrementBookmarkCount();
        ctx.Articles.Add(a);
        return a;
    });
}
```

`SeedCategoryAsync` is the same helper used by `PublicGetPublishedArticlesEndpointV1Tests`
(seeds a `ContentTypeEntity` + `CategoryEntity`).

### Cache behavior in integration

`BaseApiTest.InitializeAsync` already invalidates the tags cache before each test via the
registered invalidator. Add the same reset for the popular-articles invalidator (resolve
`IPopularArticlesCacheInvalidator` and call `Invalidate()`) so the in-process cache does not
bleed ranked results across tests. Because `ApiFixture` disables rate limiting, the
`ContentBrowsing` policy does not interfere with test throughput.

A dedicated cache-invalidation integration test:

| Test | Steps |
|------|-------|
| `GetPopularArticles_AfterEngagementChange_ReflectsNewRanking` | seed quiet(score 0) and leader(1 bookmark = 1); GET → leader first; POST an anonymous share on quiet (share weight 2); GET again → quiet first (proves the share mutation busted the cache) |
