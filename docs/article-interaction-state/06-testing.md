# Testing

Test plan for both phases, following the existing conventions in `docs/how-to-tests/`. The
executable specs (exact test methods + `## Tasks` checklist) are in
[specs/03-tests.md](specs/03-tests.md); this document is the plan and rationale.

> The user runs `dotnet test` themselves. Do not run the suite as part of this work.

---

## Conventions to mirror

From the existing article read tests:

- **Unit tests** live under `tests/Unit/Modules/Content/Application/...`, mirror the handler
  namespace, and extend `BaseContentHandlerTest` (provides the configured Mapster `IMapper`).
- **Mocking:** Moq, via the `MockArticleRepository` / `MockFileRepository` helpers under
  `tests/Unit/Common/Mocks/Repositories/`.
- **Assertions:** AwesomeAssertions (`result.Should()...`).
- **Factories:** `ArticleFactory` (`CreatePublished`, `CreateManyPublished`) under
  `tests/Fixtures/Factories/Content/`; `FileFactory.CreateImage()`; `UserFactory` under
  `tests/Fixtures/Factories/Identity/`.
- **Naming:** `Handle_WhenCondition_ShouldExpectedBehavior`.
- **Integration tests** live under `tests/Integration/Modules/Content/...`, carry
  `[Collection("Database")]`, extend `BaseApiTest`, seed via `SeedAsync<TDbContext, TEntity>`,
  authenticate via `HttpClientExtensions` (`AuthenticateAsVisitor()`, `AuthenticateAs(userId, role)`,
  `ClearAuthentication()`), and assert on the deserialized response.

---

## Mock updates

`MockArticleRepository` (`tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`)
already has `SetupHasLikedAsync` / `SetupHasBookmarkedAsync` (the write handlers use them).
Add the Phase 2 batch setups:

```csharp
mock.SetupGetLikedArticleIds(HashSet<Guid> ids);
mock.SetupGetBookmarkedArticleIds(HashSet<Guid> ids);
```

---

## Phase 1 — unit tests (get-by-slug handler)

Target: `PublicGetArticleBySlugHandlerTests` under
`tests/Unit/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetArticleBySlug/`.

| Test | Arrange | Assert |
|------|---------|--------|
| Anonymous → both flags false | `CurrentUserId = null` | `IsLiked == false`, `IsBookmarked == false`; `HasLikedAsync` / `HasBookmarkedAsync` **never called** |
| Liked + bookmarked user | `CurrentUserId = userId`, `SetupHasLikedAsync(true)`, `SetupHasBookmarkedAsync(true)` | `IsLiked == true`, `IsBookmarked == true` |
| Liked but not bookmarked | `SetupHasLikedAsync(true)`, `SetupHasBookmarkedAsync(false)` | `IsLiked == true`, `IsBookmarked == false` |
| Neither | both setups `false` | both flags `false`; both checks **were** called |

The "anonymous → never called" assertion is important: it proves anonymous traffic pays no
extra query cost.

---

## Phase 2 — unit tests

### Repository batch methods

Cover `GetLikedArticleIdsAsync` / `GetBookmarkedArticleIdsAsync` correctness (against an
in-memory / real provider per existing repo-test convention):

| Test | Assert |
|------|--------|
| Returns only ids the user interacted with | result ⊆ input, excludes non-liked ids |
| Ignores other users' rows | another user's like for the same article is absent |
| Empty input → empty set, no query blow-up | returns empty `HashSet` |
| Distinct | no duplicate ids in the result |

### Feed handlers

Target `PublicGetPublishedArticlesHandlerTests`, plus promoted and promotion-feed handler
tests.

| Test | Arrange | Assert |
|------|---------|--------|
| Anonymous → all flags false | `CurrentUserId = null` | every summary `IsLiked == false && IsBookmarked == false`; batch methods **never called** |
| Batch stamps the right items | 3 articles; `SetupGetLikedArticleIds({a1})`, `SetupGetBookmarkedArticleIds({a2})` | only `a1.IsLiked`; only `a2.IsBookmarked`; `a3` both false |
| Single batch call | authenticated feed of N | `GetLikedArticleIds` / `GetBookmarkedArticleIds` each called **exactly once** (no N+1) |
| Promotion feed one batch across sub-collections | spots + gossip strip | each batch method called **once** total |

The "exactly once" verification is the regression guard against a future refactor
reintroducing per-item existence checks.

---

## Integration tests

### Phase 1 — get-by-slug

Under `tests/Integration/Modules/Content/Application/Editorial/UseCases/Public/Queries/GetArticleBySlug/V1/`:

1. **Seed** a category, a published article, and a visitor user. Seed an `ArticleLikeEntity`
   and `ArticleBookmarkEntity` for `(user, article)`.
2. **Authenticated + liked/bookmarked:** `Client.AuthenticateAs(userId, "Visitor")`, GET the
   slug → `body.Article.IsLiked == true`, `body.Article.IsBookmarked == true`.
3. **Authenticated, no interaction:** a second seeded user with no like/bookmark → both flags
   `false`.
4. **Anonymous:** `Client.ClearAuthentication()`, GET the slug → both flags `false`.
5. **Cross-user isolation:** user A liked the article; request as user B → both flags `false`
   (A's state must not leak to B).

### Phase 2 — feed

Under the published-articles / promoted / promotion-feed integration folders:

1. Seed several published articles; seed likes/bookmarks for the current user on a **subset**.
2. **Authenticated:** the flags are set on exactly the subset the user interacted with, false
   elsewhere.
3. **Anonymous:** every item's flags are `false`.
4. **No cross-user cache leak** (guards the doc-07 concern): request the same feed as user A
   (who liked item X), then as user B (who did not). Assert B's response has `X.IsLiked == false`.
   If/when caching is added, this test must still pass — it is the caching-correctness gate.

---

## What this proves

- Anonymous requests never trigger interaction queries and always return `false`.
- Authenticated requests return the exact per-user state.
- The feed uses one batch query per interaction type, not N.
- One user's interaction state is never observable by another user, cached or not.
