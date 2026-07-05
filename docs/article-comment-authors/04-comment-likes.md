# Phase 3 — Per-Comment Likes (forward design)

Not required for Phase 1. A concrete design that mirrors the existing article-like pattern
(`ArticleLikeEntity`). Implementation-ready C# is in
[specs/03-comment-likes.md](specs/03-comment-likes.md).

---

## Goal

Let users like/unlike an individual comment, expose `LikeCount` and a per-viewer `IsLiked`
on `ArticleCommentDto`, and add like/unlike endpoints — following the same join-table shape
already used for article likes.

---

## 1. Join entity — `ArticleCommentLikeEntity`

Mirror `ArticleLikeEntity`
(`src/Modules/Content/Content/Domain/Entities/ArticleLikeEntity.cs`) — a row exists iff the
user likes the comment; created on like, removed on unlike, never updated.

```csharp
/// <summary>
/// Records that a user has liked an article comment.
/// Created when a user likes; removed when a user unlikes. Never updated.
/// </summary>
public class ArticleCommentLikeEntity : Aggregate<Guid>
{
    /// <summary>
    /// The identity user UUID of the user who liked the comment. No FK to identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// The comment that was liked.
    /// </summary>
    public Guid CommentId { get; private set; }

    /// <summary>
    /// Navigation property to the comment.
    /// </summary>
    public ArticleCommentEntity Comment { get; private set; } = null!;

    private ArticleCommentLikeEntity() { }

    public static ArticleCommentLikeEntity Create(Guid id, Guid userId, Guid commentId)
    {
        return new ArticleCommentLikeEntity
        {
            Id = id,
            UserId = userId,
            CommentId = commentId,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
```

### Cached count on the comment

Add a cached `LikeCount` to `ArticleCommentEntity` (mirrors `ArticleEntity.LikeCount`),
adjusted by `IncrementLikeCount()` / `DecrementLikeCount()`:

```csharp
public int LikeCount { get; private set; }

public void IncrementLikeCount() => LikeCount++;

public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);
```

The cached column avoids a `COUNT` per comment per page; the join table remains the source
of truth for `IsLiked` and for reconciliation.

---

## 2. EF configuration

New `ArticleCommentLikeConfiguration`:

```csharp
builder.HasKey(x => x.Id);
builder.Property(x => x.UserId).IsRequired();
builder.Property(x => x.CommentId).IsRequired();
builder.HasOne(x => x.Comment).WithMany().HasForeignKey(x => x.CommentId).OnDelete(DeleteBehavior.Cascade);
builder.HasIndex(x => new { x.CommentId, x.UserId }).IsUnique().HasDatabaseName("ix_article_comment_likes_comment_user");
```

Unique `(CommentId, UserId)` enforces one like per user per comment. Add
`LikeCount` (default 0) to `ArticleCommentConfiguration`.

Add `DbSet<ArticleCommentLikeEntity> ArticleCommentLikes` to `ContentDbContext`.

---

## 3. Migration

One Content-module migration: create `article_comment_likes` and add `like_count int NOT
NULL DEFAULT 0` to `article_comments`. Existing comments start at 0 (no backfill needed
until likes exist). See [07-migrations-and-rollout.md](07-migrations-and-rollout.md).

---

## 4. DTO change

```csharp
public record ArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    AuthorDto? Author = null,
    // Phase 2 fields (ParentCommentId, ReplyCount, Replies) elided here for brevity
    int LikeCount = 0,
    bool IsLiked = false
) : AuditableDto;
```

- `LikeCount` — from the cached column.
- `IsLiked` — whether the **current viewer** liked the comment; `false` for anonymous.

### Resolving `IsLiked` without N+1

The list query, given the current `UserId` (null for anonymous), collects the page's
comment IDs and issues one query:
`SELECT comment_id FROM article_comment_likes WHERE user_id = @me AND comment_id IN (...)`.
The resulting set marks `IsLiked` per comment. This mirrors how the article read path
resolves the viewer's like state.

The comments endpoint stays `AllowAnonymous`; when unauthenticated, skip the `IsLiked`
query and leave all `false`.

---

## 5. Endpoints

| Method | Route | Purpose |
|--------|-------|---------|
| `POST` | `/api/v1/public/articles/comments/{commentId:guid}/like` | Like a comment (auth) — idempotent |
| `DELETE` | `/api/v1/public/articles/comments/{commentId:guid}/like` | Unlike a comment (auth) — idempotent |

Command handlers mirror the article like/unlike handlers: on like, insert a join row (guard
the unique constraint for idempotency) and `IncrementLikeCount`; on unlike, remove the row
and `DecrementLikeCount`; commit via the Content unit of work.

---

## Tasks

- [ ] Add `ArticleCommentLikeEntity` (mirror `ArticleLikeEntity`).
- [ ] Add cached `LikeCount` + increment/decrement to `ArticleCommentEntity`.
- [ ] Add `ArticleCommentLikeConfiguration` + unique index; add `LikeCount` to comment config; add `DbSet`.
- [ ] Create the Content migration (join table + `like_count`).
- [ ] Add `LikeCount` + `IsLiked` to `ArticleCommentDto`; resolve `IsLiked` batched by viewer.
- [ ] Thread the viewer `UserId` into `PublicGetArticleCommentsQuery`/handler.
- [ ] Add like / unlike comment command slices (idempotent, count adjust).
- [ ] Tests: like/unlike idempotency, `IsLiked` per viewer, anonymous → all false.
