# Spec 04 — Tests

Mirrors existing published-articles and popular-tags test conventions. The user runs
`dotnet test` themselves — do not run it here.

---

## 1. Mock: `MockPopularArticlesCacheInvalidator`

**File:** `tests/Unit/Common/Mocks/Infrastructure/MockPopularArticlesCacheInvalidator.cs`

Mirror `MockPopularTagsCacheInvalidator`:

```csharp
using _116.Content.Application.Shared.Cache;
using Moq;

namespace _116.Tests.Unit.Common.Mocks.Infrastructure;

/// <summary>
/// Factory and helpers for a mocked <see cref="IPopularArticlesCacheInvalidator" />.
/// </summary>
public static class MockPopularArticlesCacheInvalidator
{
    /// <summary>
    /// Creates a mock whose eviction token never cancels, so cache entries survive within a test.
    /// </summary>
    public static Mock<IPopularArticlesCacheInvalidator> Create()
    {
        Mock<IPopularArticlesCacheInvalidator> mock = new();
        mock.Setup(x => x.GetEvictionToken()).Returns(CancellationToken.None);
        return mock;
    }

    /// <summary>
    /// Asserts <see cref="IPopularArticlesCacheInvalidator.Invalidate" /> was called exactly once.
    /// </summary>
    public static void VerifyInvalidateCalled(this Mock<IPopularArticlesCacheInvalidator> mock)
        => mock.Verify(x => x.Invalidate(), Times.Once);

    /// <summary>
    /// Asserts <see cref="IPopularArticlesCacheInvalidator.Invalidate" /> was never called.
    /// </summary>
    public static void VerifyInvalidateNotCalled(this Mock<IPopularArticlesCacheInvalidator> mock)
        => mock.Verify(x => x.Invalidate(), Times.Never);
}
```

---

## 2. Mock: extend `MockArticleRepository`

**File:** `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`

Add (mirrors `SetupGetAllAsync`):

```csharp
/// <summary>
/// Sets up <see cref="IArticleRepository.GetPopularArticlesAsync" /> to return the given list.
/// </summary>
public static Mock<IArticleRepository> SetupGetPopularArticlesAsync(
    this Mock<IArticleRepository> mock,
    IReadOnlyList<ArticleEntity> articles)
{
    mock.Setup(x => x.GetPopularArticlesAsync(
            It.IsAny<int>(),
            It.IsAny<Guid?>(),
            It.IsAny<Guid?>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(articles);
    return mock;
}
```

---

## 3. Handler unit tests

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPopularArticles/PublicGetPopularArticlesHandlerTests.cs`

```csharp
public class PublicGetPopularArticlesHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IPopularArticlesCacheInvalidator> _cacheInvalidatorMock;
    private readonly IMemoryCache _cache;
    private readonly PublicGetPopularArticlesHandler _handler;

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
}
```

Test methods:

```csharp
[Fact]
public async Task Handle_WhenPopularArticlesExist_ShouldReturnMappedList()
{
    List<ArticleEntity> articles = ArticleFactory.CreateManyPublished(CategoryId, 3);
    _articleRepositoryMock.SetupGetPopularArticlesAsync(articles);

    var query = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

    PublicGetPopularArticlesResult result = await _handler.Handle(query, CancellationToken.None);

    result.Should().NotBeNull();
    result.Articles.Count.Should().Be(articles.Count);
}

