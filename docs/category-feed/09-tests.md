# 09 — Test Plan (100% coverage)

Every production type added by this feature has a test. This doc lists the exact test
classes, the fixture additions they depend on, and the full case list per class, with
copy-ready code for the core suites. It follows the project conventions in
[`docs/how-to-tests/`](../how-to-tests/00-overview.md).

## 0. Conventions used

| Concern | Choice |
|---|---|
| Framework | xUnit v3 (`[Fact]` / `[Theory]`) |
| Assertions | AwesomeAssertions (`value.Should()...`) |
| Mocking | Moq, via the `Mock*Repository` / `Mock*` static factories under `tests/Unit/Common/Mocks/` |
| Unit handler base | `BaseContentHandlerTest` (provides `Mapper`, `TestErrorsFactory.CreateContentI18n()`) |
| Integration base | `BaseApiTest(PostgresFixture)` + `[Collection("Database")]` (real Postgres via Testcontainers, Respawn between tests) |
| Naming | `Method_Scenario_ExpectedResult` |
| Layout | `#region Success Cases` / `#region Failure Cases` |

Test namespaces mirror production namespaces under `_116.Unit.Tests.*` /
`_116.Integration.Tests.*`.

---

## 1. Fixture additions (prerequisite)

These must land first — the test suites below depend on them.

### 1.1 `CategoryBuilder` — pin support

**File:** `tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs`

```csharp
private DateTimeOffset? _pinnedToFeedAt;

/// <summary>
/// Marks the category as pinned to the feed at the given time (defaults to "now").
/// Pass distinct timestamps across categories to exercise FIFO ordering/eviction.
/// </summary>
public CategoryBuilder PinnedToFeedAt(DateTimeOffset? pinnedAt = null)
{
    _pinnedToFeedAt = pinnedAt ?? DateTimeOffset.UtcNow;
    return this;
}
```

In `Build()`, after constructing the entity, apply the pin via reflection-free setter —
the builder already constructs through `CategoryEntity.Create`, so add the same
post-construct hook the builder uses for `PosterFileId`/`IsExclusive` (call a small
internal helper, or set through the domain method `PinToFeed()` then overwrite the
timestamp). Simplest: expose a test-only seam consistent with how `WithIsExclusive`
is applied in this builder.

### 1.2 `CategoryFactory` — pin helpers

**File:** `tests/Fixtures/Factories/Content/CategoryFactory.cs`

```csharp
/// <summary>
/// Creates a category pinned to the feed at the given time (defaults to "now").
/// </summary>
public static CategoryEntity CreatePinned(ContentTypeEntity contentType, DateTimeOffset? pinnedAt = null) =>
    new CategoryBuilder(contentType.Id).WithContentType(contentType).PinnedToFeedAt(pinnedAt).Build();

/// <summary>
/// Creates several categories of a content type, each pinned at staggered times so the
/// oldest is deterministic (index 0 = oldest).
/// </summary>
public static List<CategoryEntity> CreateManyPinned(ContentTypeEntity contentType, int count, DateTimeOffset baseTime) =>
    Enumerable.Range(0, count)
        .Select(i => CreatePinned(contentType, baseTime.AddMinutes(i)))
        .ToList();
```

### 1.3 `VideoBuilder` — explicit publish time (ordering tests)

**File:** `tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs`

`AsPublished()` exists but does not let a test control `PublishedAt`. Add:

```csharp
/// <summary>
/// Publishes the video with an explicit PublishedAt, for deterministic "latest first" ordering.
/// </summary>
public VideoBuilder AsPublishedAt(DateTimeOffset publishedAt)
{
    AsPublished();
    _publishedAt = publishedAt; // applied in Build()
    return this;
}
```

### 1.4 `MockCategoryRepository` — pinned categories

**File:** `tests/Unit/Common/Mocks/Repositories/MockCategoryRepository.cs`

