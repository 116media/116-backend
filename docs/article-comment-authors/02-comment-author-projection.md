# Phase 1 — Comment Author Projection (must-have)

Add a resolved `Author` to each public comment, reusing the exact mechanism that resolves
`ArticleDetailDto.Author`, and batch it so a page of comments costs **one** Identity query,
not N. No database migration.

Full, implementation-ready C# is in [specs/01-dto-and-handler.md](specs/01-dto-and-handler.md).
This document is the design and rationale.

---

## 1. DTO change

Add `Author (AuthorDto?)` to `ArticleCommentDto` as a trailing optional parameter (records
in this codebase append optional projected fields last, e.g. `ArticleDetailDto.Author`):

```csharp
public record ArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    AuthorDto? Author = null
) : AuditableDto;
```

`Author` is null when:
- the commenter cannot be resolved (user deleted / not found), or
- the comment is soft-deleted (`IsDeleted == true`) — see §5.

`AuthorDto` is reused unchanged from
`src/Modules/Content/Content/Application/Shared/DTOs/AuthorDto.cs`.

---

## 2. Batch lookup on the resolver

`IUserLookupService` today only resolves **one** user at a time
(`GetAuthorInfoByIdAsync`). A comments page holds many distinct commenters, so per-comment
resolution is an N+1 across the Content → Identity boundary. Add a batch method to the
contract and implement it once:

**Contract** — `src/Modules/Identity/Identity.Contracts/Application/IUserLookupService.cs`:

```csharp
/// <summary>
/// Resolves author profiles for a set of user IDs in a single query.
/// </summary>
/// <param name="userIds">
/// The distinct identity user UUIDs to look up. Duplicates and unknown IDs are ignored.
/// </param>
/// <param name="ct">
/// Cancellation token.
/// </param>
/// <returns>
/// A dictionary keyed by user ID containing the resolved author info. IDs that do not
/// match a user are absent from the result.
/// </returns>
Task<IReadOnlyDictionary<Guid, AuthorInfo>> GetAuthorInfosByIdsAsync(
    IReadOnlyCollection<Guid> userIds,
    CancellationToken ct = default
);
```

**Implementation** — `src/Modules/Identity/Identity/Infrastructure/Services/UserLookupService.cs`
loads all matching users in one `WHERE id IN (...)` query with `UserRoles.ThenInclude(Role)`
and projects each to `AuthorInfo`, mirroring the single-user method exactly.

The single `GetAuthorInfoByIdAsync` stays; `AdminGetArticleByIdHandler` and the other
single-author consumers are untouched.

---

## 3. Avatar URL resolution (batched)

`AuthorInfo` carries `AvatarFileId (Guid?)`, not a URL. Articles resolve it per author via
`IFileRepository.GetByIdAsync`. For a page of comments, collect the distinct non-null
`AvatarFileId`s across all resolved authors and resolve them together.

`IFileRepository` should expose a batch read so avatars also resolve in one query. If a
batch method does not already exist, add:

```csharp
/// <summary>
/// Returns the storage URLs for the given file IDs, keyed by file ID.
/// Missing or soft-deleted files are absent from the result.
/// </summary>
Task<IReadOnlyDictionary<Guid, string>> GetStorageUrlsByIdsAsync(
    IReadOnlyCollection<Guid> fileIds,
    CancellationToken ct = default
);
```

> If adding a batch method to `IFileRepository` is out of scope for the first cut, the
> handler may loop `GetByIdAsync` over the (already de-duplicated) avatar file IDs. Avatars
> per page are few and often shared, so the N is small — but the batch method is the
> preferred, N+1-free shape and is what the specs implement.

---

## 4. Handler change — `PublicGetArticleCommentsHandler`

Inject `IUserLookupService` and `IFileRepository` alongside the existing
`IArticleRepository` and `IMapper` (same set `AdminGetArticleByIdHandler` already uses).
Flow:

1. `GetCommentsAsync(...)` — unchanged; returns the page of `ArticleCommentEntity`.
2. Collect distinct `UserId`s **from non-deleted comments only** (deleted comments leak no
   author).
3. `GetAuthorInfosByIdsAsync(userIds)` → `IReadOnlyDictionary<Guid, AuthorInfo>`.
4. Collect distinct non-null `AvatarFileId`s from those `AuthorInfo`s and resolve them to
   URLs (batch, §3).
5. Build an `AuthorDto` per resolved user: `new AuthorDto(info.UserName, Email: null,
   avatarUrl, info.Role)` — **email is intentionally dropped** (§6 Privacy).
6. Map each comment to `ArticleCommentDto` and attach `Author` (null for deleted comments
   and for commenters that did not resolve).
7. Wrap in `PaginatedResult` exactly as today.

The mapping is expressed as a new mapper overload that accepts the resolved author lookup:
`ToArticleCommentDtos(mapper, authorsByUserId)` (see specs). The existing
author-less `ToArticleCommentDtos(mapper)` stays for the add/edit command handlers, which
return a single freshly-created comment where the author is the caller.

---

## 5. Deleted comments

A soft-deleted comment already nulls `Body` in the mapper. It must also carry
`Author = null`. Two reinforcing safeguards:

- **Exclude deleted comments from the author lookup** — step 2 collects `UserId`s only from
  `!c.IsDeleted` comments, so a deleted comment never contributes to the batch query.
- **Attach `Author` only when `!IsDeleted`** — the mapper sets `Author = null` whenever
  `IsDeleted`, independent of what the lookup contains.

This guarantees a removed comment reveals neither body nor identity, matching the
frontend's "comment removed" placeholder.

---

## 6. Privacy

`AuthorInfo.Email` is populated by the Identity resolver and flows into `AuthorDto` for
**admin** article reads. The **public** comments endpoint must never expose a commenter's
email: the handler constructs `AuthorDto` with `Email: null`. This is asserted by an
integration test (see [06-testing.md](06-testing.md)).

Role is retained (`AuthorDto.Role`) because it is non-sensitive and lets the UI badge
staff/author comments; drop it too if product decides it is not needed publicly.

---

## 7. Endpoint

`GET /api/v1/public/articles/{id:guid}/comments` is unchanged in shape — same route, same
`PaginatedResult<ArticleCommentDto>` envelope. Only the item shape gains a nullable
`author` object, which is additive and backward-compatible for existing clients.

`V1/PublicGetArticleCommentsEndpointV1.cs` needs no code change beyond the DTO it already
produces; regenerate the OpenAPI client so the new field appears (see
[05-frontend-integration.md](05-frontend-integration.md)).

---

## 8. What does NOT change in Phase 1

- No entity change to `ArticleCommentEntity`.
- No EF configuration change, no migration, no backfill.
- No change to add/edit/delete comment handlers.
- No change to the repository's `GetCommentsAsync` signature.

---

## Tasks

- [ ] Add `GetAuthorInfosByIdsAsync` to `IUserLookupService` (contract + XML docs).
- [ ] Implement `GetAuthorInfosByIdsAsync` in `UserLookupService` (single `IN` query with roles).
- [ ] Add batch avatar resolution to `IFileRepository` (`GetStorageUrlsByIdsAsync`) or document the de-duplicated loop fallback.
- [ ] Add `AuthorDto? Author = null` to `ArticleCommentDto`.
- [ ] Add `ToArticleCommentDtos(mapper, authorsByUserId)` overload to `ArticleMapper`.
- [ ] Inject `IUserLookupService` + `IFileRepository` into `PublicGetArticleCommentsHandler` and resolve authors (batched, deleted-safe, email-stripped).
- [ ] Regenerate the frontend OpenAPI client (see doc 05).
