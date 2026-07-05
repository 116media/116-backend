# Phase 2 — Threading / Replies (forward design)

Not required for Phase 1. This is a concrete design so the Phase 1 DTO and query shape do
not paint threading into a corner. Implementation-ready C# is in
[specs/02-threading.md](specs/02-threading.md).

---

## Goal

Allow a comment to be a **reply** to another comment: a single level of nesting
(comment → replies), which is what the article-detail UI needs. Deeper trees are avoided
on purpose — they complicate pagination and add little product value for article comments.

---

## 1. Entity change — `ArticleCommentEntity`

Add a nullable self-reference:

```csharp
/// <summary>
/// The parent comment this comment replies to, or null for a top-level comment.
/// </summary>
public Guid? ParentCommentId { get; private set; }

/// <summary>
/// Navigation to the parent comment, or null for a top-level comment.
/// </summary>
public ArticleCommentEntity? ParentComment { get; private set; }
```

Add a reply factory so intent is explicit and a reply is validated to target a comment on
the same article:

```csharp
public static ArticleCommentEntity CreateReply(
    Guid id,
    Guid userId,
    Guid articleId,
    Guid parentCommentId,
    string body
)
{
    return new ArticleCommentEntity
    {
        Id = id,
        UserId = userId,
        ArticleId = articleId,
        ParentCommentId = parentCommentId,
        Body = body,
        IsDeleted = false,
    };
}
```

Single-level rule: a reply's parent must itself be top-level (`ParentCommentId == null`).
Enforced in the reply command handler, not the entity, since it needs the parent loaded.

---

## 2. EF configuration

`ArticleCommentConfiguration`:

```csharp
builder.Property(x => x.ParentCommentId).IsRequired(false);

builder
    .HasOne(x => x.ParentComment)
    .WithMany()
    .HasForeignKey(x => x.ParentCommentId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasIndex(x => x.ParentCommentId).HasDatabaseName("ix_article_comments_parent");
```

`Restrict` (not cascade) on the self-FK avoids multiple-cascade-path errors in PostgreSQL
and keeps reply deletion an explicit soft-delete concern.

---

## 3. Migration

A single Content-module migration adds `parent_comment_id uuid NULL` plus the index and FK.
See [07-migrations-and-rollout.md](07-migrations-and-rollout.md). Existing rows get
`parent_comment_id = NULL` (all become top-level), which is correct.

---

## 4. DTO change

```csharp
public record ArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    AuthorDto? Author = null,
    Guid? ParentCommentId = null,
    int ReplyCount = 0,
    IReadOnlyList<ArticleCommentDto>? Replies = null
) : AuditableDto;
```

- `ParentCommentId` — null for top-level.
- `ReplyCount` — number of non-deleted direct replies (lets the UI show "3 replies" without
  fetching them).
- `Replies` — optionally embedded (see §5).

---

## 5. Query shape

Two supported shapes; recommend **(a)** for the article-detail page.

**(a) Top-level paged, replies lazy.**
`GET /api/v1/public/articles/{id}/comments` returns only top-level comments
(`ParentCommentId == null`), each with `ReplyCount`. A new endpoint fetches a comment's
replies, paged:

```
GET /api/v1/public/articles/comments/{commentId}/replies?pageIndex=0&pageSize=10
```

Repository: extend `GetCommentsAsync` to filter `ParentCommentId == null` for the
top-level list, and add `GetRepliesAsync(parentCommentId, page, pageSize, ct)`. Both reuse
the Phase 1 batch author projection.

**(b) Top-level paged, replies embedded (bounded).**
The top-level query eager-loads the first *k* (e.g. 3) replies per comment into `Replies`,
with `ReplyCount` signalling whether a "view more replies" call is needed. Heavier query,
fewer round-trips.

Author resolution: whichever shape, collect commenter `UserId`s across **top-level and
embedded replies** into the single Phase 1 batch lookup.

---

## 6. Endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| `GET` | `/api/v1/public/articles/{id:guid}/comments` | Top-level comments (paged) — filter `ParentCommentId == null` |
| `GET` | `/api/v1/public/articles/comments/{commentId:guid}/replies` | Replies to a comment (paged) |
| `POST` | `/api/v1/public/articles/{id:guid}/comments/{commentId:guid}/replies` | Add a reply (auth) |

Add-reply command mirrors `PublicAddArticleCommentCommand` with a `ParentCommentId`, uses
`CreateReply`, and increments the article's comment count as today (replies still count).

---

## 7. Deletion semantics

Soft-deleting a top-level comment that has replies keeps the row (so the thread structure
survives) but nulls the body and author, exactly like Phase 1. Replies remain visible under
the "comment removed" placeholder. This is why the self-FK is `Restrict`, not `Cascade`.

---

## Tasks

- [ ] Add `ParentCommentId` + `ParentComment` nav + `CreateReply` to `ArticleCommentEntity`.
- [ ] Configure the self-FK, index, and nullable column in `ArticleCommentConfiguration`.
- [ ] Create the Content migration adding `parent_comment_id`.
- [ ] Add `ParentCommentId`, `ReplyCount`, `Replies` to `ArticleCommentDto`.
- [ ] Filter top-level in `GetCommentsAsync`; add `GetRepliesAsync`.
- [ ] Add the replies query slice (query/handler/endpoint) reusing the Phase 1 batch author projection.
- [ ] Add the add-reply command slice (command/handler/validator/endpoint).
- [ ] Tests: single-level enforcement, reply pagination, deleted-parent-with-replies.