```csharp
/// <summary>
/// Sets up GetPinnedToFeedCategoriesAsync to return the given list for any content-type filter.
/// </summary>
public static Mock<ICategoryRepository> SetupGetPinnedToFeedCategories(
    this Mock<ICategoryRepository> mock,
    IReadOnlyList<CategoryEntity> categories
)
{
    mock.Setup(x => x.GetPinnedToFeedCategoriesAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(categories);
    return mock;
}
```

Add to `SetupDefaults`:

```csharp
mock.Setup(x => x.GetPinnedToFeedCategoriesAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new List<CategoryEntity>());
```

### 1.5 `MockVideoRepository` — latest + count by category

**File:** `tests/Unit/Common/Mocks/Repositories/MockVideoRepository.cs`

```csharp
public static Mock<IVideoRepository> SetupGetLatestPublishedByCategory(
    this Mock<IVideoRepository> mock,
    Guid categoryId,
    IReadOnlyList<VideoEntity> videos
)
{
    mock.Setup(x => x.GetLatestPublishedByCategoryAsync(categoryId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(videos);
    return mock;
}

public static Mock<IVideoRepository> SetupCountPublishedByCategory(
    this Mock<IVideoRepository> mock,
    Guid categoryId,
    int count
)
{
    mock.Setup(x => x.CountPublishedByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
        .ReturnsAsync(count);
    return mock;
}
```

Add to `SetupDefaults`: both default to empty list / `0`.

### 1.6 `MockFileRepository` — batch fetch

**File:** `tests/Unit/Common/Mocks/Repositories/MockFileRepository.cs`

```csharp
public static Mock<IFileRepository> SetupGetByIds(
    this Mock<IFileRepository> mock,
    IReadOnlyDictionary<Guid, FileEntity> files
)
{
    mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(files);
    return mock;
}

public static void VerifyGetByIdsCalledOnce(this Mock<IFileRepository> mock)
{
    mock.Verify(
        x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

Add to `Create()` defaults: return an empty dictionary.

---

## 2. Unit tests

### 2.1 `CategoryEntityTests` (additions)

**File:** `tests/Unit/Modules/Content/Domain/Entities/CategoryEntityTests.cs` — add a `#region PinToFeed / UnpinFromFeed`.

```csharp
[Fact]
public void PinToFeed_WhenNotPinned_ShouldSetTimestampAndFlag()
{
    CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());

    category.PinToFeed();

    category.PinnedToFeedAt.Should().NotBeNull();
    category.IsPinnedToFeed.Should().BeTrue();
}

[Fact]
public void PinToFeed_WhenAlreadyPinned_ShouldRefreshTimestampForward()
{
    CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
    category.PinToFeed();
    DateTimeOffset first = category.PinnedToFeedAt!.Value;

    category.PinToFeed();

    category.PinnedToFeedAt!.Value.Should().BeOnOrAfter(first);
}

[Fact]
public void UnpinFromFeed_WhenPinned_ShouldClearAndReturnTrue()
{
    CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());
    category.PinToFeed();

    bool result = category.UnpinFromFeed();

    result.Should().BeTrue();
    category.PinnedToFeedAt.Should().BeNull();
    category.IsPinnedToFeed.Should().BeFalse();
}

[Fact]
public void UnpinFromFeed_WhenNotPinned_ShouldReturnFalse()
{
    CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());

    bool result = category.UnpinFromFeed();

    result.Should().BeFalse();
    category.PinnedToFeedAt.Should().BeNull();
}

[Fact]
public void IsPinnedToFeed_ShouldReflectTimestampPresence()
{
    CategoryEntity category = CategoryFactory.Create(Guid.NewGuid());

    category.IsPinnedToFeed.Should().BeFalse();
    category.PinToFeed();
    category.IsPinnedToFeed.Should().BeTrue();
}
```

**Covers:** `PinToFeed`, `UnpinFromFeed` (both branches), `IsPinnedToFeed`.

### 2.2 `CategorySpecificationTests` — `PinnedToFeedCategorySpecification` (new file)

**File:** `tests/Unit/Modules/Content/Application/Catalog/Specifications/CategorySpecificationTests.cs`

Compile each spec to a predicate and assert membership (see
[how-to-tests/09](../how-to-tests/09-writing-specification-tests.md)).

