# Article Comment Authors — Overview

The public "list article comments" endpoint returns comments that carry only
`Id, UserId, Body?, IsDeleted` (plus auditable fields). There is no way for a client
to render a commenter's **username or avatar**, there is **no threading (replies)**, and
there are **no per-comment likes**. The frontend article-detail page currently designs
around a missing `author` projection and falls back to a neutral avatar derived from the
raw `UserId` (see `apps/frontend/docs/article-detail/12-comments.md` §"Resolving the
author gap").

This folder closes that gap properly, in three clearly separated phases. **Phase 1 is the
must-have** and is specified to implementation-ready detail. Phases 2 and 3 are concrete
forward designs that are not required to ship Phase 1.

---

## Goal

Comments should resolve their author **identically to how articles resolve theirs**. The
article detail DTO already carries `Author (AuthorDto?)`, resolved through the Identity
module's cross-module `IUserLookupService`. Comments must reuse that exact mechanism so a
commenter and an article author render the same way (username, avatar URL, role), with no
new user-lookup path and no direct Content → Identity domain dependency.

---

## The existing author-resolution mechanism (reused verbatim)

| Concern | Type | File |
|---------|------|------|
| Cross-module contract | `IUserLookupService` | `src/Modules/Identity/Identity.Contracts/Application/IUserLookupService.cs` |
| Contract DTO | `AuthorInfo(UserName, Email?, AvatarFileId?, Role?)` | `src/Modules/Identity/Identity.Contracts/Application/AuthorInfo.cs` |
| Implementation | `UserLookupService` | `src/Modules/Identity/Identity/Infrastructure/Services/UserLookupService.cs` |
| DI registration | `services.AddScoped<IUserLookupService, UserLookupService>()` | `src/Modules/Identity/Identity/IdentityModule.cs` (line 140) |
| Consumer DTO | `AuthorDto(UserName, Email?, AvatarUrl?, Role?)` | `src/Modules/Content/Content/Application/Shared/DTOs/AuthorDto.cs` |
| Reference consumer | `AdminGetArticleByIdHandler` | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Queries/GetArticleById/AdminGetArticleByIdHandler.cs` |

The resolution flow, as already implemented for articles, is:

1. `IUserLookupService.GetAuthorInfoByIdAsync(userId)` → `AuthorInfo` (avatar is a **file
   ID**, not a URL).
2. If `AuthorInfo.AvatarFileId` has a value, `IFileRepository.GetByIdAsync(fileId)` →
   `FileEntity.StorageUrl`.
3. Assemble `AuthorDto(UserName, Email, AvatarUrl, Role)`.

Comments reuse steps 1–3 exactly. The only new addition is a **batch** variant of step 1
so a page of comments resolves in one Identity query instead of N (see doc 02).

---

## Phased plan

| Phase | Scope | Status | Doc |
|-------|-------|--------|-----|
| **Phase 1** | Author projection on `ArticleCommentDto` (the must-have) | Implementation-ready | [02-comment-author-projection.md](02-comment-author-projection.md) |
| Phase 2 | Threading / replies (`ParentCommentId`) | Forward design | [03-threading-replies.md](03-threading-replies.md) |
| Phase 3 | Per-comment likes (`LikeCount`, `IsLiked`) | Forward design | [04-comment-likes.md](04-comment-likes.md) |

### Phase 1 — Author projection (must-have)

- Add `Author (AuthorDto?)` to `ArticleCommentDto`.
- Resolve it in `PublicGetArticleCommentsHandler` by reusing `IUserLookupService` +
  `IFileRepository`, exactly like `AdminGetArticleByIdHandler`.
- **Batch-resolve** all distinct commenter `UserId`s in one call to avoid N+1. Add a
  `GetAuthorInfosByIdsAsync` method to `IUserLookupService`.
- Deleted comments: `Body` stays null and **no author is leaked** — a soft-deleted
  comment resolves to `Author = null`.

Phase 1 requires **no database migration** — the author is resolved at read time, never
stored on the comment row.

### Phase 2 — Threading / replies (forward design)

- Add `ParentCommentId (Guid?)` to `ArticleCommentEntity`, its EF configuration, and
  `ArticleCommentDto`.
- Query shape: top-level comments paged; replies embedded (bounded) or lazily fetched via
  a dedicated replies endpoint.
- Requires an EF migration adding the nullable self-referencing FK column.

### Phase 3 — Per-comment likes (forward design)

- Add an `ArticleCommentLikeEntity` join table mirroring the existing article-like pattern.
- Add `LikeCount (int)` and `IsLiked (bool)` to `ArticleCommentDto`.
- Add like / unlike comment endpoints.
- Requires an EF migration adding the join table (and optionally a cached count column).

---

## Documents in this folder

| Doc | Content |
|-----|---------|
| [00-overview.md](00-overview.md) | This document |
| [01-current-state.md](01-current-state.md) | Exactly what exists today, with file paths |
| [02-comment-author-projection.md](02-comment-author-projection.md) | Phase 1 design — author projection + batch resolve |
| [03-threading-replies.md](03-threading-replies.md) | Phase 2 design — replies |
| [04-comment-likes.md](04-comment-likes.md) | Phase 3 design — per-comment likes |
| [05-frontend-integration.md](05-frontend-integration.md) | How `apps/frontend` consumes each phase |
| [06-testing.md](06-testing.md) | Unit + integration test plan |
| [07-migrations-and-rollout.md](07-migrations-and-rollout.md) | Migrations, backfill, rollout order, open questions |
| [specs/00-index.md](specs/00-index.md) | Implementation-ready specs index |

---

## Key decisions

1. **Resolve-at-read, not denormalized.** The author is resolved from the Identity module
   at read time, never copied onto the comment row. This matches articles, keeps a single
   source of truth for username/avatar/role, and means Phase 1 needs no migration and no
   backfill. Trade-off and rejected alternative in [07-migrations-and-rollout.md](07-migrations-and-rollout.md).
2. **Batch resolution.** A comments page can contain many distinct commenters. Resolving
   each individually is an N+1 across a module boundary. Phase 1 adds a batch lookup to
   `IUserLookupService`.
3. **Never expose commenter email publicly.** `AuthorInfo.Email` flows into `AuthorDto` for
   admin article reads, but the **public** comments endpoint must project `Email = null`.
   See [02-comment-author-projection.md](02-comment-author-projection.md) §"Privacy".
4. **Deleted comments leak nothing.** A soft-deleted comment already nulls its `Body`; it
   must also resolve `Author = null`.
5. **Phases are independent.** Phase 1 ships alone and unblocks the frontend. Phases 2–3
   layer on without reworking Phase 1.