[Fact]
public async Task Handle_ShouldPassArgumentsToRepository()
{
    var categoryId = Guid.NewGuid();
    var excludeId = Guid.NewGuid();
    _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 1));

    var query = new PublicGetPopularArticlesQuery(Limit: 7, CategoryId: categoryId, ExcludeId: excludeId);

    await _handler.Handle(query, CancellationToken.None);

    _articleRepositoryMock.Verify(
        x => x.GetPopularArticlesAsync(7, categoryId, excludeId, It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task Handle_CalledTwiceWithSameArgs_ShouldHitRepositoryOnce()
{
    _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 3));
    var query = new PublicGetPopularArticlesQuery(Limit: 5, CategoryId: null, ExcludeId: null);

    await _handler.Handle(query, CancellationToken.None);
    await _handler.Handle(query, CancellationToken.None);

    _articleRepositoryMock.Verify(
        x => x.GetPopularArticlesAsync(5, null, null, It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task Handle_CalledWithDifferentExcludeId_ShouldHitRepositoryTwice()
{
    _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 3));

    await _handler.Handle(new PublicGetPopularArticlesQuery(5, null, Guid.NewGuid()), CancellationToken.None);
    await _handler.Handle(new PublicGetPopularArticlesQuery(5, null, Guid.NewGuid()), CancellationToken.None);

    _articleRepositoryMock.Verify(
        x => x.GetPopularArticlesAsync(It.IsAny<int>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
        Times.Exactly(2));
}

[Fact]
public async Task Handle_ShouldNotCallInvalidate()
{
    _articleRepositoryMock.SetupGetPopularArticlesAsync(ArticleFactory.CreateManyPublished(CategoryId, 3));

    await _handler.Handle(new PublicGetPopularArticlesQuery(5, null, null), CancellationToken.None);

    _cacheInvalidatorMock.VerifyInvalidateNotCalled();
}
```

`CategoryId` comes from `BaseContentHandlerTest` if it exposes one; otherwise declare a private
`static readonly Guid CategoryId = Guid.NewGuid();` in the test class (match how
`PublicGetPublishedArticlesHandlerTests` obtains its `CategoryId`).

---

## 4. Mutation-handler invalidation tests

For each of the 7 engagement handlers and the publish/archive handlers, extend the existing
handler test to assert `_cacheInvalidatorMock.VerifyInvalidateCalled()` after a successful
mutation. Inject the mocked `IPopularArticlesCacheInvalidator` into the handler under test
(these handlers already have their own test classes and their own mock setup — add one field
and one assertion).

---

## 5. Integration tests

**File:** `tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1Tests.cs`

Base: `BaseApiTest(PostgresFixture db)`, `[Collection("Database")]`. This is where the SQL
`ORDER BY` scoring is actually proven.

```csharp
[Collection("Database")]
public class PublicGetPopularArticlesEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task GetPopularArticles_AsAnonymous_OrdersByWeightedEngagementScore()
    {
        Guid categoryId = await SeedCategoryAsync();

        // weights 4/3/2/1 (like/comment/share/bookmark): high=5 likes=>20, mid=3 comments=>9, low=4 shares=>8
        ArticleEntity high = await SeedArticleAsync(categoryId, likes: 5);
        ArticleEntity mid  = await SeedArticleAsync(categoryId, comments: 3);
        ArticleEntity low  = await SeedArticleAsync(categoryId, shares: 4);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/popular?limit=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Select(a => a.Id).Should().ContainInOrder(high.Id, mid.Id, low.Id);
    }

    [Fact]
    public async Task GetPopularArticles_ExcludesGivenId()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity top = await SeedArticleAsync(categoryId, likes: 5);
        ArticleEntity other = await SeedArticleAsync(categoryId, bookmarks: 1);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/popular?limit=10&excludeId={top.Id}");

        var body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().NotContain(a => a.Id == top.Id);
        body.Articles.Should().Contain(a => a.Id == other.Id);
    }

    [Fact]
    public async Task GetPopularArticles_FiltersByCategory()
    {
        Guid categoryA = await SeedCategoryAsync();
        Guid categoryB = await SeedCategoryAsync();
        ArticleEntity inA = await SeedArticleAsync(categoryA, likes: 3);
        await SeedArticleAsync(categoryB, likes: 9);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/popular?categoryId={categoryA}");

        var body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().OnlyContain(a => a.CategoryId == categoryA);
        body.Articles.Should().Contain(a => a.Id == inA.Id);
    }

    [Fact]
    public async Task GetPopularArticles_ReturnsOnlyPublished()
    {
        Guid categoryId = await SeedCategoryAsync();
        ArticleEntity draft = await SeedAsync<ContentDbContext, ArticleEntity>(ctx =>
        {
            ArticleEntity a = ArticleFactory.Create(categoryId); // Draft
            for (int i = 0; i < 50; i++) a.IncrementShareCount();
            ctx.Articles.Add(a);
            return a;
        });

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/popular?limit=10");

        var body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Should().NotContain(a => a.Id == draft.Id);
        body.Articles.Should().OnlyContain(a => a.Status == EnumContentStatus.Published);
    }

    [Fact]
    public async Task GetPopularArticles_RespectsLimit()
    {
        Guid categoryId = await SeedCategoryAsync();
        await SeedArticleAsync(categoryId, likes: 1);
        await SeedArticleAsync(categoryId, likes: 2);
        await SeedArticleAsync(categoryId, likes: 3);

        Client.ClearAuthentication();

        var response = await Client.GetAsync($"{ApiRoutes.Public.Articles}/popular?limit=2");

        var body = await response.ReadAsAsync<PublicGetPopularArticlesResponse>();
        body.Articles.Count.Should().BeLessThanOrEqualTo(2);
    }

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

    // SeedCategoryAsync: copy from PublicGetPublishedArticlesEndpointV1Tests
    // (seeds a ContentTypeEntity + CategoryEntity, returns category.Id).
}
```

### Cache-invalidation integration test

```csharp
[Fact]
public async Task GetPopularArticles_AfterEngagementChange_ReflectsNewRanking()
{
    Guid categoryId = await SeedCategoryAsync();

    ArticleEntity quiet = await SeedArticleAsync(categoryId);
    ArticleEntity leader = await SeedArticleAsync(categoryId, bookmarks: 1);

    Client.ClearAuthentication();

    var first = await (await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10"))
        .ReadAsAsync<PublicGetPopularArticlesResponse>();
    first.Articles.First().Id.Should().Be(leader.Id); // leader (1 bookmark => 1) ahead of quiet (0)

    // drive quiet's score above leader via the real share endpoint
    // (anonymous, and the handler invalidates the cache after commit)
    await Client.PostAsync(Routes.Public.Articles.Shares(quiet.Id), null);

    var second = await (await Client.GetAsync($"{Routes.Public.Articles.Popular()}?limit=10"))
        .ReadAsAsync<PublicGetPopularArticlesResponse>();
    second.Articles.First().Id.Should().Be(quiet.Id); // quiet (1 share => 2) now ahead of leader (1)
}
```

The share endpoint is used (rather than like) because it is `AllowAnonymous` — the whole
test runs unauthenticated, and exercising the real endpoint proves the end-to-end
invalidation wiring: mutation handler → `Invalidate()` → evicted cache → fresh ranking.

### Base test cache reset

**File:** `tests/Integration/Common/Base/BaseApiTest.cs`

In `InitializeAsync`, alongside the existing `InvalidateTagCache()`, add a reset for the
popular-articles invalidator so ranked results do not bleed across tests:

```csharp
private void InvalidatePopularArticlesCache()
{
    using var scope = Api.Services.CreateScope();
    var invalidator = scope.ServiceProvider.GetRequiredService<IPopularArticlesCacheInvalidator>();
    invalidator.Invalidate();
}
```

Call it in `InitializeAsync` after `InvalidateTagCache()`.

---

## Tasks

- [x] Create `MockPopularArticlesCacheInvalidator`
- [x] Add `SetupGetPopularArticlesAsync` to `MockArticleRepository`
- [x] Create `PublicGetPopularArticlesHandlerTests` (mapping, arg-forwarding, cache hit/miss, no-invalidate)
- [x] Extend the 7 engagement + publish/archive handler tests to assert `VerifyInvalidateCalled()`
- [x] Create `PublicGetPopularArticlesEndpointV1Tests` (order, exclude, category, published-only, limit, tie-break)
- [x] Add the cache-invalidation integration test (drives ranking via the anonymous share endpoint)
- [x] Add `InvalidatePopularArticlesCache()` to `BaseApiTest.InitializeAsync`