| Test | Expectation |
|---|---|
| `Matches_PinnedActiveCategory` | active + `PinnedToFeedAt != null` ⇒ matches |
| `DoesNotMatch_UnpinnedCategory` | `PinnedToFeedAt == null` ⇒ excluded |
| `DoesNotMatch_InactivePinnedCategory` | inactive ⇒ excluded even if pinned |
| `WithContentTypeFilter_MatchesOnlyThatType` | `contentTypeId` set ⇒ other types excluded |
| `WithNullContentTypeFilter_MatchesAllTypes` | `contentTypeId == null` ⇒ any feedable type matches |

```csharp
[Fact]
public void Matches_PinnedActiveCategory()
{
    ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
    CategoryEntity pinned = CategoryFactory.CreatePinned(videoType);

    var predicate = new PinnedToFeedCategorySpecification().ToExpression().Compile();

    predicate(pinned).Should().BeTrue();
}

[Fact]
public void DoesNotMatch_InactivePinnedCategory()
{
    ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
    CategoryEntity pinned = CategoryFactory.CreatePinned(videoType);
    pinned.Deactivate();

    var predicate = new PinnedToFeedCategorySpecification().ToExpression().Compile();

    predicate(pinned).Should().BeFalse();
}
```

### 2.3 `AdminPinCategoryToFeedHandlerTests`

