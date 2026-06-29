# Spec 03 — Phase 3: Per-Comment Likes

Forward design, concrete. Mirrors the existing `ArticleLikeEntity` join pattern. Depends on
Phase 1 (author projection). Independent of Phase 2.

---

## 3.1 `ArticleCommentLikeEntity`

**File (new):** `src/Modules/Content/Content/Domain/Entities/ArticleCommentLikeEntity.cs`

```csharp
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

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

    /// <summary>
    /// Creates a new comment like record.
    /// </summary>
    /// <param name="id">The unique identifier for this like.</param>
    /// <param name="userId">The user who liked the comment.</param>
    /// <param name="commentId">The comment that was liked.</param>
    /// <returns>A new <see cref="ArticleCommentLikeEntity" />.</returns>
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

---

## 3.2 `ArticleCommentEntity` — cached count

**File:** `src/Modules/Content/Content/Domain/Entities/ArticleCommentEntity.cs`

**Add:**

```csharp
/// <summary>
/// Cached number of likes on this comment. Adjusted by like/unlike interactions.
/// </summary>
public int LikeCount { get; private set; }

/// <summary>
/// Increments the cached like count.
/// </summary>
public void IncrementLikeCount() => LikeCount++;

/// <summary>
/// Decrements the cached like count, never below zero.
/// </summary>
public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);
```

---

## 3.3 EF configuration

**File (new):** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleCommentLikeConfiguration.cs`

```csharp
using _116.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _116.Content.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for <see cref="ArticleCommentLikeEntity" />.
/// </summary>
public class ArticleCommentLikeConfiguration : IEntityTypeConfiguration<ArticleCommentLikeEntity>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ArticleCommentLikeEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.CommentId).IsRequired();

        builder
            .HasOne(x => x.Comment)
            .WithMany()
            .HasForeignKey(x => x.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new { x.CommentId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_article_comment_likes_comment_user");
    }
}
```

**File:** `ArticleCommentConfiguration.cs` — add:

```csharp
builder.Property(x => x.LikeCount).HasDefaultValue(0).IsRequired();
```

**File:** `ContentDbContext` — add:

```csharp
/// <summary>
/// Likes placed on individual article comments.
/// </summary>
public DbSet<ArticleCommentLikeEntity> ArticleCommentLikes { get; set; }
```

---

## 3.4 Migration

```bash
dotnet ef migrations add AddArticleCommentLikes \
  --project src/Modules/Content/Content.Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

Creates `article_comment_likes` (+ unique `(comment_id, user_id)`) and adds
`like_count int NOT NULL DEFAULT 0` to `article_comments`. No backfill.

---

## 3.5 `ArticleCommentDto` — like fields

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleCommentDto.cs`

**Append** (after the Phase 1/2 fields):

```csharp
/// <param name="LikeCount">
/// The cached number of likes on this comment.
/// </param>
/// <param name="IsLiked">
/// Whether the current viewer has liked this comment. False for anonymous viewers.
/// </param>
// ... appended to the record parameter list:
    int LikeCount = 0,
    bool IsLiked = false
```

---

## 3.6 Query — viewer + `IsLiked`

`PublicGetArticleCommentsQuery` gains a nullable viewer:

```csharp
/// <param name="ViewerUserId">The current viewer's user ID, or null for anonymous.</param>
public record PublicGetArticleCommentsQuery(
    Guid ArticleId,
    PaginatedRequest PaginatedRequest,
    Guid? ViewerUserId = null
) : IQuery<PublicGetArticleCommentsResult>;
```

The endpoint passes the authenticated user's ID when present (endpoint stays
`AllowAnonymous`; resolve the ID from the `ClaimsPrincipal` when authenticated).

Repository adds a batch viewer-like lookup:

```csharp
/// <summary>
/// Returns the subset of the given comment IDs that the viewer has liked.
/// </summary>
Task<IReadOnlySet<Guid>> GetLikedCommentIdsAsync(
    Guid viewerUserId,
    IReadOnlyCollection<Guid> commentIds,
    CancellationToken cancellationToken = default
);
```

Handler: after mapping (with authors), if `ViewerUserId` is non-null, resolve the liked set
in one query over the page's comment IDs and set `IsLiked` per comment; `LikeCount` comes
from the cached column. Anonymous → skip the query, all `false`.

---

## 3.7 Like / unlike command slices

**Like** — `.../Public/Commands/LikeArticleComment/`:
`PublicLikeArticleCommentCommand(Guid CommentId, Guid UserId)`. Handler: load comment; if
no existing like row for `(CommentId, UserId)`, add one and `IncrementLikeCount`; commit.
**Idempotent** — a second like is a no-op.

**Unlike** — `.../Public/Commands/UnlikeArticleComment/`:
`PublicUnlikeArticleCommentCommand(Guid CommentId, Guid UserId)`. Handler: if a like row
exists, remove it and `DecrementLikeCount`; commit. **Idempotent**.

Endpoints (both `RequireAuthorization`, `ContentBrowsing` rate limit):

| Method | Route |
|--------|-------|
| `POST` | `/api/v1/public/articles/comments/{commentId:guid}/like` |
| `DELETE` | `/api/v1/public/articles/comments/{commentId:guid}/like` |

---

## Tasks

- [ ] Add `ArticleCommentLikeEntity`.
- [ ] Add cached `LikeCount` + increment/decrement to `ArticleCommentEntity`.
- [ ] Add `ArticleCommentLikeConfiguration` (unique index); add `LikeCount` to comment config; add `DbSet`.
- [ ] Create `AddArticleCommentLikes` migration.
- [ ] Add `LikeCount`/`IsLiked` to `ArticleCommentDto`.
- [ ] Add `ViewerUserId` to the query; resolve `IsLiked` batched.
- [ ] Add like/unlike command slices (idempotent) + endpoints.
