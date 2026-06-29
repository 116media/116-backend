# Testing

Mirrors the existing comment-slice tests. The user runs `dotnet test` themselves — do not
run the suite here. Full test bodies are in [specs/04-tests.md](specs/04-tests.md); this
document is the plan and the coverage rationale.

---

## Phase 1 — unit tests

Extend `tests/Unit/Modules/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/PublicGetArticleCommentsHandlerTests.cs`
(currently extends `BaseContentHandlerTest`, mocks `IArticleRepository` via
`MockArticleRepository`, uses `ArticleCommentFactory`). The handler now also depends on
`IUserLookupService` and `IFileRepository`, so add mocks for those.

| Test | Asserts |
|------|---------|
| `Handle_WhenCommentHasResolvableUser_MapsAuthor` | Author is populated with the resolved `userName`, `avatarUrl`, `role`; batch lookup returns one entry |
| `Handle_ResolvesAllCommentersInOneBatchCall` | `GetAuthorInfosByIdsAsync` is invoked **exactly once** with the page's distinct user IDs (no N+1) |
| `Handle_WhenUserNotResolvable_LeavesAuthorNull` | A commenter absent from the lookup dictionary → `Author == null`, comment still returned |
| `Handle_WhenCommentDeleted_AuthorAndBodyAreNull` | Deleted comment → `Body == null` **and** `Author == null`; its `UserId` is **not** passed to the lookup |
| `Handle_DoesNotExposeEmail` | Resolved `AuthorDto.Email == null` even though `AuthorInfo.Email` was non-null |
| `Handle_WhenNoComments_ReturnsEmptyPage` | Existing behavior preserved; lookup is skipped or called with an empty set |

Mock guidance:
- `MockUserLookupService.SetupGetAuthorInfosByIdsAsync(dictionary)` — add alongside the
  existing repository mocks (or a `Mock<IUserLookupService>` set up inline).
- `MockFileRepository` returns a `FileEntity` with a known `StorageUrl` for the avatar file
  ID, and nothing for missing IDs.
- Use `ArticleCommentFactory.Create(articleId, userId)` and a deleted variant
  (`SoftDelete()`), matching the existing test's construction.

---

## Phase 1 — integration tests

Extend `tests/Integration/Modules/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/V1/PublicGetArticleCommentsEndpointV1Tests.cs`
(`[Collection("Database")]`, `BaseApiTest`). The existing `SeedArticleWithCommentAsync`
seeds `ContentType → Category → Article → ArticleComment` for `TestUser.VisitorId`.
`BaseApiTest` already seeds `TestUser.VisitorId` as an Identity `UserEntity`
(`BaseApiTest.cs` line 139), so the commenter is resolvable without extra Identity setup —
lean on that exactly as the existing tests do.

| Test | Asserts |
|------|---------|
| `GetArticleComments_WithSeededComment_ReturnsAuthor` | The returned comment's `author.userName` matches the seeded visitor user; `author` is non-null |
| `GetArticleComments_AuthorEmail_IsNotExposed` | `author.email` is null on the public response |
| `GetArticleComments_DeletedComment_HasNoAuthor` | Seed a soft-deleted comment → `body == null` and `author == null` |
| `GetArticleComments_WithAvatar_ResolvesAvatarUrl` | Seed a `FileEntity` avatar and set the user's `AvatarFileId` → `author.avatarUrl` equals the file's `StorageUrl` |
| `GetArticleComments_WithMultipleCommenters_ReturnsEachAuthor` | Seed a second Identity user + a second comment → both authors resolve; still one page |

Seeding note: to exercise a distinct second commenter, seed an extra `UserEntity` via
`SeedAsync<IdentityDbContext, UserEntity>(...)` (the same `SeedAsync` helper the existing
test uses for Content), then a comment authored by that user. For the avatar test, seed a
`FileEntity` in the Core context and set the user's `AvatarFileId` to it. This mirrors how
integration tests already cross-seed Identity users and Content articles.

---

## Phase 2 — tests (forward)

- **Unit**: single-level enforcement (replying to a reply is rejected); reply pagination;
  a deleted parent with live replies keeps replies visible; the batch author projection
  covers both top-level and reply commenters.
- **Integration**: `/replies` endpoint returns paged replies with authors; posting a reply
  increments the article comment count; top-level list excludes replies.

## Phase 3 — tests (forward)

- **Unit**: like is idempotent (second like is a no-op, count unchanged); unlike decrements
  and never goes below zero; `IsLiked` reflects the viewer; anonymous → all `false`.
- **Integration**: like then unlike round-trips `likeCount`/`isLiked`; a second user's like
  does not set the first user's `isLiked`.

---

## Running (by the user)

```bash
# unit
dotnet test tests/Unit

# integration (Postgres fixture)
dotnet test tests/Integration
```

Do not run these from the agent — the user runs the suite themselves.