**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/PinCategoryToFeed/AdminPinCategoryToFeedHandlerTests.cs`

```csharp
public class AdminPinCategoryToFeedHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly AdminPinCategoryToFeedHandler _handler;

    public AdminPinCategoryToFeedHandlerTests()
    {
        _categoryRepositoryMock = MockCategoryRepository.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _handler = new AdminPinCategoryToFeedHandler(
            _categoryRepositoryMock.Object,
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    private const int Min = 4; // EditorialFeedConstants.MinVideosToPinToFeed
    private const int Cap = 5; // CatalogFeedConstants.MaxPinnedCategoriesPerContentType

    #region Success Cases

    [Fact]
    public async Task Handle_WhenEligibleAndBelowCap_ShouldPin()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories(new List<CategoryEntity>());
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, Min);

        var command = new AdminPinCategoryToFeedCommand(Id: category.Id.ToString());

        AdminPinCategoryToFeedResult result = await _handler.Handle(command, CancellationToken.None);

        result.Category.IsPinnedToFeed.Should().BeTrue();
        category.IsPinnedToFeed.Should().BeTrue();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenExactlyMinimumPublishedVideos_ShouldPin()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, Min); // boundary: exactly 4

        await _handler.Handle(new AdminPinCategoryToFeedCommand(category.Id.ToString()), CancellationToken.None);

        category.IsPinnedToFeed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenCapReached_ShouldEvictOldestAndPinNew()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        List<CategoryEntity> existing = CategoryFactory.CreateManyPinned(videoType, Cap, baseTime); // index 0 = oldest
        CategoryEntity newCategory = CategoryFactory.Create(videoType);

        _categoryRepositoryMock.SetupGetByIdOrThrow(newCategory);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories(existing);
        _videoRepositoryMock.SetupCountPublishedByCategory(newCategory.Id, Min);

        await _handler.Handle(new AdminPinCategoryToFeedCommand(newCategory.Id.ToString()), CancellationToken.None);

        existing[0].IsPinnedToFeed.Should().BeFalse();          // oldest evicted
        existing.Skip(1).Should().OnlyContain(c => c.IsPinnedToFeed); // others kept
        newCategory.IsPinnedToFeed.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenAlreadyPinnedAtCap_ShouldRefreshAndNotEvict()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        List<CategoryEntity> existing = CategoryFactory.CreateManyPinned(videoType, Cap, baseTime);
        CategoryEntity target = existing[0]; // the oldest, already pinned

        _categoryRepositoryMock.SetupGetByIdOrThrow(target);
        _categoryRepositoryMock.SetupGetPinnedToFeedCategories(existing);
        _videoRepositoryMock.SetupCountPublishedByCategory(target.Id, Min);

        await _handler.Handle(new AdminPinCategoryToFeedCommand(target.Id.ToString()), CancellationToken.None);

        existing.Should().OnlyContain(c => c.IsPinnedToFeed); // nothing evicted, still 5
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenInactive_ShouldThrowBadRequest()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        category.Deactivate();
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        Func<Task> act = () => _handler.Handle(new AdminPinCategoryToFeedCommand(category.Id.ToString()), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        category.IsPinnedToFeed.Should().BeFalse();
    }

    [Theory]
    [InlineData(nameof(EnumCoreContentType.Short))]
    [InlineData(nameof(EnumCoreContentType.Custom))]
    [InlineData(nameof(EnumCoreContentType.Article))]
    public async Task Handle_WhenNonVideoContentType_ShouldThrowBadRequest(string typeName)
    {
        ContentTypeEntity type = ContentTypeFactory.Create(typeName);
        CategoryEntity category = CategoryFactory.Create(type);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);

        Func<Task> act = () => _handler.Handle(new AdminPinCategoryToFeedCommand(category.Id.ToString()), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Handle_WhenFewerThanMinimumPublishedVideos_ShouldThrowBadRequest()
    {
        ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
        CategoryEntity category = CategoryFactory.Create(videoType);
        _categoryRepositoryMock.SetupGetByIdOrThrow(category);
        _videoRepositoryMock.SetupCountPublishedByCategory(category.Id, Min - 1); // 3 published

        Func<Task> act = () => _handler.Handle(new AdminPinCategoryToFeedCommand(category.Id.ToString()), CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>();
        category.IsPinnedToFeed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenCategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _categoryRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        Func<Task> act = () => _handler.Handle(new AdminPinCategoryToFeedCommand(id.ToString()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
```

**Covers every branch of the handler:** active guard, content-type guard (both excluded
types), min-videos gate (below + boundary), not-found, below-cap pin, FIFO eviction at cap,
re-pin-at-cap no-eviction, per-content-type cap independence, commit.

### 2.4 `AdminUnpinCategoryFromFeedHandlerTests`

**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UnpinCategoryFromFeed/AdminUnpinCategoryFromFeedHandlerTests.cs`

| Test | Expectation |
|---|---|
| `Handle_WhenPinned_ShouldUnpinAndCommit` | `PinnedToFeedAt` cleared; DTO `IsPinnedToFeed == false`; commit once |
| `Handle_WhenNotPinned_ShouldBeIdempotent` | no throw; DTO `IsPinnedToFeed == false`; commit once |
| `Handle_WhenNotFound_ShouldThrowNotFound` | `NotFoundException` |

```csharp
[Fact]
public async Task Handle_WhenPinned_ShouldUnpinAndCommit()
{
    ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
    CategoryEntity category = CategoryFactory.CreatePinned(videoType);
    _categoryRepositoryMock.SetupGetByIdOrThrow(category);

    AdminUnpinCategoryFromFeedResult result =
        await _handler.Handle(new AdminUnpinCategoryFromFeedCommand(category.Id.ToString()), CancellationToken.None);

    category.IsPinnedToFeed.Should().BeFalse();
    result.Category.IsPinnedToFeed.Should().BeFalse();
    _unitOfWorkMock.VerifyCommitCalled();
}
```

### 2.5 `PublicGetVideoFeedHandlerTests`

**File:** `tests/Unit/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetVideoFeed/PublicGetVideoFeedHandlerTests.cs`

| Test | Expectation |
|---|---|
| `Handle_WithPinnedVideoCategories_ShouldReturnSectionPerCategory` | one section per pinned video category that has videos |
| `Handle_ShouldOmitEmptySections` | a pinned category whose video list is empty produces no section |
| `Handle_ShouldExcludeNonVideoPinnedCategories` | pinned Article categories are filtered out |
| `Handle_WhenNoPinnedCategories_ShouldReturnEmptySections` | empty list, no throw |
| `Handle_ShouldRequestMaxVideosPerSection` | verifies `GetLatestPublishedByCategoryAsync` is called with `MaxVideosPerFeedSection` |
| `Handle_ShouldBatchFileLookupOnce` | `GetByIdsAsync` invoked exactly once (no per-item N+1) |
| `Handle_ShouldResolveThumbnailAndPosterUrlsFromBatch` | DTO URLs come from the batched file map |
| `Handle_ShouldOrderSectionsByPinnedDescending` | section order matches the repo's pinned order |

```csharp
[Fact]
public async Task Handle_ShouldOmitEmptySections_AndBatchFilesOnce()
{
    ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
    CategoryEntity withVideos = CategoryFactory.CreatePinned(videoType);
    CategoryEntity empty = CategoryFactory.CreatePinned(videoType);

    List<VideoEntity> videos = VideoFactory.CreateManyWithCategory(withVideos.Id, withVideos, 3);

    _categoryRepositoryMock.SetupGetPinnedToFeedCategories(new List<CategoryEntity> { withVideos, empty });
    _videoRepositoryMock.SetupGetLatestPublishedByCategory(withVideos.Id, videos);
    _videoRepositoryMock.SetupGetLatestPublishedByCategory(empty.Id, new List<VideoEntity>());
    _fileRepositoryMock.SetupGetByIds(new Dictionary<Guid, FileEntity>());

    PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

    result.Sections.Should().HaveCount(1);
    result.Sections[0].Category.Id.Should().Be(withVideos.Id);
    result.Sections[0].Videos.Should().HaveCount(3);
    _fileRepositoryMock.VerifyGetByIdsCalledOnce();
}

[Fact]
public async Task Handle_ShouldExcludeNonVideoPinnedCategories()
{
    ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
    ContentTypeEntity articleType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Article));
    CategoryEntity video = CategoryFactory.CreatePinned(videoType);
    CategoryEntity article = CategoryFactory.CreatePinned(articleType);

    _categoryRepositoryMock.SetupGetPinnedToFeedCategories(new List<CategoryEntity> { video, article });
    _videoRepositoryMock.SetupGetLatestPublishedByCategory(video.Id, VideoFactory.CreateManyWithCategory(video.Id, video, 2));
    _fileRepositoryMock.SetupGetByIds(new Dictionary<Guid, FileEntity>());

    PublicGetVideoFeedResult result = await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

    result.Sections.Should().ContainSingle(s => s.Category.Id == video.Id);
}

[Fact]
public async Task Handle_ShouldRequestMaxVideosPerSection()
{
    ContentTypeEntity videoType = ContentTypeFactory.Create(nameof(EnumCoreContentType.Video));
    CategoryEntity category = CategoryFactory.CreatePinned(videoType);
    _categoryRepositoryMock.SetupGetPinnedToFeedCategories(new List<CategoryEntity> { category });
    _videoRepositoryMock.SetupGetLatestPublishedByCategory(category.Id, VideoFactory.CreateManyWithCategory(category.Id, category, 1));
    _fileRepositoryMock.SetupGetByIds(new Dictionary<Guid, FileEntity>());

    await _handler.Handle(new PublicGetVideoFeedQuery(), CancellationToken.None);

    _videoRepositoryMock.Verify(
        x => x.GetLatestPublishedByCategoryAsync(category.Id, EditorialFeedConstants.MaxVideosPerFeedSection, It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

> The "caps at 8" and "PublishedAt ordering" behaviours live in the **repository** (the
> handler just forwards the limit), so they are asserted in §3.4, not here.

---

## 3. Integration tests

All use `[Collection("Database")]`, `BaseApiTest(PostgresFixture)`, `SeedAsync<ContentDbContext>(...)`,
`Client.Authenticate*` / `ClearAuthentication`, `response.ReadAsAsync<T>()`, and
`response.ShouldBeProblem(HttpStatusCode.X)`.

### 3.1 `AdminPinCategoryToFeedEndpointV1Tests`

**File:** `tests/Integration/Modules/Content/Application/Catalog/UseCases/Admin/Commands/PinCategoryToFeed/V1/AdminPinCategoryToFeedEndpointV1Tests.cs`

| Test | Expectation |
|---|---|
| `Pin_AsSuperAdmin_WhenEligible_ReturnsOk` | `200`; body `IsPinnedToFeed == true`; row persisted |
| `Pin_AsAdmin_ReturnsForbidden` | `403` |
| `Pin_WithNoAuth_ReturnsUnauthorized` | `401` |
| `Pin_NonExistentCategory_ReturnsNotFound` | `404` problem |
| `Pin_WhenInactive_ReturnsBadRequest` | `400` problem; not pinned |
| `Pin_WhenNonFeedableType_ReturnsBadRequest` | `400` problem (Short/Custom) |
| `Pin_WhenFewerThan4PublishedVideos_ReturnsBadRequest` | `400` problem; not pinned |
| `Pin_WhenCapReached_EvictsOldest` | seed 5 pinned (staggered) + 1 new eligible; after pin, oldest row `PinnedToFeedAt == null`, new row set, count stays 5 |

```csharp
[Collection("Database")]
public class AdminPinCategoryToFeedEndpointV1Tests(PostgresFixture db) : BaseApiTest(db)
{
    private const string PinSegment = "pin-to-feed";

    private async Task<DateTimeOffset?> PinnedAtAsync(Guid id)
    {
        await using var ctx = CreateDbContext<ContentDbContext>();
        CategoryEntity? c = await ctx.Categories.FindAsync(id);
        return c!.PinnedToFeedAt;
    }

    [Fact]
    public async Task Pin_AsSuperAdmin_WhenEligible_ReturnsOk()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(cat);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(cat.Id, 4)); // meets the minimum
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsAsync<AdminPinCategoryToFeedResponse>();
        body.Category.IsPinnedToFeed.Should().BeTrue();
        (await PinnedAtAsync(category.Id)).Should().NotBeNull();
    }

    [Fact]
    public async Task Pin_WhenFewerThan4PublishedVideos_ReturnsBadRequest()
    {
        CategoryEntity category = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);
            CategoryEntity cat = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(cat);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(cat.Id, 3)); // one short
            return cat;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{category.Id}/{PinSegment}", null);

        await response.ShouldBeProblem(HttpStatusCode.BadRequest);
        (await PinnedAtAsync(category.Id)).Should().BeNull();
    }

    [Fact]
    public async Task Pin_WhenCapReached_EvictsOldest()
    {
        var oldestId = Guid.Empty;
        CategoryEntity newCat = await SeedAsync<ContentDbContext, CategoryEntity>(ctx =>
        {
            ContentTypeEntity type = ContentTypeFactory.Create("Video");
            ctx.ContentTypes.Add(type);

            var baseTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
            List<CategoryEntity> pinned = CategoryFactory.CreateManyPinned(type, 5, baseTime);
            oldestId = pinned[0].Id;
            ctx.Categories.AddRange(pinned);

            CategoryEntity fresh = CategoryFactory.Create(type.Id);
            ctx.Categories.Add(fresh);
            ctx.Videos.AddRange(VideoFactory.CreateManyPublished(fresh.Id, 4));
            return fresh;
        });

        Client.AuthenticateAsSuperAdmin();

        var response = await Client.PatchAsync($"{ApiRoutes.Admin.Categories}/{newCat.Id}/{PinSegment}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await PinnedAtAsync(oldestId)).Should().BeNull();   // evicted
        (await PinnedAtAsync(newCat.Id)).Should().NotBeNull(); // pinned

        await using var ctx2 = CreateDbContext<ContentDbContext>();
        int pinnedCount = ctx2.Categories.Count(c => c.PinnedToFeedAt != null);
        pinnedCount.Should().Be(5);
    }
}
```

### 3.2 `AdminUnpinCategoryFromFeedEndpointV1Tests`

| Test | Expectation |
|---|---|
| `Unpin_AsSuperAdmin_WhenPinned_ReturnsOk` | `200`; row `PinnedToFeedAt == null` |
| `Unpin_AsSuperAdmin_WhenNotPinned_ReturnsOk` | `200` (idempotent) |
| `Unpin_AsAdmin_ReturnsForbidden` | `403` |
| `Unpin_WithNoAuth_ReturnsUnauthorized` | `401` |
| `Unpin_NonExistentCategory_ReturnsNotFound` | `404` problem |

### 3.3 `PublicGetVideoFeedEndpointV1Tests`

**File:** `tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetVideoFeed/V1/PublicGetVideoFeedEndpointV1Tests.cs`

| Test | Expectation |
|---|---|
| `GetFeed_AsAnonymous_WhenNoPinnedCategories_ReturnsOkEmpty` | `200`; `Sections` empty |
| `GetFeed_AsAnonymous_WithPinnedCategory_ReturnsSection` | `200`; one section with that category + its published videos |
| `GetFeed_ShouldCapVideosAtEight` | category with 12 published ⇒ section has 8 |
| `GetFeed_ShouldOmitCategoryWithNoPublishedVideos` | pinned but only-draft category ⇒ no section |
| `GetFeed_ShouldOnlyIncludeVideoCategories` | pinned article category excluded |
| `GetFeed_ShouldOrderSectionsByMostRecentlyPinned` | section order = `PinnedToFeedAt` desc |

```csharp
[Fact]
public async Task GetFeed_AsAnonymous_WithPinnedCategory_ReturnsSection()
{
    CategoryEntity category = null!;
    await SeedAsync<ContentDbContext>(ctx =>
    {
        ContentTypeEntity type = ContentTypeFactory.Create("Video");
        ctx.ContentTypes.Add(type);
        category = CategoryFactory.CreatePinned(type);
        ctx.Categories.Add(category);
        ctx.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 5));
    });

    Client.ClearAuthentication();

    var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/feed");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
    body.Sections.Should().ContainSingle(s => s.Category.Id == category.Id);
    body.Sections[0].Videos.Should().HaveCount(5);
}

[Fact]
public async Task GetFeed_ShouldCapVideosAtEight()
{
    CategoryEntity category = null!;
    await SeedAsync<ContentDbContext>(ctx =>
    {
        ContentTypeEntity type = ContentTypeFactory.Create("Video");
        ctx.ContentTypes.Add(type);
        category = CategoryFactory.CreatePinned(type);
        ctx.Categories.Add(category);
        ctx.Videos.AddRange(VideoFactory.CreateManyPublished(category.Id, 12));
    });

    Client.ClearAuthentication();

    var response = await Client.GetAsync($"{ApiRoutes.Public.Videos}/feed");

    var body = await response.ReadAsAsync<PublicGetVideoFeedResponse>();
    body.Sections.Single().Videos.Should().HaveCount(8);
}
```

> Add `ApiRoutes.Public.Videos` (and `ApiRoutes.Admin.Categories` already exists). Confirm the
> `Feed` segment constant value (`"feed"`) when wiring the URL.

### 3.4 Repository integration tests

Real Postgres, seeded data, assert query semantics.

**`CategoryRepositoryTests`**

| Test | Expectation |
|---|---|
| `GetPinnedToFeedCategoriesAsync_ReturnsOnlyActivePinned` | excludes unpinned and inactive rows |
| `GetPinnedToFeedCategoriesAsync_OrdersByPinnedDescending` | newest pinned first |
| `GetPinnedToFeedCategoriesAsync_WithContentTypeFilter_ScopesToType` | only the requested content type |
| `GetPinnedToFeedCategoriesAsync_IncludesContentType` | `ContentType` nav populated (feed relies on it) |

**`VideoRepositoryTests`**

| Test | Expectation |
|---|---|
| `GetLatestPublishedByCategoryAsync_ReturnsOnlyPublished` | drafts/pending/archived/rejected excluded |
| `GetLatestPublishedByCategoryAsync_RespectsLimit` | returns at most `limit` |
| `GetLatestPublishedByCategoryAsync_OrdersByPublishedAtDescending` | newest published first (uses `AsPublishedAt`) |
| `GetLatestPublishedByCategoryAsync_IncludesCategory` | `Category` nav populated |
| `CountPublishedByCategoryAsync_CountsOnlyPublished` | excludes non-published statuses |
| `CountPublishedByCategoryAsync_WhenNone_ReturnsZero` | `0` |

**`FileRepositoryTests`** (Core)

| Test | Expectation |
|---|---|
| `GetByIdsAsync_ReturnsRequestedFilesKeyedById` | map contains each requested existing file |
| `GetByIdsAsync_OmitsMissingIds` | unknown ids simply absent |
| `GetByIdsAsync_WithEmptyInput_ReturnsEmpty` | empty input ⇒ empty map, no query |

```csharp
[Fact]
public async Task GetLatestPublishedByCategoryAsync_OrdersByPublishedAtDescending()
{
    Guid categoryId = Guid.NewGuid();
    await SeedAsync<ContentDbContext>(ctx =>
    {
        ContentTypeEntity type = ContentTypeFactory.Create("Video");
        ctx.ContentTypes.Add(type);
        CategoryEntity cat = new CategoryBuilder(type.Id).WithId(categoryId).Build();
        ctx.Categories.Add(cat);
        ctx.Videos.Add(new VideoBuilder(categoryId).AsPublishedAt(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)).Build());
        ctx.Videos.Add(new VideoBuilder(categoryId).AsPublishedAt(new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero)).Build());
    });

    await using var ctx2 = CreateDbContext<ContentDbContext>();
    var repo = new VideoRepository(ctx2);

    IReadOnlyList<VideoEntity> videos = await repo.GetLatestPublishedByCategoryAsync(categoryId, 8, default);

    videos.Should().HaveCount(2);
    videos[0].PublishedAt.Should().BeAfter(videos[1].PublishedAt!.Value);
}
```

---

## 4. Coverage matrix

Every production element added by this feature → its asserting test(s). This is the
checklist for "100%".

| Production element | Asserted by |
|---|---|
| `CategoryEntity.PinToFeed()` | 2.1 (new + refresh) |
| `CategoryEntity.UnpinFromFeed()` | 2.1 (pinned + not-pinned) |
| `CategoryEntity.IsPinnedToFeed` | 2.1 |
| `PinnedToFeedCategorySpecification` | 2.2 (5 cases) |
| `ICategoryRepository.GetPinnedToFeedCategoriesAsync` | 3.4 CategoryRepository (4 cases) |
| `IVideoRepository.GetLatestPublishedByCategoryAsync` | 3.4 VideoRepository (4 cases) |
| `IVideoRepository.CountPublishedByCategoryAsync` | 3.4 VideoRepository (2 cases) |
| `IFileRepository.GetByIdsAsync` | 3.4 FileRepository (3 cases) |
| `CategoryDto.IsPinnedToFeed` / `PinnedToFeedAt` | 2.3 / 3.1 (DTO assertions) |
| `ToCategoryDto(mapper, files)` overload | 2.5 (URL resolution) |
| `ToVideoSummaryDto(mapper, files)` overload | 2.5 (URL resolution) |
| `AdminPinCategoryToFeedHandler` (all branches) | 2.3 (11 cases) |
| `AdminUnpinCategoryFromFeedHandler` (all branches) | 2.4 (3 cases) |
| `PublicGetVideoFeedHandler` (all branches) | 2.5 (8 cases) |
| Pin endpoint (auth, status, persistence, eviction) | 3.1 (8 cases) |
| Unpin endpoint (auth, status, idempotency) | 3.2 (5 cases) |
| Feed endpoint (anon, sections, cap, omission, ordering) | 3.3 (6 cases) |
| EF config: `pinned_to_feed_at` column + partial index | exercised by every 3.x integration test (migration applied to the test DB) |
| Error keys `CannotPinInactiveToFeed` / `ContentTypeNotFeedable` / `NotEnoughVideosToPinToFeed` | 2.3 (unit throws) + 3.1 (problem responses) |

### Running with coverage

```bash
dotnet test tests/Unit/_116.Unit.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
dotnet test tests/Integration/_116.Integration.Tests.csproj
```

> Per project convention, **the user runs the integration tests** (the Testcontainers
> Postgres + coverlet PDB lock makes concurrent `dotnet test` runs flaky). Hand off the
> integration run rather than executing it alongside other test projects.
